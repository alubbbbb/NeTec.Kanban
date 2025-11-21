using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeTec.Kanban.Domain.Entities;
using NeTec.Kanban.Domain.Entities.ViewModel;
using NeTec.Kanban.Infrastructure.Data;

namespace NeTec.Kanban.Web.Controllers
{
    /// <summary>
    /// Controller für die Verwaltung von Boards und deren Spaltenstruktur.
    /// Beinhaltet die Logik für CRUD-Operationen auf Board-Ebene.
    /// </summary>
    public class BoardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BoardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ============================================================
        // VIEW ACTIONS
        // ============================================================

        /// <summary>
        /// Zeigt die Übersicht aller Boards an, die vom aktuellen Benutzer erstellt wurden.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Redirect("/Identity/Account/Login");

            // Performance-Optimierung: AsNoTracking wird verwendet, da die Daten nur gelesen werden.
            var boards = await _context.Boards
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return View(boards);
        }

        /// <summary>
        /// Sucht nach Boards anhand des Titels (innerhalb der eigenen Boards).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Redirect("/Identity/Account/Login");

            if (string.IsNullOrWhiteSpace(q)) return RedirectToAction(nameof(Index));

            var boards = await _context.Boards
                .Where(x => x.UserId == userId && x.Titel.Contains(q))
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            ViewData["SearchQuery"] = q;
            return View("Index", boards);
        }

        /// <summary>
        /// Lädt ein spezifisches Board inklusive Spalten und Aufgaben für die Kanban-Ansicht.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Redirect("/Identity/Account/Login");

            var board = await _context.Boards
                .Include(b => b.Columns)
                    .ThenInclude(c => c.Tasks)
                        .ThenInclude(t => t.AssignedTo)
                .AsNoTracking()
                .FirstOrDefaultAsync(b =>
                    b.Id == id &&
                    (
                        b.UserId == userId ||
                        b.Columns.Any(c => c.Tasks.Any(t => t.UserId == userId))
                    )
                );

            if (board == null)
            {
                TempData["ErrorMessage"] = "Board nicht gefunden oder Zugriff verweigert.";
                return RedirectToAction(nameof(Index));
            }

            return View("Kanban", board);
        }

        // ============================================================
        // BOARD CRUD OPERATIONS
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBoardViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Redirect("/Identity/Account/Login");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Ungültige Eingabedaten.";
                return RedirectToAction(nameof(Index));
            }

            var board = new Board
            {
                Titel = model.Titel,
                Description = model.Description,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Boards.Add(board);
            await _context.SaveChangesAsync();

            // Initialisierung der Standard-Spaltenstruktur
            await CreateDefaultColumnsInternal(board.Id);

            TempData["SuccessMessage"] = "Board erfolgreich erstellt.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditBoardViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Redirect("/Identity/Account/Login");

            var board = await _context.Boards.FindAsync(id);

            // Sicherheitsprüfung: Gehört das Board dem User?
            if (board == null || board.UserId != userId) return NotFound();

            if (ModelState.IsValid)
            {
                board.Titel = model.Titel ?? board.Titel;
                board.Description = model.Description;
                board.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Änderungen gespeichert.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Redirect("/Identity/Account/Login");

            var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (board == null) return NotFound();

            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Board wurde gelöscht.";
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // COLUMN MANAGEMENT
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateColumn(int boardId, string title)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Redirect("/Identity/Account/Login");

            var boardExists = await _context.Boards.AnyAsync(b => b.Id == boardId && b.UserId == userId);
            if (!boardExists) return NotFound();

            // Ermittlung der höchsten Position für das Einfügen am Ende
            var maxOrder = await _context.Columns
                .Where(c => c.BoardId == boardId)
                .MaxAsync(c => (int?)c.OrderIndex) ?? 0;

            _context.Columns.Add(new Column
            {
                BoardId = boardId,
                Titel = title,
                OrderIndex = maxOrder + 1
            });

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = boardId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteColumn(int columnId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Redirect("/Identity/Account/Login");

            var column = await _context.Columns
                .Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == columnId && c.Board.UserId == userId);

            if (column == null) return NotFound();

            int boardId = column.BoardId;
            _context.Columns.Remove(column);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = boardId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveColumn(int columnId, string direction)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Redirect("/Identity/Account/Login");

            var column = await _context.Columns
                .Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == columnId && c.Board.UserId == userId);

            if (column == null) return NotFound();

            var siblings = await _context.Columns
                .Where(c => c.BoardId == column.BoardId)
                .OrderBy(c => c.OrderIndex)
                .ToListAsync();

            var idx = siblings.IndexOf(column);

            // Tauschlogik für die Sortierreihenfolge
            if (direction == "left" && idx > 0)
            {
                (column.OrderIndex, siblings[idx - 1].OrderIndex) = (siblings[idx - 1].OrderIndex, column.OrderIndex);
            }
            else if (direction == "right" && idx < siblings.Count - 1)
            {
                (column.OrderIndex, siblings[idx + 1].OrderIndex) = (siblings[idx + 1].OrderIndex, column.OrderIndex);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = column.BoardId });
        }

        /// <summary>
        /// Hilfsmethode: Erstellt die initialen Spalten für ein neues Board.
        /// </summary>
        private async Task CreateDefaultColumnsInternal(int boardId)
        {
            _context.Columns.AddRange(
                new Column { BoardId = boardId, Titel = "To Do", OrderIndex = 1 },
                new Column { BoardId = boardId, Titel = "In Progress", OrderIndex = 2 },
                new Column { BoardId = boardId, Titel = "Done", OrderIndex = 3 }
            );
            await _context.SaveChangesAsync();
        }
    }
}