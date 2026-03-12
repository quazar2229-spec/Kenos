using System.Collections.Concurrent;
using KENOS.Bot.Models;

namespace KENOS.Bot.Services;

public sealed class MemoryKeyRepo : IKeyRepo
{
    private readonly ConcurrentDictionary<long, UserKey> _db = new();

    public Task<UserKey?> Get(long userId, CancellationToken ct = default)
    {
        _db.TryGetValue(userId, out var key);
        return Task.FromResult(key?.Active == true ? key : null);
    }

    public Task<UserKey> Activate(long userId, string hwid, string plan, TimeSpan duration, CancellationToken ct = default)
    {
        var key = new UserKey
        {
            UserId    = userId,
            KeyValue  = MakeKey(),
            Hwid      = hwid,
            Plan      = plan,
            ExpiresAt = DateTime.UtcNow.Add(duration)
        };
        _db[userId] = key;
        return Task.FromResult(key);
    }

    public Task Revoke(long userId, CancellationToken ct = default)
    {
        _db.TryRemove(userId, out _);
        return Task.CompletedTask;
    }

    private static string MakeKey()
    {
        static string Seg() => Convert.ToHexString(Guid.NewGuid().ToByteArray())[..4];
        return $"KENOS-{Seg()}-{Seg()}-{Seg()}-{Seg()}";
    }
}
