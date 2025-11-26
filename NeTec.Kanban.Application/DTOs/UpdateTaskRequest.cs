using System.ComponentModel.DataAnnotations;

namespace NeTec.Kanban.Application.DTOs
{
    /// <summary>
    /// DTO für Drag & Drop Operationen von Aufgaben.
    /// Beinhaltet die neue Spalte und die neue Sortierposition.
    /// </summary>
    public class UpdateTaskRequest
    {
        [Required]
        public int TaskId { get; set; }

        [Required]
        public int NewColumnId { get; set; }

        /// <summary>
        /// Der neue Sortierindex innerhalb der Spalte (0-basiert).
        /// Kann null sein, wenn keine Umsortierung stattfindet.
        /// </summary>
        public int? NewOrderIndex { get; set; }
    }
}