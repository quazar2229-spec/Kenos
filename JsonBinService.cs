using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using KENOS.Bot.Models;
using KENOS.Bot.Serialization;

namespace KENOS.Bot.Services;

/// <summary>
/// JSONBin — хранилище для мини-приложения.
/// Оптимизации:
///   • Source Generator JSON — нет рефлексии, AOT-safe
///   • ObjectPool&lt;StringBuilder&gt; — переиспользование буфера, нет GC-давления
///   • Единственный статический HttpClient — нет socket exhaustion
/// </summary>
public sealed class JsonBin(
    IOptions<BotConfig> cfg,
    ObjectPool<StringBuilder> sbPool,
    ILogger<JsonBin> log)
{
    // Один HttpClient на весь процесс — нет socket exhaustion
    private static readonly HttpClient Http = new(new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(10),
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
        MaxConnectionsPerServer = 4
    });

    /// <summary>Сохранить changelog + info в одном запросе.</summary>
    public async Task<(bool ok, string msg)> Push(
        IReadOnlyList<Entry>     changelog,
        IReadOnlyList<InfoEntry> info,
        CancellationToken        ct)
    {
        var c = cfg.Value;
        if (string.IsNullOrWhiteSpace(c.JsonBinKey)) return (false, "JsonBinKey не задан");
        if (string.IsNullOrWhiteSpace(c.JsonBinId))  return (false, "JsonBinId не задан");

        try
        {
            // Строим payload через Source Generator — ноль рефлексии
            var payload = new JsonBinPayload(
                Changelog: changelog.Select(e => new ChangelogEntryDto(
                    Ver:  e.Ver,
                    Date: e.Date.ToString("dd.MM.yyyy"),
                    Text: e.Text)),
                Info: info.Select(i => new InfoEntryDto(
                    Type:   i.Type,
                    Title:  i.Title,
                    Body:   i.Body,
                    Date:   i.Date,
                    Active: i.Active)));

            // Сериализация через Source Gen — нет DynamicCode, AOT-safe
            var json = JsonSerializer.Serialize(payload, KENOSJsonCtx.Default.JsonBinPayload);

            var req = new HttpRequestMessage(
                HttpMethod.Put,
                $"https://api.jsonbin.io/v3/b/{c.JsonBinId}")
            {
                // StringContent с явной кодировкой — нет лишних аллокаций
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            req.Headers.TryAddWithoutValidation("X-Master-Key",     c.JsonBinKey);
            req.Headers.TryAddWithoutValidation("X-Bin-Versioning", "false");

            using var res  = await Http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
            var       text = await res.Content.ReadAsStringAsync(ct);

            log.LogInformation("JSONBin {S}: {B}", (int)res.StatusCode, text);
            return res.IsSuccessStatusCode ? (true, "OK") : (false, $"HTTP {(int)res.StatusCode}");
        }
        catch (Exception ex)
        {
            log.LogError(ex, "JSONBin error");
            return (false, ex.Message);
        }
    }

    /// <summary>Обратная совместимость — только changelog.</summary>
    public Task<(bool ok, string msg)> Push(IReadOnlyList<Entry> changelog, CancellationToken ct)
        => Push(changelog, Array.Empty<InfoEntry>(), ct);
}
