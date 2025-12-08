using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace NeTec.Kanban.Domain.Entities
{
    /// <summary>
    /// Erweiterte Benutzerklasse für die Anwendung.
    /// Erbt von IdentityUser und fügt projektspezifische Profildaten hinzu.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Der vollständige Name des Benutzers (z.B. "Max Mustermann").
        /// Wird für die Anzeige auf Boards und Tickets verwendet.
        /// </summary>
        [StringLength(100, ErrorMessage = "Der Name darf maximal 100 Zeichen lang sein.")]
        public string? FullName { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties (Relationen)

        /// <summary>
        /// Boards, die dieser Benutzer erstellt hat (Besitzer).
        /// </summary>
        public ICollection<Board> Boards { get; set; } = new List<Board>();

        /// <summary>
        /// Aufgaben, die diesem Benutzer zugewiesen sind.
        /// </summary>
        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();

        /// <summary>
        /// Kommentare, die dieser Benutzer verfasst hat.
        /// </summary>
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        /// <summary>
        /// Zeiterfassungs-Einträge dieses Benutzers.
        /// </summary>
        public ICollection<TimeTracking> TimeTrackings { get; set; } = new List<TimeTracking>();
    }
}