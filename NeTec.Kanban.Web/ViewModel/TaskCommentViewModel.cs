namespace NeTec.Kanban.Domain.Entities.ViewModel
{
    /// <summary>
    /// DTO für die Anzeige eines einzelnen Kommentars innerhalb der Task-Details.
    /// </summary>
    public class TaskCommentViewModel
    {
        public string UserName { get; set; } = "Unbekannt";
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}