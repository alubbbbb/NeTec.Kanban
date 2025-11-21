namespace NeTec.Kanban.Domain.Entities.ViewModel
{
    /// <summary>
    /// Umfassendes ViewModel für die Detailansicht einer Aufgabe.
    /// Bündelt Stammdaten, relationale Informationen und Kommentare für die Anzeige.
    /// </summary>
    public class TaskDetailsViewModel
    {
        // --- Stammdaten ---
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Priority { get; set; } = "Medium";

        // --- Zeitplanung ---
        public DateTime? DueDate { get; set; }
        public decimal? PlannedTime { get; set; }
        public decimal? ActualTime { get; set; }

        // --- Zuordnungen ---
        public string ColumnName { get; set; } = string.Empty;
        public int ColumnId { get; set; }
        public int BoardId { get; set; }

        public string AssignedUserName { get; set; } = "Nicht zugewiesen";
        public string? AssignedUserId { get; set; }

        // --- Meta-Informationen ---
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // --- Listen ---
        public List<TaskCommentViewModel> Comments { get; set; } = new();

        public string FullName { get; set; }
    }
}