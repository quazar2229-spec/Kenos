namespace KENOS.Bot.Models;

public enum TicketStatus { Open, InProgress, Closed }

public sealed class Ticket
{
    public int    Id         { get; set; }
    public long   UserId     { get; set; }
    public string Username   { get; set; } = "";
    public string Subject    { get; set; } = "";
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<TicketMessage> Messages { get; set; } = new();
}

public sealed class TicketMessage
{
    public int    Id        { get; set; }
    public int    TicketId  { get; set; }
    public bool   FromAdmin { get; set; }
    public string Text      { get; set; } = "";
    public string Sender    { get; set; } = ""; // имя отправителя
    public DateTime SentAt  { get; set; } = DateTime.UtcNow;
    public Ticket? Ticket   { get; set; }
}
