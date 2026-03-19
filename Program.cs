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
builder.Services.AddSingleton<IKeyRepo, FileKeyRepo>();

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

// ── /api/key — получить ключ пользователя ─────────────────
app.MapPost("/api/key", async (HttpContext ctx, IKeyRepo keys, InitDataValidator validator) =>
{
    var body = await new StreamReader(ctx.Request.Body).ReadToEndAsync();
    System.Text.Json.JsonElement root;
    try { root = System.Text.Json.JsonDocument.Parse(body).RootElement; }
    catch { return Results.BadRequest(); }

    var initData = root.TryGetProperty("initData", out var d) ? d.GetString() ?? "" : "";
    if (!validator.Validate(initData)) return Results.Unauthorized();

    long userId = 0;
    foreach (var pair in initData.Split('&'))
    {
        var idx = pair.IndexOf('=');
        if (idx < 0 || pair[..idx] != "user") continue;
        try { var u = System.Text.Json.JsonDocument.Parse(Uri.UnescapeDataString(pair[(idx+1)..])).RootElement; userId = u.TryGetProperty("id", out var id) ? id.GetInt64() : 0; } catch { }
    }
    if (userId == 0) return Results.Unauthorized();

    var key = await keys.Get(userId);
    if (key is null) return Results.Ok(new { active = false });
    return Results.Ok(new { active = true, key = key.KeyValue, hwid = key.Hwid, plan = key.Plan, expires = key.ExpiresAt.ToString("dd.MM.yyyy") });
});

// ── /api/ai — прокси к Anthropic ──────────────────────────
app.MapPost("/api/ai", async (HttpContext ctx) =>
{
    var body = await new StreamReader(ctx.Request.Body).ReadToEndAsync();
    System.Text.Json.JsonElement root;
    try { root = System.Text.Json.JsonDocument.Parse(body).RootElement; }
    catch { return Results.BadRequest(); }

    var lang = root.TryGetProperty("lang", out var l) ? l.GetString() : "ru";
    var system = lang == "en"
        ? "You are KENOS assistant for BlueStacks setup in Standoff 2. Answer briefly in English."
        : "Ты — помощник KENOS, сервиса настройки BlueStacks для Standoff 2. Отвечай кратко, по делу, на русском.";

    // Читаем ключ напрямую из окружения
    var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ?? "";
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        // Дебаг — показываем все переменные содержащие ANTHROP
        var envKeys = Environment.GetEnvironmentVariables().Keys
            .Cast<string>()
            .Where(k => k.Contains("ANTHROP", StringComparison.OrdinalIgnoreCase))
            .ToList();
        var hint = envKeys.Any() ? string.Join(",", envKeys) : "не найдено";
        return Results.Json(new { reply = $"API ключ не найден. Переменные: {hint}" });
    }

    using var http = new HttpClient();
    http.DefaultRequestHeaders.Add("x-api-key", apiKey);
    http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

    var messages = root.TryGetProperty("messages", out var m) ? m.GetRawText() : "[]";
    var payload = $"{{\"model\":\"claude-haiku-4-5-20251001\",\"max_tokens\":800,\"system\":{System.Text.Json.JsonSerializer.Serialize(system)},\"messages\":{messages}}}";

    var resp = await http.PostAsync("https://api.anthropic.com/v1/messages",
        new StringContent(payload, System.Text.Encoding.UTF8, "application/json"));
    var result = await resp.Content.ReadAsStringAsync();

    try
    {
        var doc = System.Text.Json.JsonDocument.Parse(result);
        var reply = doc.RootElement.TryGetProperty("content", out var cont) && cont.GetArrayLength() > 0
            ? cont[0].GetProperty("text").GetString() : "Нет ответа";
        return Results.Json(new { reply });
    }
    catch { return Results.Json(new { reply = "Ошибка ответа от AI" }); }
});

await app.RunAsync();
