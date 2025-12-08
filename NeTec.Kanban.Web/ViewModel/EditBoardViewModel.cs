using System.ComponentModel.DataAnnotations;

namespace NeTec.Kanban.Domain.Entities.ViewModel
{
    /// <summary>
    /// ViewModel für die Bearbeitung von Board-Metadaten.
    /// </summary>
    public class EditBoardViewModel
    {
        [Required(ErrorMessage = "Der Titel ist erforderlich.")]
        [StringLength(100, ErrorMessage = "Der Titel darf maximal 100 Zeichen lang sein.")]
        public string? Titel { get; set; }

        [StringLength(500, ErrorMessage = "Die Beschreibung darf maximal 500 Zeichen lang sein.")]
        public string? Description { get; set; }
    }
}