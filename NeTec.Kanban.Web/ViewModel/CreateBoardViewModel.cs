using System.ComponentModel.DataAnnotations;

namespace NeTec.Kanban.Domain.Entities.ViewModel
{
    /// <summary>
    /// ViewModel für die Erstellung eines neuen Boards.
    /// Enthält Validierungslogik für Benutzereingaben.
    /// </summary>
    public class CreateBoardViewModel
    {
        [Required(ErrorMessage = "Der Titel ist erforderlich.")]
        [StringLength(100, ErrorMessage = "Der Titel darf maximal 100 Zeichen lang sein.")]
        [Display(Name = "Board-Titel")]
        public string Titel { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Die Beschreibung darf maximal 500 Zeichen lang sein.")]
        [Display(Name = "Beschreibung (Optional)")]
        public string? Description { get; set; }
    }
}