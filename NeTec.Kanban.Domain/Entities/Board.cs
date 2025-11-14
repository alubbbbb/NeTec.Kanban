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

        // FK zum Besitzer (Identity-User)
        [Required]
        public string UserId { get; set; } = null!;

        // Navigation Property → Owner
        [ForeignKey(nameof(UserId))]
        public ApplicationUser? Owner { get; set; }

        [Required(ErrorMessage = "Der Board-Titel ist erforderlich.")]
        [StringLength(100, ErrorMessage = "Der Titel darf maximal 100 Zeichen lang sein.")]
        public string Titel { get; set; } = null!;

        [StringLength(2000, ErrorMessage = "Die Beschreibung darf maximal 2000 Zeichen lang sein.")]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Property → Columns
        public ICollection<Column> Columns { get; set; } = new List<Column>();
    }
}
