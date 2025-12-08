using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeTec.Kanban.Domain.Entities
{
    /// <summary>
    /// Repräsentiert einen Benutzerkommentar zu einer Aufgabe.
    /// Dient der Kommunikation und Historie innerhalb eines Tickets.
    /// </summary>
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TaskItemId { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "Der Kommentar darf nicht leer sein.")]
        [StringLength(1000, ErrorMessage = "Der Kommentar darf maximal 1000 Zeichen lang sein.")]
        public string Content { get; set; } = string.Empty;

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(TaskItemId))]
        public TaskItem? TaskItem { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }
    }
}