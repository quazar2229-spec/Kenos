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

// ── /api/aitest — проверка ключа ──────────────────────────
app.MapGet("/api/aitest", (IConfiguration config) =>
{
    var k1 = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ?? "";
    var k2 = Environment.GetEnvironmentVariable("KENOS_ANTHROPIC_API_KEY") ?? "";
    var k3 = config["ANTHROPIC_API_KEY"] ?? "";
    return Results.Json(new {
        env1 = k1.Length > 0 ? $"OK ({k1.Length} chars, starts: {k1[..8]})" : "EMPTY",
        env2 = k2.Length > 0 ? $"OK ({k2.Length} chars)" : "EMPTY",
        cfg  = k3.Length > 0 ? $"OK ({k3.Length} chars)" : "EMPTY",
    });
});

// ── /api/ai — прокси к Anthropic ──────────────────────────
app.MapPost("/api/ai", async (HttpContext ctx, IConfiguration config) =>
{
    var body = await new StreamReader(ctx.Request.Body).ReadToEndAsync();
    System.Text.Json.JsonElement root;
    try { root = System.Text.Json.JsonDocument.Parse(body).RootElement; }
    catch { return Results.BadRequest(); }

    var lang = root.TryGetProperty("lang", out var l) ? l.GetString() : "ru";
    var system = lang == "en"
        ? """
          You are KENOS AI — expert assistant for KENOS service.
          KENOS provides custom BlueStacks setups for Standoff 2.
          Prices: 800 RUB or 400 Stars/month. Contacts: @datadied (owner), @WinInput32 (dev).
          Answer in English, max 4 sentences, friendly and concise.
          """
        : """
          Ты — KENOS AI, помощник сервиса KENOS.
          KENOS — кастомная настройка BlueStacks для Standoff 2: лучший FPS, сенса, HWID-лицензия.
          Цены: 800 ₽ или 400 Stars в месяц. Контакты: @datadied (владелец), @WinInput32 (кодер).
          Отвечай на русском, максимум 4 предложения, дружелюбно и по делу.
          """;

    // API ключ
    var apiKey =
        Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
        ?? Environment.GetEnvironmentVariable("KENOS_ANTHROPIC_API_KEY")
        ?? config["ANTHROPIC_API_KEY"]
        ?? "";

    if (string.IsNullOrWhiteSpace(apiKey))
        return Results.Json(new { reply = lang == "en"
            ? "AI service is not configured yet. Contact @datadied."
            : "AI сервис ещё не настроен. Обратитесь к @datadied." });

    using var http = new HttpClient();
    http.DefaultRequestHeaders.Add("x-api-key", apiKey);
    http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

    var messages = root.TryGetProperty("messages", out var m) ? m.GetRawText() : "[]";
    var payload  = $$"""{"model":"claude-haiku-4-5-20251001","max_tokens":600,"system":{{System.Text.Json.JsonSerializer.Serialize(system)}},"messages":{{messages}}}""";

    try
    {
        var resp   = await http.PostAsync("https://api.anthropic.com/v1/messages",
            new StringContent(payload, System.Text.Encoding.UTF8, "application/json"));
        var result = await resp.Content.ReadAsStringAsync();

        // Логируем для отладки
        Console.WriteLine($"[AI] Status: {(int)resp.StatusCode}, Body: {result[..Math.Min(300, result.Length)]}");

        var doc = System.Text.Json.JsonDocument.Parse(result);
        var root2 = doc.RootElement;

        // Anthropic возвращает error объект если что-то не так
        if (root2.TryGetProperty("error", out var err))
        {
            var errMsg = err.TryGetProperty("message", out var em) ? em.GetString() : "API error";
            Console.WriteLine($"[AI] Error: {errMsg}");
            return Results.Json(new { reply = lang == "en" ? $"AI error: {errMsg}" : $"Ошибка AI: {errMsg}" });
        }

        string? reply = null;
        if (root2.TryGetProperty("content", out var cont) && cont.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            foreach (var item in cont.EnumerateArray())
            {
                if (item.TryGetProperty("type", out var t) && t.GetString() == "text"
                    && item.TryGetProperty("text", out var txt))
                {
                    reply = txt.GetString();
                    break;
                }
            }
        }

        return Results.Json(new { reply = reply ?? (lang == "en" ? "No response" : "Нет ответа") });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[AI] Exception: {ex.Message}");
        return Results.Json(new { reply = lang == "en" ? "Connection error" : "Ошибка соединения" });
    }
});

await app.RunAsync();
