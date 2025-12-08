using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NeTec.Kanban.Domain.Entities;

namespace NeTec.Kanban.Infrastructure.Data
{
    /// <summary>
    /// Zentraler Datenbankkontext der Anwendung.
    /// Konfiguriert die Verbindung zu SQL Server, das Identity-Framework
    /// sowie die relationalen Abbildungen (Fluent API) der Fachmodelle.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Fachliche Tabellen
        public DbSet<Board> Boards { get; set; } = null!;
        public DbSet<Column> Columns { get; set; } = null!;
        public DbSet<TaskItem> TaskItems { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<TimeTracking> TimeTrackings { get; set; } = null!;

        /// <summary>
        /// Konfiguration des Datenbankschemas mittels Fluent API.
        /// Definiert Indizes, Datentypen und das Löschverhalten (Referenzielle Integrität).
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ============================================================
            // INDIZES (Performance-Optimierung)
            // ============================================================

            // Beschleunigt Abfragen der Spaltenreihenfolge pro Board
            builder.Entity<Column>()
                .HasIndex(c => new { c.BoardId, c.OrderIndex });

            // Beschleunigt Abfragen der Aufgabenreihenfolge pro Spalte
            builder.Entity<TaskItem>()
                .HasIndex(t => new { t.ColumnId, t.OrderIndex });

            // ============================================================
            // DATENTYPEN & EINSCHRÄNKUNGEN
            // ============================================================

            // Begrenzung von Identity-Feldern für optimale Speichernutzung
            builder.Entity<ApplicationUser>(b =>
            {
                b.Property(u => u.UserName).HasMaxLength(100);
                b.Property(u => u.Email).HasMaxLength(150);
            });

            builder.Entity<IdentityRole>(b =>
            {
                b.Property(r => r.Name).HasMaxLength(50);
            });

            // Präzise Definition für Finanz-/Zeitwerte (Vermeidung von Rundungsfehlern)
            builder.Entity<TaskItem>().Property(t => t.EstimatedHours).HasColumnType("decimal(8,2)");
            builder.Entity<TaskItem>().Property(t => t.RemainingHours).HasColumnType("decimal(8,2)");
            builder.Entity<TimeTracking>().Property(tt => tt.HoursSpent).HasColumnType("decimal(8,2)");

            // Maximale Länge für Fremdschlüssel (SQL Server Index Limitierung)
            builder.Entity<Board>().Property(b => b.UserId).HasMaxLength(450);
            builder.Entity<TaskItem>().Property(t => t.UserId).HasMaxLength(450);
            builder.Entity<Comment>().Property(c => c.UserId).HasMaxLength(450);

            // ============================================================
            // BEZIEHUNGEN & LÖSCHWEITERGABE (CASCADE DELETE)
            // ============================================================

            // Board -> Columns: Löschen eines Boards entfernt alle enthaltenen Spalten
            builder.Entity<Board>()
                .HasMany(b => b.Columns)
                .WithOne(c => c.Board)
                .HasForeignKey(c => c.BoardId)
                .OnDelete(DeleteBehavior.Cascade);

            // Column -> Tasks: Löschen einer Spalte entfernt alle enthaltenen Aufgaben
            builder.Entity<Column>()
                .HasMany(c => c.Tasks)
                .WithOne(t => t.Column)
                .HasForeignKey(t => t.ColumnId)
                .OnDelete(DeleteBehavior.Cascade);

            // Task -> Comments: Löschen einer Aufgabe entfernt alle zugehörigen Kommentare
            builder.Entity<TaskItem>()
                .HasMany(t => t.Comments)
                .WithOne(c => c.TaskItem)
                .HasForeignKey(c => c.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Task -> AssignedUser:
            // Wird ein Benutzer gelöscht, bleibt die Aufgabe bestehen, 
            // die Zuweisung wird auf NULL gesetzt (Datenerhalt).
            builder.Entity<TaskItem>()
                .HasOne(t => t.AssignedTo)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Board -> Owner:
            // Deaktivierung von Cascade Delete um "Multiple Cascade Paths" Fehler im SQL Server zu vermeiden.
            // Boards müssen explizit gelöscht werden, bevor der Benutzer gelöscht werden kann, 
            // oder die Logik wird anwendungsseitig behandelt.
            builder.Entity<Board>()
                .HasOne(b => b.Owner)
                .WithMany(u => u.Boards)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}