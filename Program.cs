using System.Text;
using KENOS.Bot.Handlers;
using KENOS.Bot.Models;
using KENOS.Bot.Services;
using Microsoft.Extensions.ObjectPool;
using Telegram.Bot;

Console.OutputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false) // reloadOnChange=false — нет FileWatcher на проде
    .AddEnvironmentVariables("KENOS_");

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o =>
{
    o.SingleLine      = true;
    o.TimestampFormat = "[HH:mm:ss] ";
});

var cfg = builder.Configuration.GetSection(BotConfig.Section).Get<BotConfig>()
    ?? throw new InvalidOperationException("appsettings.json: BotConfiguration missing");

if (string.IsNullOrWhiteSpace(cfg.BotToken))
    throw new InvalidOperationException("appsettings.json: BotToken is empty");

// ── Сервисы ────────────────────────────────────────────────────
builder.Services.Configure<BotConfig>(builder.Configuration.GetSection(BotConfig.Section));
builder.Services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(cfg.BotToken));

// MemoryCache для кэша валидации initData Telegram
// SizeLimit=1000 — не более 1000 уникальных initData в памяти одновременно
builder.Services.AddMemoryCache(o => { o.SizeLimit = 1_000; });

// ObjectPool<StringBuilder> — переиспользование буферов строк, нет GC-давления
builder.Services.AddSingleton<ObjectPool<StringBuilder>>(
    new DefaultObjectPool<StringBuilder>(
        new StringBuilderPooledObjectPolicy { InitialCapacity = 512, MaximumRetainedCapacity = 4096 },
        maximumRetained: 16));

builder.Services.AddSingleton<Sessions>();
builder.Services.AddSingleton<Keyboards>();
builder.Services.AddSingleton<Changelog>();
builder.Services.AddSingleton<InfoService>();
builder.Services.AddSingleton<JsonBin>();
builder.Services.AddSingleton<InitDataValidator>(); // кэш подписи TG initData
builder.Services.AddSingleton<IKeyRepo, MemoryKeyRepo>();

builder.Services.AddScoped<Support>();
builder.Services.AddScoped<MsgHandler>();
builder.Services.AddScoped<CbHandler>();

builder.Services.AddHostedService<BotWorker>();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(
        "https://quazar2229-spec.github.io",
        "https://kenos-production.up.railway.app",
        "http://localhost:3000",
        "http://localhost:5500",
        "null")
     .AllowAnyMethod()
     .AllowAnyHeader()));

var app = builder.Build();
app.UseCors();

// Healthcheck + initData validation endpoint
app.MapGet("/api/ping", () => "pong");

app.MapPost("/api/validate", (HttpContext ctx, InitDataValidator validator) =>
{
    if (!ctx.Request.Headers.TryGetValue("X-Telegram-Init-Data", out var raw))
        return Results.Unauthorized();

    return validator.Validate(raw.ToString())
        ? Results.Ok(new { ok = true })
        : Results.Unauthorized();
});

await app.RunAsync();
