using System.Collections.Concurrent;
using System.Text.Json;
using KENOS.Bot.Serialization;
using Microsoft.Extensions.Logging;

namespace KENOS.Bot.Services;

public record Entry(string Ver, string Text, DateTime Date);

public sealed class Changelog
{
    private readonly List<Entry> _entries = new();
    private readonly ConcurrentDictionary<long, byte> _users = new();
    private readonly string _usersFile;
    private readonly ILogger<Changelog> _log;
    private readonly object _lk = new();

    public int Version { get; private set; } = 1;

    public Changelog(ILogger<Changelog> log)
    {
        _log = log;
        _usersFile = Path.Combine(AppContext.BaseDirectory, "users.json");
        LoadUsers();
        _log.LogInformation("ChangelogService: загружено {N} пользователей", _users.Count);
    }

    public void Track(long id)
    {
        if (_users.TryAdd(id, 0)) SaveUsers();
    }

    public Entry Add(string ver, string text)
    {
        var e = new Entry(ver, text, DateTime.UtcNow);
        lock (_lk)
        {
            _entries.Insert(0, e);
            if (_entries.Count > 30) _entries.RemoveAt(_entries.Count - 1);
            Version++;
        }
        return e;
    }

    public void Clear()
    {
        lock (_lk) { _entries.Clear(); Version++; }
    }

    public IReadOnlyList<Entry> All()  { lock (_lk) return _entries.ToList(); }
    public IReadOnlyList<long>  Users() => _users.Keys.ToList();
    public int UserCount => _users.Count;

    // ── Source Gen JSON — нет рефлексии, AOT/trim-safe ────────

    private void SaveUsers()
    {
        try
        {
            var ids  = _users.Keys.ToList();
            // Используем Source Generator — KENOSJsonCtx зарегистрирован для List<long>
            var json = JsonSerializer.Serialize(ids, KENOSJsonCtx.Default.ListInt64);
            lock (_lk) File.WriteAllText(_usersFile, json);
        }
        catch (Exception ex) { _log.LogError(ex, "Не удалось сохранить users.json"); }
    }

    private void LoadUsers()
    {
        try
        {
            if (!File.Exists(_usersFile)) return;
            var json = File.ReadAllText(_usersFile);
            // Source Gen десериализация — нет IL2026/IL3050
            var ids  = JsonSerializer.Deserialize(json, KENOSJsonCtx.Default.ListInt64);
            foreach (var id in ids ?? []) _users.TryAdd(id, 0);
        }
        catch (Exception ex) { _log.LogError(ex, "Не удалось загрузить users.json"); }
    }
}
