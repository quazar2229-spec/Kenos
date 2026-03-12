using System.Collections.Concurrent;
using KENOS.Bot.Models;

namespace KENOS.Bot.Services;

public sealed class Sessions
{
    private readonly ConcurrentDictionary<long, UserSession> _map = new();

    public State Get(long id) => _map.TryGetValue(id, out var s) ? s.State : State.Idle;

    public void Set(long id, State state) =>
        _map.AddOrUpdate(id, new UserSession { UserId = id, State = state },
            (_, s) => { s.State = state; return s; });

    public void Reset(long id) => Set(id, State.Idle);
}
