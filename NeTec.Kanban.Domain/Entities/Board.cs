using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeTec.Kanban.Domain.Entities
{
    /// <summary>
    /// Repräsentiert ein Kanban-Board.
    /// Dient als Container für Spalten und Aufgaben und ist einem Benutzer zugeordnet.
    /// </summary>
    public class Board
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Fremdschlüssel zum Ersteller/Besitzer des Boards.
        /// </summary>
        [Required]
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Navigation-Property zum Benutzerobjekt.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public ApplicationUser? Owner { get; set; }

        [Required(ErrorMessage = "Der Board-Titel ist erforderlich.")]
        [StringLength(100, ErrorMessage = "Der Titel darf maximal 100 Zeichen lang sein.")]
        public string Titel { get; set; } = null!;

        [StringLength(2000, ErrorMessage = "Die Beschreibung darf maximal 2000 Zeichen lang sein.")]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Liste der zugehörigen Spalten.
        /// Wird initialisiert, um NullReferenceExceptions zu vermeiden.
        /// </summary>
        public ICollection<Column> Columns { get; set; } = new List<Column>();
    }
}