using KENOS.Bot.Models;

namespace KENOS.Bot.Services;

public interface IKeyRepo
{
    Task<UserKey?> Get(long userId, CancellationToken ct = default);
    Task<UserKey>  Activate(long userId, string hwid, string plan, TimeSpan duration, CancellationToken ct = default);
    Task           Revoke(long userId, CancellationToken ct = default);
}
