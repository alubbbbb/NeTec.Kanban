namespace NeTec.Kanban.Domain.Entities;

public class Comment
{
    public int CommentID { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property: Ein Kommentar gehört zu einem Task
    public int TaskItemID { get; set; }
    public TaskItem? TaskItem { get; set; }

    // Navigation Property: Ein Kommentar wurde von einem User verfasst
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
}