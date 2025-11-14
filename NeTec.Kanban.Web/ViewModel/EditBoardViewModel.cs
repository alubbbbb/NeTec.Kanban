using System.ComponentModel.DataAnnotations;

namespace NeTec.Kanban.Domain.Entities.ViewModel
{
    public class EditBoardViewModel
    {
        [Required(ErrorMessage = "Der Board-Name ist erforderlich.")]
        [StringLength(100, ErrorMessage = "Der Board-Name darf maximal 100 Zeichen lang sein.")]
        public string? Titel { get; set; }

        [StringLength(500, ErrorMessage = "Die Beschreibung darf maximal 500 Zeichen lang sein.")]
        // HIER ist die Änderung: Initialisierung mit einem leeren String.
        public string? Description { get; set; } 
    }
}