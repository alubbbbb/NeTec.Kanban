namespace NeTec.Kanban.Domain.Entities.ViewModel;
public class TaskDetailsViewModel
{
    public int TaskId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? PlannedTime { get; set; }
    public decimal? ActualTime { get; set; }

    public string ColumnName { get; set; }
    public int ColumnId { get; set; } // Neu
    public int BoardId { get; set; }

    public string AssignedUserName { get; set; }
    public string? AssignedUserId { get; set; } // Neu

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string FullName { get; set; }

    public List<TaskCommentViewModel> Comments { get; set; } = new();
}


