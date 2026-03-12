using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using KENOS.Bot.Models;

namespace KENOS.Bot.Services;

/// <summary>
/// JSONBin — хранилище для мини-приложения.
/// Структура: { "changelog": [...], "info": [...] }
/// </summary>
public sealed class JsonBin(IOptions<BotConfig> cfg, ILogger<JsonBin> log)
{
    private static readonly HttpClient Http = new();

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
            var payload = JsonSerializer.Serialize(new
            {
                changelog = changelog.Select(e => new
                {
                    ver  = e.Ver,
                    date = e.Date.ToString("dd.MM.yyyy"),
                    text = e.Text
                }),
                info = info.Select(i => new
                {
                    type   = i.Type,
                    title  = i.Title,
                    body   = i.Body,
                    date   = i.Date,
                    active = i.Active
                })
            });

            var req = new HttpRequestMessage(HttpMethod.Put,
                $"https://api.jsonbin.io/v3/b/{c.JsonBinId}")
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            req.Headers.Add("X-Master-Key",    c.JsonBinKey);
            req.Headers.Add("X-Bin-Versioning","false");

            var res  = await Http.SendAsync(req, ct);
            var text = await res.Content.ReadAsStringAsync(ct);
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
