using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeTec.Kanban.Domain.Entities
{
    public class TimeTracking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TaskItemId { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [Column(TypeName = "decimal(8,2)")]
        public decimal HoursSpent { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(TaskItemId))]
        public TaskItem? TaskItem { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }
    }
}