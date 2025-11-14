using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NeTec.Kanban.Domain.Entities;
using System.Reflection.Emit;

namespace NeTec.Kanban.Infrastructure.Data
{
    // IdentityDbContext<ApplicationUser> (string keys) — einfache Standard-Variante
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

            // ApplicationDbContext.OnModelCreating
            builder.Entity<Column>()
                .HasIndex(c => new { c.BoardId, c.OrderIndex });

            builder.Entity<TaskItem>()
                .HasIndex(t => new { t.ColumnId, t.OrderIndex });


            // ---- Identity column sizing (entspricht deinem ERD-Anforderungsrahmen) ----
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

            // ---- Decimal precision ----
            builder.Entity<TaskItem>()
                .Property(t => t.EstimatedHours)
                .HasColumnType("decimal(8,2)");

            builder.Entity<TaskItem>()
                .Property(t => t.RemainingHours)
                .HasColumnType("decimal(8,2)");

            builder.Entity<TimeTracking>()
                .Property(tt => tt.HoursSpent)
                .HasColumnType("decimal(8,2)");

            // ---- Relationships & delete behavior ----
            builder.Entity<Board>()
                .HasMany(b => b.Columns)
                .WithOne(c => c.Board)
                .HasForeignKey(c => c.BoardId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Column>()
                .HasMany(c => c.Tasks)
                .WithOne(t => t.Column)
                .HasForeignKey(t => t.ColumnId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TaskItem>()
                .HasMany(t => t.Comments)
                .WithOne(c => c.TaskItem)
                .HasForeignKey(c => c.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TaskItem>()
                .HasOne(t => t.AssignedTo)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Ensure string FK lengths in DB (for our UserId fields)
            builder.Entity<Board>().Property(b => b.UserId).HasMaxLength(450);
            builder.Entity<TaskItem>().Property(t => t.UserId).HasMaxLength(450);
            builder.Entity<Comment>().Property(c => c.UserId).HasMaxLength(450);
            builder.Entity<TimeTracking>().Property(tt => tt.UserId).HasMaxLength(450);


        }
    }
}