using KENOS.Bot.Models;
using KENOS.Bot.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace KENOS.Bot.Handlers;

public sealed class CbHandler(
    ITelegramBotClient bot,
    IOptions<BotConfig> cfg,
    Keyboards kb,
    MsgHandler msg,
    ILogger<CbHandler> log)
{
    public async Task Handle(CallbackQuery q, CancellationToken ct)
    {
        var uid  = q.From.Id;
        var data = q.Data ?? "";
        var cid  = q.Message?.Chat.Id ?? uid;
        var mid  = q.Message?.MessageId ?? 0;

        await bot.AnswerCallbackQuery(q.Id, cancellationToken: ct);

        switch (data)
        {
            case "menu:key":
                await bot.DeleteMessage(cid, mid, ct);
                await msg.SendKey(uid, ct);
                break;
            case "menu:pay":
                await bot.DeleteMessage(cid, mid, ct);
                await msg.SendPayment(uid, ct);
                break;
            case "menu:support":
                await bot.DeleteMessage(cid, mid, ct);
                await msg.StartSupport(uid, ct);
                break;
            default:
                log.LogDebug("Unknown cb: {D}", data);
                break;
        }
    }
}
