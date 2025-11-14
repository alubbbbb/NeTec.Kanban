using System.ComponentModel.DataAnnotations;

public class CreateBoardViewModel
{
    [Required(ErrorMessage = "Der Board-Name ist erforderlich.")]
    [StringLength(100, ErrorMessage = "Der Board-Name darf maximal 100 Zeichen lang sein.")]
    public string Titel { get; set; } = string.Empty;
}