using KENOS.Bot.Models;
using Microsoft.EntityFrameworkCore;

namespace KENOS.Bot.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> opts) : DbContext(opts)
{
    public DbSet<Ticket>        Tickets  { get; set; }
    public DbSet<TicketMessage> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Ticket>().HasIndex(t => t.UserId);
        b.Entity<TicketMessage>().HasIndex(m => m.TicketId);
    }
}
