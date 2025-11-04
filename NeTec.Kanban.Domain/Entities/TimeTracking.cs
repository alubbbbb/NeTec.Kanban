namespace NeTec.Kanban.Domain.Entities;

public class TimeTracking
{
    public int TimeTrackingID { get; set; }
    public decimal HoursSpent { get; set; }
    public string? Description { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property: Ein Zeiteintrag gehört zu einem Task
    public int TaskItemID { get; set; }
    public TaskItem? TaskItem { get; set; }

    // Navigation Property: Ein Zeiteintrag wurde von einem User erfasst
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
}