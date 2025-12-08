using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeTec.Kanban.Domain.Entities
{
    /// <summary>
    /// Repräsentiert eine einzelne Aufgabe (Ticket) im System.
    /// Enthält alle fachlichen Daten sowie Verknüpfungen zu Status (Spalte) und Bearbeiter.
    /// </summary>
    public class TaskItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ColumnId { get; set; }

        /// <summary>
        /// ID des zugewiesenen Benutzers (kann null sein, wenn Aufgabe unzugewiesen ist).
        /// </summary>
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Der Titel ist erforderlich.")]
        [StringLength(150, ErrorMessage = "Der Titel darf maximal 150 Zeichen lang sein.")]
        public string Title { get; set; } = null!;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = "Medium";

        // Zeiterfassung (Dezimal für Stunden, z.B. 1.5h)
        [Range(0, 1000)]
        [Column(TypeName = "decimal(8,2)")]
        public decimal? EstimatedHours { get; set; }

        [Range(0, 1000)]
        [Column(TypeName = "decimal(8,2)")]
        public decimal? RemainingHours { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Sortierreihenfolge innerhalb der Spalte (für Drag & Drop).
        /// </summary>
        public int OrderIndex { get; set; } = 0;

        // Navigation Properties
        [ForeignKey(nameof(ColumnId))]
        public Column? Column { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? AssignedTo { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public ICollection<TimeTracking> TimeTrackings { get; set; } = new List<TimeTracking>();
    }
}