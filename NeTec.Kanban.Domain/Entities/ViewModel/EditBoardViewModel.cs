using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeTec.Kanban.Domain.Entities.ViewModel
{
    public class EditBoardViewModel
    {
        [Required(ErrorMessage = "Der Board-Name ist erforderlich.")]
        [StringLength(100, ErrorMessage = "Der Board-Name darf maximal 100 Zeichen lang sein.")]
        public string Titel { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Die Beschreibung darf maximal 500 Zeichen lang sein.")]
        public string Description { get; set; }
    }
}
