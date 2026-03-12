using KENOS.Bot.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace KENOS.Bot.Services;

public sealed class Support(
    ITelegramBotClient bot,
    IOptions<BotConfig> cfg,
    ILogger<Support> log)
{
    private readonly BotConfig _c = cfg.Value;

    public async Task ForwardToAdmin(Message msg, string subject, CancellationToken ct)
    {
        if (_c.AdminChatId == 0) return;
        var uid   = msg.From?.Id ?? 0;
        var uname = msg.From?.Username is { } u ? $"@{u}" : msg.From?.FirstName ?? "?";

        await bot.SendMessage(_c.AdminChatId,
            $"Новое обращение\n\n" +
            $"Пользователь: <code>{uid}</code> ({uname})\n" +
            $"Тема: {Esc(subject)}\n\n" +
            $"{Esc(msg.Text ?? "")}\n\n" +
            $"Ответить: <code>/reply {uid} текст</code>",
            ParseMode.Html, cancellationToken: ct);
    }

    public async Task<bool> HandleAdminReply(Message msg, CancellationToken ct)
    {
        var text = msg.Text ?? "";
        if (!text.StartsWith("/reply ", StringComparison.OrdinalIgnoreCase)) return false;

        var parts = text.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3 || !long.TryParse(parts[1], out var target)) return false;

        try
        {
            await bot.SendMessage(target,
                $"Ответ оператора:\n\n{Esc(parts[2])}",
                ParseMode.Html, cancellationToken: ct);

            await bot.SendMessage(_c.AdminChatId,
                $"Ответ доставлен <code>{target}</code>.",
                ParseMode.Html, cancellationToken: ct);

            return true;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Reply delivery failed");
            return false;
        }
    }

    private static string Esc(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
