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

        // FK auf ApplicationUser (Owner) als string
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(250)]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        public ICollection<Column>? Columns { get; set; }
    }
}