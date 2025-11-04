using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeTec.Kanban.Domain.Entities
{
    public class Column
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BoardId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        public int OrderIndex { get; set; } = 0;

        // Navigation
        [ForeignKey(nameof(BoardId))]
        public Board? Board { get; set; }

        public ICollection<TaskItem>? Tasks { get; set; }
    }
}