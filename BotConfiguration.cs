namespace KENOS.Bot.Models;

public sealed class BotConfig
{
    public const string Section = "BotConfiguration";

    public string BotToken    { get; init; } = "";
    public long   AdminChatId { get; init; }
    public List<long> AdminIds { get; init; } = new();

    public string WebAppUrl   { get; init; } = "";
    public string JsonBinKey  { get; init; } = "";
    public string JsonBinId   { get; init; } = "";

    public string SupportChatLink { get; init; } = "https://t.me/scantoptmz";
    public string ChannelLink     { get; init; } = "https://t.me/scantoptmz";
    public string OwnerLink       { get; init; } = "https://t.me/datadied";
    public string DevsLink        { get; init; } = "https://t.me/playerwallhack";

    public bool IsAdmin(long id) => id == AdminChatId || AdminIds.Contains(id);
}
