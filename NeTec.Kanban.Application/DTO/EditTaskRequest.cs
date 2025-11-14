public class EditTaskRequest
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? PlannedTime { get; set; }
    public decimal? ActualTime { get; set; }
    public string? AssignedUserId { get; set; }
}