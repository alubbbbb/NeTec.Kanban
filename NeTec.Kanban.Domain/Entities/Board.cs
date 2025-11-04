using System.Data.Common;

namespace NeTec.Kanban.Domain.Entities;

public class Board
{
    public int BoardID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property: Ein Board gehört zu einem User
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    // Navigation Property: Ein Board hat viele Spalten
    public ICollection<Column> Columns { get; set; } = new List<Column>();
}