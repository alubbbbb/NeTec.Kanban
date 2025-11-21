namespace NeTec.Kanban.Web.Models.Api
{
    /// <summary>
    /// Datentransferobjekt für die API-Ausgabe von Boards.
    /// Reduziert die Datenmenge und verhindert Zirkelbezüge bei der Serialisierung.
    /// </summary>
    public class BoardDto
    {
        public int Id { get; set; }
        public string Titel { get; set; } = string.Empty;
        public int AnzahlSpalten { get; set; }
        public int AnzahlAufgaben { get; set; }
        public DateTime ErstelltAm { get; set; }
    }
}