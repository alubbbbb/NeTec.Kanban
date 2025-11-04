using System.Xml.Linq;

namespace NeTec.Kanban.Domain.Entities;

public class TaskItem
{
    public int TaskItemID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; } // z.B. 1=Hoch, 2=Mittel, 3=Niedrig
    public decimal? EstimatedHours { get; set; }
    public decimal? RemainingHours { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property: Ein Task gehört zu einer Spalte
    public int ColumnID { get; set; }
    public Column? Column { get; set; }

    // Navigation Property: Ein Task ist einem User zugewiesen
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    // Navigation Property: Ein Task hat viele Kommentare
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    // Navigation Property: Ein Task hat viele Zeiteinträge
    public ICollection<TimeTracking> TimeTrackings { get; set; } = new List<TimeTracking>();
}