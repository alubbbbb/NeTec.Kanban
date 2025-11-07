using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeTec.Kanban.Domain.Entities
{
    public class TaskItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ColumnId { get; set; }

        public string? UserId { get; set; }

        [Required(ErrorMessage = "Der Aufgaben-Titel ist erforderlich.")]
        [StringLength(150, ErrorMessage = "Maximal 150 Zeichen.")]
        public string Title { get; set; } = null!;

        [StringLength(2000, ErrorMessage = "Maximal 2000 Zeichen.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Die Priorität ist erforderlich.")]
        [StringLength(20, ErrorMessage = "Maximal 20 Zeichen.")]
        public string Priority { get; set; } = "Medium";

        [Range(0, 500, ErrorMessage = "Schätzwert muss zwischen 0 und 500 Stunden liegen.")]
        [Column(TypeName = "decimal(8,2)")]
        public decimal? EstimatedHours { get; set; }

        [Range(0, 500, ErrorMessage = "Schätzwert muss zwischen 0 und 500 Stunden liegen.")]
        [Column(TypeName = "decimal(8,2)")]
        public decimal? RemainingHours { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(ColumnId))]
        public Column? Column { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? AssignedTo { get; set; }

        public ICollection<Comment>? Comments { get; set; }
        public ICollection<TimeTracking>? TimeTrackings { get; set; }

        // NEU: Drag & Drop Sortierreihenfolge innerhalb einer Spalte
        public int OrderIndex { get; set; } = 0;
    }
}