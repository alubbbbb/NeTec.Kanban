using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NeTec.Kanban.Domain.Entities
{
    /// <summary>
    /// Repräsentiert ein Kanban-Board in der Datenbank.
    /// </summary>
    public class Board
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "Der Board-Titel ist erforderlich.")]
        [StringLength(100, ErrorMessage = "Der Titel darf maximal 100 Zeichen lang sein.")]
        public string Titel { get; set; } = null!;

        [StringLength(2000, ErrorMessage = "Die Beschreibung darf maximal 2000 Zeichen lang sein.")]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Die Liste der Spalten, die zu diesem Board gehören.
        // Wird automatisch von Entity Framework Core geladen, wenn .Include() verwendet wird.
        public ICollection<Column> Columns { get; set; } = new List<Column>();
    }
}