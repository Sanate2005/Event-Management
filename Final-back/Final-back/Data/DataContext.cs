using Microsoft.EntityFrameworkCore;
using Final_back.Models;

namespace Final_back.Data
{
    public class DataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Location> Locations => Set<Location>();

        public DataContext(DbContextOptions<DataContext> options)
            : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Keep this only for local-dev convenience; in production use appsettings / secrets.
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    @"Data Source=(localdb)\MSSQLLocalDB;
                      Initial Catalog=Final14;
                      Integrated Security=True;
                      Connect Timeout=30;
                      Encrypt=False;
                      Trust Server Certificate=False;
                      Application Intent=ReadWrite;
                      Multi Subnet Failover=False");
            }
        }

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // ───── 1. Event.Location as an owned value object ─────
            b.Entity<Event>()
             .OwnsOne(e => e.Location);

            // ───── 2. Participant ••• Ticket  (NO cascade) ─────
            b.Entity<Participant>()
             .HasOne(p => p.Ticket)
             .WithMany(t => t.Participants)
             .HasForeignKey(p => p.TicketId)
             .OnDelete(DeleteBehavior.Restrict);

            // ───── 3. Participant ••• User   (NO cascade) ─────
            b.Entity<Participant>()
             .HasOne(p => p.User)
             .WithMany(u => u.ParticipantEntries)
             .HasForeignKey(p => p.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            // ───── 4. Purchase ••• Ticket   (NO cascade) ─────
            b.Entity<Purchase>()
             .HasOne(p => p.Ticket)
             .WithMany(t => t.Purchases)
             .HasForeignKey(p => p.TicketId)
             .OnDelete(DeleteBehavior.Restrict);

            // ───── 5. Purchase ••• User     (NO cascade) ─────
            b.Entity<Purchase>()
             .HasOne(p => p.User)
             .WithMany(u => u.Purchases)
             .HasForeignKey(p => p.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            // ───── 6. Unique constraints & precision ─────
            b.Entity<User>()
             .HasIndex(u => u.Email)
             .IsUnique();

            b.Entity<Ticket>()
             .HasIndex(t => new { t.EventId, t.Type })
             .IsUnique();

            b.Entity<Ticket>()
             .Property(t => t.Price)
             .HasColumnType("decimal(10,2)");

            b.Entity<Purchase>()
             .Property(p => p.TotalAmount)
             .HasColumnType("decimal(10,2)");
        }
    }
}