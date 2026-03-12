using KENOS.Bot.Models;
using KENOS.Bot.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace KENOS.Bot.Handlers;

public sealed class MsgHandler(
    ITelegramBotClient bot,
    IOptions<BotConfig> cfg,
    Sessions sessions,
    Support support,
    IKeyRepo keys,
    Keyboards kb,
    Changelog log,
    InfoService info,
    TicketService tickets,
    JsonBin bin,
    ILogger<MsgHandler> logger)
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

        if (msg.WebAppData is { } wd) { await WebAppData(msg, wd, ct); return; }

        await Send(uid, ct);
    }

    private Task Send(long id, CancellationToken ct) =>
        bot.SendMessage(id, "KENOS", replyMarkup: kb.App(), cancellationToken: ct);

    // ── Рассылка ──────────────────────────────────────────

    private async Task<(int sent, int blocked, int failed)> Blast(string text, CancellationToken ct)
    {
        var users = log.Users();
        int sent = 0, blocked = 0, failed = 0;

        logger.LogInformation("Рассылка: {N} получателей", users.Count);

        foreach (var uid in users)
        {
            try
            {
                await bot.SendMessage(uid, text, ParseMode.Html, replyMarkup: kb.App(), cancellationToken: ct);
                sent++;
                await Task.Delay(40, ct);
            }
            catch (ApiRequestException ex) when (
                ex.Message.Contains("blocked") || ex.Message.Contains("deactivated") ||
                ex.Message.Contains("not found") || ex.Message.Contains("kicked"))
            {
                blocked++;
            }
            catch (ApiRequestException ex) when (ex.Message.Contains("Too Many Requests"))
            {
                await Task.Delay(2000, ct);
                try { await bot.SendMessage(uid, text, ParseMode.Html, replyMarkup: kb.App(), cancellationToken: ct); sent++; }
                catch { failed++; }
            }
            catch { failed++; }
        }

        logger.LogInformation("Рассылка: отправлено={S} заблок={B} ошибок={F}", sent, blocked, failed);
        return (sent, blocked, failed);
    }

    // ── Синхронизация JSONBin ─────────────────────────────

    private async Task<(bool ok, string msg)> SyncBin(CancellationToken ct)
        => await bin.Push(log.All(), info.All(), ct);

    // ── Тип объявления → emoji ────────────────────────────

    private static string TypeEmoji(string type) => type switch
    {
        "tech" => "🔧",
        "warn" => "🚨",
        "ok"   => "✅",
        _      => "ℹ️"
    };

    // ── Список объявлений для бота ────────────────────────

    private string InfoList()
    {
        var items = info.All();
        if (!items.Any()) return "Объявлений нет.";

        return string.Join("\n\n", items.Select((e, i) =>
            $"<b>{i + 1}. {TypeEmoji(e.Type)} {e.Title}</b>\n" +
            $"{e.Body}\n" +
            $"<i>{e.Date} · {(e.Active ? "🟢 Активно" : "⚫ Закрыто")}</i>"));
    }

    // ── Админ-команды ─────────────────────────────────────

    private async Task<bool> AdminCmd(Message msg, string text, CancellationToken ct)
    {
        var p = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (p.Length == 0) return false;

        switch (p[0].ToLower())
        {
            // ─ /update v1.2 Что нового ────────────────────
            case "/update" when p.Length >= 3:
            {
                var entry = log.Add(p[1], string.Join(' ', p.Skip(2)));
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

            // ─ /info — справка ────────────────────────────
            case "/info" when p.Length == 1:
            {
                var help =
                    "<b>📋 Раздел Инфо — управление объявлениями</b>\n\n" +
                    "<b>Добавить:</b>\n" +
                    "<code>/info add [тип] [заголовок] | [текст]</code>\n\n" +
                    "<b>Типы:</b> <code>tech</code> 🔧 | <code>info</code> ℹ️ | <code>ok</code> ✅ | <code>warn</code> 🚨\n\n" +
                    "<b>Примеры:</b>\n" +
                    "<code>/info add tech Тех. работы | Ведётся обновление ключей. Ожидайте 30 мин.</code>\n" +
                    "<code>/info add ok Готово | Всё работает в штатном режиме.</code>\n" +
                    "<code>/info add warn Важно | С 00:00 поднимаем цену до 900р.</code>\n\n" +
                    "<b>Управление:</b>\n" +
                    "<code>/info list</code> — список всех\n" +
                    "<code>/info close 1</code> — закрыть #1\n" +
                    "<code>/info del 1</code> — удалить #1\n" +
                    "<code>/info clear</code> — очистить все";

                await bot.SendMessage(_c.AdminChatId, help, ParseMode.Html, cancellationToken: ct);
                return true;
            }

            // ─ /info add [type] [title] | [body] ──────────
            case "/info" when p.Length >= 4 && p[1].ToLower() == "add":
            {
                var type  = p[2].ToLower() switch { "tech" => "tech", "ok" => "ok", "warn" => "warn", _ => "info" };
                var rest  = string.Join(' ', p.Skip(3));
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

                // уведомить всех пользователей о тех. работах или важных объявлениях
                if (type is "tech" or "warn")
                {
                    await Blast(
                        $"{TypeEmoji(type)} <b>{entry.Title}</b>\n\n{entry.Body}\n\n" +
                        $"<i>Подробнее — в разделе «Инфо» мини-приложения.</i>", ct);
                }

                await bot.SendMessage(_c.AdminChatId,
                    $"{TypeEmoji(type)} Объявление добавлено\n" +
                    $"<b>{entry.Title}</b>\n{entry.Body}\n\n" +
                    $"JSONBin: {(ok ? "✅" : "⚠️")} {binMsg}\n" +
                    $"Активных: {info.ActiveCount()}",
                    ParseMode.Html, cancellationToken: ct);
                return true;
            }

            // ─ /info list ─────────────────────────────────
            case "/info" when p.Length >= 2 && p[1].ToLower() == "list":
            {
                await bot.SendMessage(_c.AdminChatId,
                    $"<b>📋 Объявления:</b>\n\n{InfoList()}",
                    ParseMode.Html, cancellationToken: ct);
                return true;
            }

            // ─ /info close N ──────────────────────────────
            case "/info" when p.Length >= 3 && p[1].ToLower() == "close":
            {
                if (!int.TryParse(p[2], out var idx)) return false;
                var closed = info.Close(idx);
                if (!closed) { await bot.SendMessage(_c.AdminChatId, $"❌ Объявление #{idx} не найдено", cancellationToken: ct); return true; }
                var (ok, binMsg) = await SyncBin(ct);
                await bot.SendMessage(_c.AdminChatId,
                    $"✅ Объявление #{idx} закрыто\nJSONBin: {(ok ? "✅" : "⚠️")} {binMsg}",
                    cancellationToken: ct);
                return true;
            }

            // ─ /info del N ────────────────────────────────
            case "/info" when p.Length >= 3 && p[1].ToLower() == "del":
            {
                if (!int.TryParse(p[2], out var idx)) return false;
                var removed = info.Remove(idx);
                if (!removed) { await bot.SendMessage(_c.AdminChatId, $"❌ Объявление #{idx} не найдено", cancellationToken: ct); return true; }
                var (ok, binMsg) = await SyncBin(ct);
                await bot.SendMessage(_c.AdminChatId,
                    $"🗑 Объявление #{idx} удалено\nJSONBin: {(ok ? "✅" : "⚠️")} {binMsg}",
                    cancellationToken: ct);
                return true;
            }

            // ─ /info clear ────────────────────────────────
            case "/info" when p.Length >= 2 && p[1].ToLower() == "clear":
            {
                info.Clear();
                var (ok, binMsg) = await SyncBin(ct);
                await bot.SendMessage(_c.AdminChatId,
                    $"🗑 Все объявления удалены\nJSONBin: {(ok ? "✅" : "⚠️")} {binMsg}",
                    cancellationToken: ct);
                return true;
            }

            // ─ /clearlog ──────────────────────────────────
            case "/clearlog":
            {
                log.Clear();
                var (ok, binMsg) = await SyncBin(ct);
                await bot.SendMessage(_c.AdminChatId,
                    $"История обновлений очищена.\nJSONBin: {(ok ? "✅" : "⚠️")} {binMsg}",
                    ParseMode.Html, cancellationToken: ct);
                return true;
            }

            // ─ /broadcast Текст ───────────────────────────
            case "/broadcast" when p.Length >= 2:
            {
                var (sent, blocked, failed) = await Blast(string.Join(' ', p.Skip(1)), ct);
                await bot.SendMessage(_c.AdminChatId,
                    $"Рассылка: отправлено {sent} | заблок {blocked} | ошибок {failed}",
                    ParseMode.Html, cancellationToken: ct);
                return true;
            }

            // ─ /users ─────────────────────────────────────
            case "/users":
                await bot.SendMessage(_c.AdminChatId,
                    $"Пользователей в базе: <b>{log.UserCount}</b>",
                    ParseMode.Html, cancellationToken: ct);
                return true;

            // ─ /testbin ───────────────────────────────────
            case "/testbin":
            {
                var (ok, binMsg) = await SyncBin(ct);
                await bot.SendMessage(_c.AdminChatId,
                    $"JSONBin: {(ok ? "✅" : "❌")} {binMsg}",
                    ParseMode.Html, cancellationToken: ct);
                return true;
            }

            // ─ /activate userId hwid plan days ────────────
            case "/activate" when p.Length >= 5:
            {
                if (!long.TryParse(p[1], out var tid) || !int.TryParse(p[4], out var days)) return false;
                var key = await keys.Activate(tid, p[2], p[3], TimeSpan.FromDays(days), ct);

                await bot.SendMessage(tid,
                    $"Ключ активирован\n\n<code>{key.KeyValue}</code>\n\n" +
                    $"Тариф: {key.Plan}\nДо: {key.ExpiresAt:dd.MM.yyyy}",
                    ParseMode.Html, replyMarkup: kb.App(), cancellationToken: ct);

                await bot.SendMessage(_c.AdminChatId,
                    $"Ключ для {tid}: <code>{key.KeyValue}</code>",
                    ParseMode.Html, cancellationToken: ct);
                return true;
            }

            // ─ /revoke userId ─────────────────────────────
            case "/revoke" when p.Length >= 2:
            {
                if (!long.TryParse(p[1], out var rid)) return false;
                await keys.Revoke(rid, ct);
                await bot.SendMessage(_c.AdminChatId,
                    $"Ключ {rid} отозван.", cancellationToken: ct);
                return true;
            }

            // ─ /tickets — список открытых тикетов ──────────
            case "/tickets":
            {
                var list = await tickets.GetAllOpenAsync();
                if (!list.Any())
                {
                    await bot.SendMessage(_c.AdminChatId, "📭 Открытых тикетов нет.", cancellationToken: ct);
                    return true;
                }
                var txt = "<b>📋 Открытые тикеты:</b>\n\n" +
                    string.Join("\n", list.Select(t =>
                        $"<b>#{t.Id}</b> — @{t.Username}\n" +
                        $"📌 {t.Subject}\n" +
                        $"🕐 {t.CreatedAt:dd.MM HH:mm} · {t.Status}\n"));
                await bot.SendMessage(_c.AdminChatId, txt, ParseMode.Html, cancellationToken: ct);
                return true;
            }

            // ─ /reply ticketId текст ────────────────────────
            case "/reply" when p.Length >= 3:
            {
                if (!int.TryParse(p[1], out var tid)) return false;
                var replyText = string.Join(' ', p.Skip(2));
                var adminName = msg.From?.Username ?? msg.From?.FirstName ?? "Администратор";
                var m = await tickets.AdminReplyAsync(tid, adminName, replyText);
                if (m is null)
                {
                    await bot.SendMessage(_c.AdminChatId, $"❌ Тикет #{tid} не найден.", cancellationToken: ct);
                    return true;
                }
                var ticket = await tickets.GetAsync(tid);
                // уведомить пользователя
                if (ticket is not null)
                    try
                    {
                        await bot.SendMessage(ticket.UserId,
                            $"📩 <b>Ответ по тикету #{tid}</b>\n\n{replyText}\n\n<i>— {adminName}</i>",
                            ParseMode.Html, replyMarkup: kb.App(), cancellationToken: ct);
                    }
                    catch { }

                await bot.SendMessage(_c.AdminChatId, $"✅ Ответ по тикету #{tid} отправлен.", cancellationToken: ct);
                return true;
            }

            // ─ /close ticketId ──────────────────────────────
            case "/close" when p.Length >= 2:
            {
                if (!int.TryParse(p[1], out var cid)) return false;
                var ok = await tickets.CloseAsync(cid);
                if (!ok) { await bot.SendMessage(_c.AdminChatId, $"❌ Тикет #{cid} не найден.", cancellationToken: ct); return true; }
                var ticket = await tickets.GetAsync(cid);
                if (ticket is not null)
                    try { await bot.SendMessage(ticket.UserId, $"✅ Ваш тикет <b>#{cid}</b> закрыт. Спасибо за обращение!", ParseMode.Html, replyMarkup: kb.App(), cancellationToken: ct); } catch { }
                await bot.SendMessage(_c.AdminChatId, $"🔒 Тикет #{cid} закрыт.", cancellationToken: ct);
                return true;
            }
        }
        return false;
    }

    private async Task WebAppData(Message msg, WebAppData wd, CancellationToken ct)
    {
        try
        {
            var j = System.Text.Json.JsonDocument.Parse(wd.Data).RootElement;
            if (j.GetProperty("type").GetString() == "support")
            {
                var subj = j.TryGetProperty("subject", out var s) ? s.GetString() ?? "" : "";
                await support.ForwardToAdmin(new Message { From = msg.From, Text = j.GetProperty("message").GetString() }, subj, ct);
                await bot.SendMessage(msg.From!.Id, "Сообщение отправлено. Ответим скоро.", cancellationToken: ct);
            }
        }
        catch (Exception ex) { logger.LogError(ex, "WebApp data error"); }
    }

    public Task SendKey(long id, CancellationToken ct)     => Send(id, ct);
    public Task SendPayment(long id, CancellationToken ct)  => Send(id, ct);
    public Task StartSupport(long id, CancellationToken ct) => Send(id, ct);
}
