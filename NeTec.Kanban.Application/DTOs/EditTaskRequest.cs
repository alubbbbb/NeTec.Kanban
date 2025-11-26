using System.ComponentModel.DataAnnotations;

namespace NeTec.Kanban.Application.DTOs
{
    /// <summary>
    /// DTO für das Erstellen und Bearbeiten einer Aufgabe.
    /// Wird vom Task-Modal verwendet.
    /// </summary>
    public class EditTaskRequest
    {
        /// <summary>
        /// Die ID der Aufgabe. Null bei Neuerstellung.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Die ID der Spalte, in der die Aufgabe erstellt/bearbeitet wird.
        /// </summary>
        [Required]
        public int ColumnId { get; set; }

        [Required(ErrorMessage = "Ein Titel ist erforderlich.")]
        [StringLength(150, ErrorMessage = "Der Titel darf maximal 150 Zeichen lang sein.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Die Beschreibung darf maximal 2000 Zeichen lang sein.")]
        public string? Description { get; set; }

        [Required]
        public string Priority { get; set; } = "Medium";

        public DateTime? DueDate { get; set; }

        [Range(0, 1000, ErrorMessage = "Der Wert muss zwischen 0 und 1000 liegen.")]
        public decimal? PlannedTime { get; set; }

        [Range(0, 1000, ErrorMessage = "Der Wert muss zwischen 0 und 1000 liegen.")]
        public decimal? ActualTime { get; set; }

        /// <summary>
        /// Die User-ID des zugewiesenen Benutzers (optional).
        /// </summary>
        public string? AssignedUserId { get; set; }
    }
}