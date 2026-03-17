using System.Collections.Concurrent;
using System.Text.Json;
using KENOS.Bot.Models;
using Microsoft.Extensions.Logging;

namespace KENOS.Bot.Services;

/// <summary>Хранит ключи в keys.json — не теряются при рестарте.</summary>
public sealed class FileKeyRepo : IKeyRepo
{
    private readonly ConcurrentDictionary<long, UserKey> _db = new();
    private readonly string _file;
    private readonly ILogger<FileKeyRepo> _log;
    private readonly object _lk = new();

    public FileKeyRepo(ILogger<FileKeyRepo> log)
    {
        _log  = log;
        _file = Path.Combine(AppContext.BaseDirectory, "keys.json");
        Load();
        _log.LogInformation("FileKeyRepo: загружено {N} ключей", _db.Count);
    }

    public Task<UserKey?> Get(long userId, CancellationToken ct = default)
    {
        _db.TryGetValue(userId, out var key);
        return Task.FromResult(key?.Active == true ? key : null);
    }

    public Task<UserKey> Activate(long userId, string hwid, string plan,
        TimeSpan duration, CancellationToken ct = default)
    {
        var key = new UserKey
        {
            UserId    = userId,
            KeyValue  = _db.TryGetValue(userId, out var ex) ? ex.KeyValue : MakeKey(),
            Hwid      = hwid,
            Plan      = plan,
            ExpiresAt = DateTime.UtcNow.Add(duration)
        };
        _db[userId] = key;
        Save();
        return Task.FromResult(key);
    }

    public Task Revoke(long userId, CancellationToken ct = default)
    {
        _db.TryRemove(userId, out _);
        Save();
        return Task.CompletedTask;
    }

    private static string MakeKey()
    {
        static string Seg() => Convert.ToHexString(Guid.NewGuid().ToByteArray())[..4];
        return $"KENOS-{Seg()}-{Seg()}-{Seg()}-{Seg()}";
    }

    private void Save()
    {
        try
        {
            var list = _db.Values.Select(k => new { k.UserId, k.KeyValue, k.Hwid, k.Plan, k.ExpiresAt }).ToList();
            var json = JsonSerializer.Serialize(list);
            lock (_lk) File.WriteAllText(_file, json);
        }
        catch (Exception ex) { _log.LogError(ex, "Не удалось сохранить keys.json"); }
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_file)) return;
            using var doc = JsonDocument.Parse(File.ReadAllText(_file));
            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var k = new UserKey
                {
                    UserId    = el.GetProperty("UserId").GetInt64(),
                    KeyValue  = el.GetProperty("KeyValue").GetString() ?? "",
                    Hwid      = el.GetProperty("Hwid").GetString() ?? "",
                    Plan      = el.GetProperty("Plan").GetString() ?? "",
                    ExpiresAt = el.GetProperty("ExpiresAt").GetDateTime()
                };
                _db[k.UserId] = k;
            }
        }
        catch (Exception ex) { _log.LogError(ex, "Не удалось загрузить keys.json"); }
    }
}
