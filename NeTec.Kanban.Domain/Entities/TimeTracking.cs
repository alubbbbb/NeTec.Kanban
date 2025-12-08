using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeTec.Kanban.Domain.Entities
{
    /// <summary>
    /// Repräsentiert einen Zeiterfassungs-Eintrag für eine Aufgabe.
    /// </summary>
    public class TimeTracking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TaskItemId { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Die erfasste Zeit in Stunden (z.B. 1.5 für 1h 30min).
        /// </summary>
        [Range(0.01, 100, ErrorMessage = "Bitte einen gültigen Wert zwischen 0.01 und 100 Stunden eingeben.")]
        [Column(TypeName = "decimal(8,2)")]
        public decimal HoursSpent { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(TaskItemId))]
        public TaskItem? TaskItem { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }
    }
}