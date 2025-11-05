using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeTec.Kanban.Domain.Entities
{
    public class Board
    {
        [Key]
        public int Id { get; set; }

        [Required] 
        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "Der Board-Titel ist erforderlich.")]
        [StringLength(100, ErrorMessage = "Maximal 100 Zeichen.")]
        public string Titel { get; set; } = null!;

        [StringLength(2000, ErrorMessage = "Maximal 2000 Zeichen für die Beschreibung.")]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        public ICollection<Column>? Columns { get; set; }
    }
}