namespace KENOS.Bot.Models;

public sealed class UserSession
{
    public long   UserId { get; init; }
    public State  State  { get; set; } = State.Idle;
}

public enum State { Idle, WaitingSupport }

public sealed class UserKey
{
    public long     UserId    { get; init; }
    public string   KeyValue  { get; init; } = "";
    public string   Hwid      { get; set; }  = "";
    public string   Plan      { get; set; }  = "Custom BlueStacks";
    public DateTime ExpiresAt { get; set; }
    public bool     Active    => DateTime.UtcNow < ExpiresAt;
}
