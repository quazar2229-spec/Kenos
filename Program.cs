using KENOS.Bot.Handlers;
using KENOS.Bot.Models;
using KENOS.Bot.Services;
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

builder.Services.AddSingleton<Sessions>();
builder.Services.AddSingleton<Keyboards>();
builder.Services.AddSingleton<Changelog>();
builder.Services.AddSingleton<InfoService>();
builder.Services.AddSingleton<JsonBin>();
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
        "null"
    )
    .AllowAnyMethod()
    .AllowAnyHeader()));

var app = builder.Build();

app.UseCors();

app.MapGet("/api/ping", () => "pong");

await app.RunAsync();
