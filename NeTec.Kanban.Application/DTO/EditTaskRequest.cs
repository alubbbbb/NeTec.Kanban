public class EditTaskRequest
{
    public int? Id { get; set; }  // null = neue Aufgabe

    public int ColumnId { get; set; }  // WICHTIG: nötig für Create UND Edit

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string Priority { get; set; } = "Medium";

    public DateTime? DueDate { get; set; }

    public decimal? PlannedTime { get; set; }
    public decimal? ActualTime { get; set; }

    public string? AssignedUserId { get; set; }
}
