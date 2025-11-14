public class UpdateTaskRequest
{
    public int TaskId { get; set; }
    public int NewColumnId { get; set; }
    public int? NewOrderIndex { get; set; } // für Drag&Drop-Position
}