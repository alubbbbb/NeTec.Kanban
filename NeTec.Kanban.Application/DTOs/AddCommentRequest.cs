using System.ComponentModel.DataAnnotations;

namespace NeTec.Kanban.Application.DTOs
{
    /// <summary>
    /// DTO zum Hinzufügen eines Kommentars via API.
    /// </summary>
    public class AddCommentRequest
    {
        [Required]
        public int TaskId { get; set; }

        [Required(ErrorMessage = "Der Kommentar darf nicht leer sein.")]
        [StringLength(1000, ErrorMessage = "Maximal 1000 Zeichen.")]
        public string Text { get; set; } = string.Empty;
    }
}