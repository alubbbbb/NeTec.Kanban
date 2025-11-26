namespace NeTec.Kanban.Application.DTOs;
public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Priority { get; set; } // "High" | "Medium" | "Low" (du nutzt string)
    public decimal? EstimatedHours { get; set; }
    public decimal? RemainingHours { get; set; }
    public int ColumnId { get; set; }
}