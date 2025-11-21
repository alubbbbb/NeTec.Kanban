using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeTec.Kanban.Domain.Entities;
using NeTec.Kanban.Infrastructure.Data;
using NeTec.Kanban.Web.Models.DTOs;

namespace NeTec.Kanban.Web.Controllers.Api
{
    /// <summary>
    /// REST-API Endpunkt für den Zugriff auf Board-Daten durch externe Systeme (z.B. CRM).
    /// </summary>
    [Route("api/boards")]
    [ApiController]
    public class BoardsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BoardsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Liefert eine Liste aller Boards inklusive Metadaten.
        /// GET: api/boards
        /// </summary>
        /// <returns>Liste von BoardDto Objekten im JSON-Format.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BoardDto>>> GetBoards()
        {
            // Projektion der Datenbank-Entitäten auf DTOs für effiziente Datenübertragung
            var boards = await _context.Boards
                .AsNoTracking()
                .Select(b => new BoardDto
                {
                    Id = b.Id,
                    Titel = b.Titel,
                    ErstelltAm = b.CreatedAt,
                    // Berechnung der Metriken direkt in der Datenbank
                    AnzahlSpalten = b.Columns.Count,
                    AnzahlAufgaben = b.Columns.SelectMany(c => c.Tasks!).Count()
                })
                .ToListAsync();

            return Ok(boards);
        }

        /// <summary>
        /// Liefert Details zu einem spezifischen Board.
        /// GET: api/boards/5
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BoardDto>> GetBoard(int id)
        {
            var board = await _context.Boards
                .Include(b => b.Columns)
                .ThenInclude(c => c.Tasks)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (board == null)
            {
                return NotFound();
            }

            var dto = new BoardDto
            {
                Id = board.Id,
                Titel = board.Titel,
                ErstelltAm = board.CreatedAt,
                AnzahlSpalten = board.Columns.Count,
                AnzahlAufgaben = board.Columns.SelectMany(c => c.Tasks!).Count()
            };

            return Ok(dto);
        }
    }
}