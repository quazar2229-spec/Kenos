using KENOS.Bot.Handlers;
using KENOS.Bot.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace KENOS.Bot.Services;

public sealed class BotWorker(
    ITelegramBotClient bot,
    IServiceScopeFactory scopes,
    IOptions<BotConfig> cfg,
    ILogger<BotWorker> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var me = await bot.GetMe(ct);
        log.LogInformation("KENOS Bot started as @{Name} (id={Id})", me.Username, me.Id);

        bot.StartReceiving(
            OnUpdate, OnError,
            new ReceiverOptions
            {
                AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
                DropPendingUpdates = true
            },
            ct);

        await Task.Delay(Timeout.Infinite, ct);
    }

    private async Task OnUpdate(ITelegramBotClient _, Update u, CancellationToken ct)
    {
        await using var scope = scopes.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        try
        {
            if (u.Message is { } msg)
                await sp.GetRequiredService<MsgHandler>().Handle(msg, ct);
            else if (u.CallbackQuery is { } cb)
                await sp.GetRequiredService<CbHandler>().Handle(cb, ct);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Update {Id} failed", u.Id);
        }
    }

    private Task OnError(ITelegramBotClient _, Exception ex, CancellationToken ct)
    {
        var text = ex is ApiRequestException a ? $"[{a.ErrorCode}] {a.Message}" : ex.Message;
        log.LogError("Polling error: {E}", text);
        return Task.CompletedTask;
    }
}
