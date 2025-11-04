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

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = null!;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(20)]
        public string Priority { get; set; } = "Medium"; 

        [Column(TypeName = "decimal(8,2)")]
        public decimal? EstimatedHours { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? RemainingHours { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(ColumnId))]
        public Column? Column { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? AssignedTo { get; set; }

        public ICollection<Comment>? Comments { get; set; }
        public ICollection<TimeTracking>? TimeTrackings { get; set; }
    }
}