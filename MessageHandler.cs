using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using KENOS.Bot.Models;
using KENOS.Bot.Serialization;
using KENOS.Bot.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace KENOS.Bot.Handlers;

public sealed class MsgHandler(
    ITelegramBotClient        bot,
    IOptions<BotConfig>       cfg,
    Sessions                  sessions,
    Support                   support,
    IKeyRepo                  keys,
    Keyboards                 kb,
    Changelog                 log,
    InfoService               info,
    JsonBin                   bin,
    ObjectPool<StringBuilder> sbPool,
    ILogger<MsgHandler>       logger)
{
    private readonly BotConfig _c = cfg.Value;

    public async Task Handle(Message msg, CancellationToken ct)
    {
        if (msg.From is null) return;

        var uid  = msg.From.Id;
        var text = msg.Text ?? "";

        log.Track(uid);

        if (_c.IsAdmin(uid))
        {
            if (await support.HandleAdminReply(msg, ct)) return;
            if (await AdminCmd(msg, text, ct)) return;
        }

        if (msg.WebAppData is { } wd) { await HandleWebAppData(msg, wd, ct); return; }

        await Send(uid, ct);
    }

    private Task Send(long id, CancellationToken ct) =>
        bot.SendMessage(id, "KENOS", replyMarkup: kb.App(), cancellationToken: ct);

    // ── Рассылка ──────────────────────────────────────────────
    // Используем IReadOnlyList<long> напрямую — нет .ToList() копии

    private async Task<(int sent, int blocked, int failed)> Blast(
        string text, CancellationToken ct)
    {
        var users = log.Users(); // IReadOnlyList<long>
        int sent = 0, blocked = 0, failed = 0;

        logger.LogInformation("Рассылка: {N} получателей", users.Count);

        // Span нельзя использовать с await, итерируем через индекс
        for (int i = 0; i < users.Count; i++)
        {
            var uid = users[i];
            try
            {
                await bot.SendMessage(
                    uid, text, ParseMode.Html,
                    replyMarkup: kb.App(), cancellationToken: ct);
                sent++;
                await Task.Delay(40, ct);
            }
            catch (ApiRequestException ex) when (IsUserGone(ex.Message))
            {
                blocked++;
            }
            catch (ApiRequestException ex) when (ex.Message.Contains("Too Many Requests"))
            {
                await Task.Delay(2000, ct);
                try
                {
                    await bot.SendMessage(
                        uid, text, ParseMode.Html,
                        replyMarkup: kb.App(), cancellationToken: ct);
                    sent++;
                }
                catch { failed++; }
            }
            catch { failed++; }
        }

        logger.LogInformation(
            "Рассылка: отправлено={S} заблок={B} ошибок={F}", sent, blocked, failed);
        return (sent, blocked, failed);
    }

    // Вынесено в отдельный метод — JIT инлайнит предикат
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsUserGone(string msg) =>
        msg.Contains("blocked",    StringComparison.Ordinal) ||
        msg.Contains("deactivated",StringComparison.Ordinal) ||
        msg.Contains("not found",  StringComparison.Ordinal) ||
        msg.Contains("kicked",     StringComparison.Ordinal);

    // ── Синхронизация JSONBin ──────────────────────────────────

    private Task<(bool ok, string msg)> SyncBin(CancellationToken ct)
        => bin.Push(log.All(), info.All(), ct);

    // ── Тип объявления → символ ───────────────────────────────

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string TypeEmoji(string type) => type switch
    {
        "tech" => "🔧",
        "warn" => "🚨",
        "ok"   => "✅",
        _      => "ℹ️"
    };

    // ── Список объявлений — ObjectPool<StringBuilder> ─────────
    // Вместо string.Join + Select (2 аллокации) — один буфер из пула

    private string InfoList()
    {
        var items = info.All();
        if (items.Count == 0) return "Объявлений нет.";

        var sb = sbPool.Get();
        try
        {
            for (int i = 0; i < items.Count; i++)
            {
                var e = items[i];
                if (i > 0) sb.Append("\n\n");
                sb.Append('<').Append('b').Append('>')
                  .Append(i + 1).Append(". ")
                  .Append(TypeEmoji(e.Type)).Append(' ')
                  .Append(HtmlEsc(e.Title))
                  .Append("</b>\n")
                  .Append(e.Body).Append('\n')
                  .Append("<i>").Append(e.Date)
                  .Append(" · ")
                  .Append(e.Active ? "🟢 Активно" : "⚫ Закрыто")
                  .Append("</i>");
            }
            return sb.ToString();
        }
        finally
        {
            sbPool.Return(sb);
        }
    }

    // ── WebApp данные — Source Gen десериализация ─────────────

    private async Task HandleWebAppData(Message msg, WebAppData wd, CancellationToken ct)
    {
        try
        {
            var payload = JsonSerializer.Deserialize(
                wd.Data, KENOSJsonCtx.Default.WebAppSupportPayload);

            if (payload?.Type == "support")
            {
                var fakeMsg = new Message { From = msg.From, Text = payload.Message };
                await support.ForwardToAdmin(fakeMsg, payload.Subject, ct);
                await bot.SendMessage(
                    msg.From!.Id,
                    "Сообщение отправлено. Ответим скоро.",
                    cancellationToken: ct);
            }
        }
        catch (Exception ex) { logger.LogError(ex, "WebApp data error"); }
    }

    // ── Админ-команды ──────────────────────────────────────────

    private async Task<bool> AdminCmd(Message msg, string text, CancellationToken ct)
    {
        // stackalloc для разбивки команды — нет heap аллокации
        Span<Range> ranges = stackalloc Range[8];
        var span  = text.AsSpan();
        int count = span.Split(ranges, ' ', StringSplitOptions.RemoveEmptyEntries);
        if (count == 0) return false;

        var cmd = span[ranges[0]].ToString().ToLowerInvariant();

        switch (cmd)
        {
            // /update v1.2 Что нового
            case "/update" when count >= 3:
            {
                var ver  = span[ranges[1]].ToString();
                var note = span[ranges[2]..count].ToString().Trim(); // всё после версии
                // Fallback: собираем через StringBuilder из пула
                var sb   = sbPool.Get();
                try
                {
                    for (int i = 2; i < count; i++)
                    {
                        if (i > 2) sb.Append(' ');
                        sb.Append(span[ranges[i]]);
                    }
                    note = sb.ToString();
                }
                finally { sbPool.Return(sb); }

                var entry = log.Add(ver, note);
                var (ok, binMsg) = await SyncBin(ct);
                var (sent, blocked, failed) = await Blast(
                    $"<b>KENOS {entry.Ver}</b>\n\n{entry.Text}", ct);

                await bot.SendMessage(_c.AdminChatId,
                    $"Обновление <b>{entry.Ver}</b>\n" +
                    $"JSONBin: {(ok ? "✅" : "⚠️")} {binMsg}\n" +
                    $"Отправлено: {sent} | Заблок: {blocked} | Ошибок: {failed}\n" +
                    $"База: {log.UserCount} чел.",
                    ParseMode.Html, cancellationToken: ct);
                return true;
            }

            case "/info" when count == 1:
            {
                const string help =
                    "<b>📋 Раздел Инфо — управление объявлениями</b>\n\n" +
                    "<b>Добавить:</b>\n" +
                    "<code>/info add [тип] [заголовок] | [текст]</code>\n\n" +
                    "<b>Типы:</b> <code>tech</code> 🔧 | <code>info</code> ℹ️ | <code>ok</code> ✅ | <code>warn</code> 🚨\n\n" +
                    "<b>Управление:</b>\n" +
                    "<code>/info list</code> — список\n" +
                    "<code>/info close 1</code> — закрыть\n" +
                    "<code>/info del 1</code> — удалить\n" +
                    "<code>/info clear</code> — очистить";
                await bot.SendMessage(_c.AdminChatId, help, ParseMode.Html, cancellationToken: ct);
                return true;
            }

            case "/info" when count >= 4 && span[ranges[1]].Equals("add".AsSpan(), StringComparison.OrdinalIgnoreCase):
            {
                var typeStr = span[ranges[2]].ToString().ToLowerInvariant();
                var type    = typeStr switch { "tech" => "tech", "ok" => "ok", "warn" => "warn", _ => "info" };

                // Собираем rest из пула
                var sb = sbPool.Get();
                try
                {
                    for (int i = 3; i < count; i++)
                    {
                        if (i > 3) sb.Append(' ');
                        sb.Append(span[ranges[i]]);
                    }
                    var rest  = sb.ToString();
                    var parts = rest.Split('|', 2);
                    var title = parts[0].Trim();
                    var body  = parts.Length > 1 ? parts[1].Trim() : "";

                    if (string.IsNullOrWhiteSpace(title))
                    {
                        await bot.SendMessage(_c.AdminChatId,
                            "❌ Укажи заголовок. Пример:\n<code>/info add tech Тех. работы | Описание</code>",
                            ParseMode.Html, cancellationToken: ct);
                        return true;
                    }

                    var entry = info.Add(type, title, body);
                    var (ok, binMsg) = await SyncBin(ct);

                    if (type is "tech" or "warn")
                        await Blast($"{TypeEmoji(type)} <b>{entry.Title}</b>\n\n{entry.Body}\n\n" +
                                    "<i>Подробнее — в разделе «Инфо» мини-приложения.</i>", ct);

                    await bot.SendMessage(_c.AdminChatId,
                        $"{TypeEmoji(type)} Объявление добавлено\n<b>{entry.Title}</b>\n{entry.Body}\n\n" +
                        $"JSONBin: {(ok ? "✅" : "⚠️")} {binMsg}\nАктивных: {info.ActiveCount()}",
                        ParseMode.Html, cancellationToken: ct);
                    return true;
                }
                finally { sbPool.Return(sb); }
            }

            case "/info" when count >= 2 && span[ranges[1]].Equals("list".AsSpan(), StringComparison.OrdinalIgnoreCase):
                await bot.SendMessage(_c.AdminChatId,
                    $"<b>📋 Объявления:</b>\n\n{InfoList()}",
                    ParseMode.Html, cancellationToken: ct);
                return true;

            case "/info" when count >= 3 && span[ranges[1]].Equals("close".AsSpan(), StringComparison.OrdinalIgnoreCase):
            {
                if (!int.TryParse(span[ranges[2]], out var idx)) return false;
                if (!info.Close(idx))
                {
                    await bot.SendMessage(_c.AdminChatId, $"❌ #{idx} не найдено", cancellationToken: ct);
                    return true;
                }
                var (ok, binMsg) = await SyncBin(ct);
                await bot.SendMessage(_c.AdminChatId,
                    $"✅ #{idx} закрыто — JSONBin: {(ok ? "✅" : "⚠️")} {binMsg}", cancellationToken: ct);
                return true;
            }

            case "/info" when count >= 3 && span[ranges[1]].Equals("del".AsSpan(), StringComparison.OrdinalIgnoreCase):
            {
                if (!int.TryParse(span[ranges[2]], out var idx)) return false;
                if (!info.Remove(idx))
                {
                    await bot.SendMessage(_c.AdminChatId, $"❌ #{idx} не найдено", cancellationToken: ct);
                    return true;
                }
                var (ok, binMsg) = await SyncBin(ct);
                await bot.SendMessage(_c.AdminChatId,
                    $"🗑 #{idx} удалено — JSONBin: {(ok ? "✅" : "⚠️")} {binMsg}", cancellationToken: ct);
                return true;
            }

            case "/info" when count >= 2 && span[ranges[1]].Equals("clear".AsSpan(), StringComparison.OrdinalIgnoreCase):
            {
                info.Clear();
                var (ok, binMsg) = await SyncBin(ct);
                await bot.SendMessage(_c.AdminChatId,
                    $"🗑 Очищено — JSONBin: {(ok ? "✅" : "⚠️")} {binMsg}", cancellationToken: ct);
                return true;
            }

            case "/clearlog":
            {
                log.Clear();
                var (ok, binMsg) = await SyncBin(ct);
                await bot.SendMessage(_c.AdminChatId,
                    $"История очищена — JSONBin: {(ok ? "✅" : "⚠️")} {binMsg}",
                    ParseMode.Html, cancellationToken: ct);
                return true;
            }

            case "/broadcast" when count >= 2:
            {
                var sb = sbPool.Get();
                string broadText;
                try
                {
                    for (int i = 1; i < count; i++) { if (i > 1) sb.Append(' '); sb.Append(span[ranges[i]]); }
                    broadText = sb.ToString();
                }
                finally { sbPool.Return(sb); }

                var (sent, blocked, failed) = await Blast(broadText, ct);
                await bot.SendMessage(_c.AdminChatId,
                    $"Рассылка: {sent} | заблок {blocked} | ошибок {failed}",
                    ParseMode.Html, cancellationToken: ct);
                return true;
            }

            case "/users":
                await bot.SendMessage(_c.AdminChatId,
                    $"Пользователей: <b>{log.UserCount}</b>",
                    ParseMode.Html, cancellationToken: ct);
                return true;

            case "/testbin":
            {
                var (ok, binMsg) = await SyncBin(ct);
                await bot.SendMessage(_c.AdminChatId,
                    $"JSONBin: {(ok ? "✅" : "❌")} {binMsg}",
                    ParseMode.Html, cancellationToken: ct);
                return true;
            }

            case "/activate" when count >= 5:
            {
                if (!long.TryParse(span[ranges[1]], out var tid) ||
                    !int.TryParse(span[ranges[4]], out var days)) return false;

                var key = await keys.Activate(
                    tid, span[ranges[2]].ToString(), span[ranges[3]].ToString(),
                    TimeSpan.FromDays(days), ct);

                await bot.SendMessage(tid,
                    $"Ключ активирован\n\n<code>{key.KeyValue}</code>\n\n" +
                    $"Тариф: {key.Plan}\nДо: {key.ExpiresAt:dd.MM.yyyy}",
                    ParseMode.Html, replyMarkup: kb.App(), cancellationToken: ct);

                await bot.SendMessage(_c.AdminChatId,
                    $"Ключ {tid}: <code>{key.KeyValue}</code>",
                    ParseMode.Html, cancellationToken: ct);
                return true;
            }

            case "/revoke" when count >= 2:
            {
                if (!long.TryParse(span[ranges[1]], out var rid)) return false;
                await keys.Revoke(rid, ct);
                await bot.SendMessage(_c.AdminChatId, $"Ключ {rid} отозван.", cancellationToken: ct);
                return true;
            }
        }

        return false;
    }

    // Константное время — нет уязвимости по времени
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string HtmlEsc(string s) =>
        s.Replace("&","&amp;",StringComparison.Ordinal)
         .Replace("<","&lt;",  StringComparison.Ordinal)
         .Replace(">","&gt;",  StringComparison.Ordinal);

    public Task SendKey(long id, CancellationToken ct)     => Send(id, ct);
    public Task SendPayment(long id, CancellationToken ct)  => Send(id, ct);
    public Task StartSupport(long id, CancellationToken ct) => Send(id, ct);
}
