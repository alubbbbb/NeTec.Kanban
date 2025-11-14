using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NeTec.Kanban.Domain.Entities;

namespace NeTec.Kanban.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Board> Boards { get; set; } = null!;
        public DbSet<Column> Columns { get; set; } = null!;
        public DbSet<TaskItem> TaskItems { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<TimeTracking> TimeTrackings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ---------------------------------------
            // Indexe für Sortierung / Spaltenreihenfolge
            // ---------------------------------------
            builder.Entity<Column>()
                .HasIndex(c => new { c.BoardId, c.OrderIndex });

            builder.Entity<TaskItem>()
                .HasIndex(t => new { t.ColumnId, t.OrderIndex });

            // ---------------------------------------
            // Identity column sizing (optional, sauber)
            // ---------------------------------------
            builder.Entity<ApplicationUser>(b =>
            {
                b.Property(u => u.UserName).HasMaxLength(100);
                b.Property(u => u.Email).HasMaxLength(150);
                b.Property(u => u.PasswordHash).HasMaxLength(255);
            });

            builder.Entity<IdentityRole>(b =>
            {
                b.Property(r => r.Name).HasMaxLength(50);
                b.Property(r => r.NormalizedName).HasMaxLength(50);
            });

            // ---------------------------------------
            // Decimal precision (Zeiterfassung)
            // ---------------------------------------
            builder.Entity<TaskItem>()
                .Property(t => t.EstimatedHours)
                .HasColumnType("decimal(8,2)");

            builder.Entity<TaskItem>()
                .Property(t => t.RemainingHours)
                .HasColumnType("decimal(8,2)");

            builder.Entity<TimeTracking>()
                .Property(tt => tt.HoursSpent)
                .HasColumnType("decimal(8,2)");

            // ---------------------------------------
            // Beziehungen & DeleteBehavior
            // ---------------------------------------

            // Board → Columns (Cascade erlaubt)
            builder.Entity<Board>()
                .HasMany(b => b.Columns)
                .WithOne(c => c.Board)
                .HasForeignKey(c => c.BoardId)
                .OnDelete(DeleteBehavior.Cascade);

            // Column → Tasks (Cascade erlaubt)
            builder.Entity<Column>()
                .HasMany(c => c.Tasks)
                .WithOne(t => t.Column)
                .HasForeignKey(t => t.ColumnId)
                .OnDelete(DeleteBehavior.Cascade);

            // Task → Comments (Cascade erlaubt)
            builder.Entity<TaskItem>()
                .HasMany(t => t.Comments)
                .WithOne(c => c.TaskItem)
                .HasForeignKey(c => c.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Task → AssignedTo (SetNull, weil User gelöscht werden darf)
            builder.Entity<TaskItem>()
                .HasOne(t => t.AssignedTo)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // ---------------------------------------
            // FIX: Board → UserId (SetNull, verhindert multiple cascade paths)
            // ---------------------------------------
            builder.Entity<Board>()
                .HasOne(b => b.Owner)
                .WithMany() // Boards gehören nicht in ApplicationUser
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.NoAction);


            // ---------------------------------------
            // FK string sizes (SQL Server limit 450)
            // ---------------------------------------
            builder.Entity<Board>().Property(b => b.UserId).HasMaxLength(450);
            builder.Entity<TaskItem>().Property(t => t.UserId).HasMaxLength(450);
            builder.Entity<Comment>().Property(c => c.UserId).HasMaxLength(450);
            builder.Entity<TimeTracking>().Property(tt => tt.UserId).HasMaxLength(450);
        }
    }
}
