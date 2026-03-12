using System.Text.Json;
using KENOS.Bot.Models;

namespace KENOS.Bot.Services;

/// <summary>
/// In-memory хранилище объявлений для раздела «Инфо».
/// Данные автоматически синхронизируются в JSONBin при каждом изменении.
/// </summary>
public sealed class InfoService
{
    private readonly List<InfoEntry> _items = new();
    private readonly Lock _lock = new();

    // ── Чтение ────────────────────────────────────────────

    public IReadOnlyList<InfoEntry> All()
    {
        lock (_lock) return _items.AsReadOnly();
    }

    public int ActiveCount()
    {
        lock (_lock) return _items.Count(i => i.Active);
    }

    // ── Изменение ─────────────────────────────────────────

    /// <summary>Добавить новое объявление.</summary>
    public InfoEntry Add(string type, string title, string body)
    {
        var entry = new InfoEntry(
            Type   : type,
            Title  : title,
            Body   : body,
            Date   : DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
            Active : true);
        lock (_lock) _items.Insert(0, entry); // новые сверху
        return entry;
    }

    /// <summary>Закрыть объявление по индексу (1-based).</summary>
    public bool Close(int index)
    {
        lock (_lock)
        {
            if (index < 1 || index > _items.Count) return false;
            var old = _items[index - 1];
            _items[index - 1] = old with { Active = false };
            return true;
        }
    }

    /// <summary>Удалить объявление по индексу (1-based).</summary>
    public bool Remove(int index)
    {
        lock (_lock)
        {
            if (index < 1 || index > _items.Count) return false;
            _items.RemoveAt(index - 1);
            return true;
        }
    }

    /// <summary>Очистить все объявления.</summary>
    public void Clear()
    {
        lock (_lock) _items.Clear();
    }
}
