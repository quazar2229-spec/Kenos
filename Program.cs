using KENOS.Bot.Data;
using KENOS.Bot.Handlers;
using KENOS.Bot.Models;
using KENOS.Bot.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables("KENOS_");

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o => { o.SingleLine = true; o.TimestampFormat = "[HH:mm:ss] "; });

var cfg = builder.Configuration.GetSection(BotConfig.Section).Get<BotConfig>()
    ?? throw new Exception("appsettings.json: BotConfiguration missing");

if (string.IsNullOrWhiteSpace(cfg.BotToken))
    throw new Exception("appsettings.json: BotToken is empty");

// ── Сервисы ───────────────────────────────────────────────
builder.Services.Configure<BotConfig>(builder.Configuration.GetSection(BotConfig.Section));
builder.Services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(cfg.BotToken));

// SQLite
builder.Services.AddDbContextFactory<AppDbContext>(o =>
    o.UseSqlite("Data Source=tickets.db"));

builder.Services.AddSingleton<Sessions>();
builder.Services.AddSingleton<Keyboards>();
builder.Services.AddSingleton<Changelog>();
builder.Services.AddSingleton<InfoService>();
builder.Services.AddSingleton<JsonBin>();
builder.Services.AddSingleton<IKeyRepo, MemoryKeyRepo>();
builder.Services.AddSingleton<TicketService>();

builder.Services.AddScoped<Support>();
builder.Services.AddScoped<MsgHandler>();
builder.Services.AddScoped<CbHandler>();

builder.Services.AddHostedService<BotWorker>();

// CORS — разрешаем GitHub Pages и localhost
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(
        "https://quazar2229-spec.github.io",
        "https://kenos-production.up.railway.app",
        "http://localhost:3000",
        "http://localhost:5500",
        "null"
    )
    .AllowAnyMethod()
    .AllowAnyHeader()));

var app = builder.Build();

// ── Авто-миграция БД ─────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var ctx = await db.CreateDbContextAsync();
    await ctx.Database.EnsureCreatedAsync();
}

app.UseCors();

// ════════════════════════════════════════════════════════
//  REST API
// ════════════════════════════════════════════════════════
var api = app.MapGroup("/api");

// ── Проверка ──────────────────────────────────────────────
api.MapGet("/ping", () => "pong");

// ── Создать тикет ─────────────────────────────────────────
api.MapPost("/tickets", async (CreateTicketRequest req, TicketService svc) =>
{
    if (string.IsNullOrWhiteSpace(req.Message)) return Results.BadRequest("empty message");
    var ticket = await svc.CreateAsync(req.UserId, req.Username, req.Subject, req.Message);
    return Results.Ok(new { ticket.Id, ticket.Subject, ticket.Status });
});

// ── Тикеты пользователя ───────────────────────────────────
api.MapGet("/tickets/user/{userId:long}", async (long userId, TicketService svc) =>
{
    var list = await svc.GetUserTicketsAsync(userId);
    return Results.Ok(list.Select(t => new
    {
        t.Id, t.Subject,
        status = t.Status.ToString().ToLower(),
        t.CreatedAt
    }));
});

// ── Сообщения тикета (пользователь) ──────────────────────
api.MapGet("/tickets/{id:int}/messages", async (int id, long userId, TicketService svc) =>
{
    var msgs = await svc.GetMessagesAsync(id, userId);
    return Results.Ok(msgs.Select(m => new
    {
        m.Id, m.FromAdmin, m.Sender, m.Text,
        sentAt = m.SentAt
    }));
});

// ── Сообщение от пользователя ─────────────────────────────
api.MapPost("/tickets/{id:int}/messages", async (int id, UserMessageRequest req, TicketService svc, ITelegramBotClient bot, Microsoft.Extensions.Options.IOptions<BotConfig> cfgOpt) =>
{
    var msg = await svc.UserReplyAsync(id, req.UserId, req.Text);
    if (msg is null) return Results.BadRequest("ticket not found or closed");

    // уведомляем всех админов
    var ticket = await svc.GetAsync(id);
    var notif = $"💬 <b>Тикет #{id}</b> — {ticket?.Username}\n\n{req.Text}";
    foreach (var adminId in cfgOpt.Value.AdminIds)
        try { await bot.SendMessage(adminId, notif, Telegram.Bot.Types.Enums.ParseMode.Html); } catch { }

    return Results.Ok(new { msg.Id, msg.Text, msg.SentAt });
});

// ══════════════════════════════
//  ADMIN API (проверка adminId)
// ══════════════════════════════
var admin = api.MapGroup("/admin").AddEndpointFilter(async (ctx, next) =>
{
    if (!ctx.HttpContext.Request.Query.TryGetValue("adminId", out var raw) ||
        !long.TryParse(raw, out var id)) return Results.Unauthorized();

    var botCfg = ctx.HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptions<BotConfig>>().Value;
    if (!botCfg.AdminIds.Contains(id)) return Results.Forbid();
    return await next(ctx);
});

// Все открытые тикеты
admin.MapGet("/tickets", async (TicketService svc) =>
{
    var list = await svc.GetAllOpenAsync();
    return Results.Ok(list.Select(t => new
    {
        t.Id, t.UserId, t.Username, t.Subject,
        status = t.Status.ToString().ToLower(),
        t.CreatedAt
    }));
});

// Сообщения любого тикета (для админа)
admin.MapGet("/tickets/{id:int}/messages", async (int id, TicketService svc) =>
{
    var msgs = await svc.GetMessagesAsync(id);
    return Results.Ok(msgs.Select(m => new
    {
        m.Id, m.FromAdmin, m.Sender, m.Text,
        sentAt = m.SentAt
    }));
});

// Ответ администратора
admin.MapPost("/tickets/{id:int}/reply", async (int id, AdminReplyRequest req, TicketService svc, ITelegramBotClient bot) =>
{
    var msg = await svc.AdminReplyAsync(id, req.AdminName, req.Text);
    if (msg is null) return Results.NotFound();

    // уведомляем пользователя в Telegram
    var ticket = await svc.GetAsync(id);
    if (ticket is not null)
        try
        {
            await bot.SendMessage(ticket.UserId,
                $"📩 <b>Ответ по тикету #{id}</b>\n\n{req.Text}\n\n<i>— {req.AdminName}</i>",
                Telegram.Bot.Types.Enums.ParseMode.Html);
        }
        catch { }

    return Results.Ok(new { msg.Id, msg.Text, msg.SentAt });
});

// Закрыть тикет
admin.MapPost("/tickets/{id:int}/close", async (int id, TicketService svc, ITelegramBotClient bot) =>
{
    var ok = await svc.CloseAsync(id);
    if (!ok) return Results.NotFound();

    var ticket = await svc.GetAsync(id);
    if (ticket is not null)
        try { await bot.SendMessage(ticket.UserId, $"✅ Тикет <b>#{id}</b> закрыт. Спасибо за обращение!", Telegram.Bot.Types.Enums.ParseMode.Html); } catch { }

    return Results.Ok();
});

await app.RunAsync();

// ── Request DTOs ──────────────────────────────────────────
record CreateTicketRequest(long UserId, string Username, string Subject, string Message);
record UserMessageRequest(long UserId, string Text);
record AdminReplyRequest(string AdminName, string Text);
