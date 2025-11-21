using System.ComponentModel.DataAnnotations;

namespace NeTec.Kanban.Web.Models.DTOs
{
    /// <summary>
    /// DTO für das Verschieben von Spalten (Links/Rechts).
    /// </summary>
    public class MoveColumnRequest
    {
        [Required]
        public int ColumnId { get; set; }

        /// <summary>
        /// Die Richtung der Verschiebung ("left" oder "right").
        /// </summary>
        [Required]
        [RegularExpression("^(left|right)$", ErrorMessage = "Richtung muss 'left' oder 'right' sein.")]
        public string Direction { get; set; } = string.Empty;
    }
}