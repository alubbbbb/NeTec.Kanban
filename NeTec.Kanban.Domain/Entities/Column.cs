using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeTec.Kanban.Domain.Entities
{
    /// <summary>
    /// Repräsentiert eine Spalte innerhalb eines Boards (z.B. "To Do", "In Progress").
    /// Dient der Status-Gruppierung von Aufgaben.
    /// </summary>
    public class Column
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BoardId { get; set; }

        [Required(ErrorMessage = "Der Spaltentitel ist erforderlich.")]
        [StringLength(100, ErrorMessage = "Der Titel darf maximal 100 Zeichen lang sein.")]
        public string Titel { get; set; } = null!;

        /// <summary>
        /// Bestimmt die Reihenfolge der Spalten im Board (von links nach rechts).
        /// </summary>
        public int OrderIndex { get; set; } = 0;

        // Navigation
        [ForeignKey(nameof(BoardId))]
        public Board? Board { get; set; }

        /// <summary>
        /// Liste der Aufgaben in dieser Spalte.
        /// </summary>
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}