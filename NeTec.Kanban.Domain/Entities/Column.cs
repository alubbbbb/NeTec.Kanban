namespace NeTec.Kanban.Domain.Entities;

public class Column
{
    public int ColumnID { get; set; }
    public string Name { get; set; } = string.Empty;
    public int OrderIndex { get; set; }

    // Navigation Property: Eine Spalte gehört zu einem Board
    public int BoardID { get; set; }
    public Board? Board { get; set; }

    // Navigation Property: Eine Spalte hat viele Tasks
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}