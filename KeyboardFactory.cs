using Microsoft.Extensions.Options;
using KENOS.Bot.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace KENOS.Bot.Services;

public sealed class Keyboards(IOptions<BotConfig> cfg)
{
    public InlineKeyboardMarkup App() => new(new[]
    {
        new[] { InlineKeyboardButton.WithWebApp("Открыть KENOS", new() { Url = cfg.Value.WebAppUrl }) }
    });
}
