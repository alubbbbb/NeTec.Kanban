using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace NeTec.Kanban.Domain.Entities
{
    
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string? FullName { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Board>? Boards { get; set; }
        public ICollection<TaskItem>? AssignedTasks { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<TimeTracking>? TimeTrackings { get; set; }
    }
}