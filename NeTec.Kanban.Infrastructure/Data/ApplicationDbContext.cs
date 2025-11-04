using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NeTec.Kanban.Domain.Entities;

namespace NeTec.Kanban.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Hier registrieren wir unsere eigenen Entitäten als Tabellen
    public DbSet<Board> Boards { get; set; }
    public DbSet<Column> Columns { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<TimeTracking> TimeTrackings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TaskItem>()
         .HasOne(t => t.User)
         .WithMany()
         .HasForeignKey(t => t.UserId)
         .OnDelete(DeleteBehavior.Restrict);

        // Beziehung: Comment -> User
        builder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Geändert zu Restrict für Konsistenz

        // Beziehung: TimeTracking -> User (DIE NEUE, FINALE KORREKTUR)
        builder.Entity<TimeTracking>()
            .HasOne(tt => tt.User)
            .WithMany()
            .HasForeignKey(tt => tt.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TaskItem>()
            .Property(t => t.EstimatedHours)
            .HasColumnType("decimal(18,2)");

        builder.Entity<TaskItem>()
            .Property(t => t.RemainingHours)
            .HasColumnType("decimal(18,2)");

        builder.Entity<TimeTracking>()
            .Property(tt => tt.HoursSpent)
            .HasColumnType("decimal(18,2)");
    }
}