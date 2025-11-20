using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeTec.Kanban.Domain.Entities;
using NeTec.Kanban.Domain.Entities.ViewModel;
using NeTec.Kanban.Infrastructure.Data;

namespace NeTec.Kanban.Web.Controllers
{
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
        // 1. BOARD ÜBERSICHT & SUCHE
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToLogin();

            // AsNoTracking() ist schneller, wenn wir die Daten nur lesen (nicht bearbeiten)
            var boards = await _context.Boards
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return View(boards);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToLogin();

            // Wenn Suchbegriff leer ist, zeigen wir einfach die normale Liste
            if (string.IsNullOrWhiteSpace(q))
            {
                return RedirectToAction(nameof(Index));
            }

            var boards = await _context.Boards
                .Where(x => x.UserId == userId && x.Titel.Contains(q))
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            ViewData["SearchQuery"] = q;
            return View("Index", boards); // Wir nutzen die gleiche View wie Index
        }

        // ============================================================
        // 2. BOARD DETAILS (KANBAN VIEW)
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToLogin();

            // Hier laden wir ALLES: Board -> Spalten -> Aufgaben -> Zugewiesene User
            // Das ist nötig, damit das Board vollständig angezeigt werden kann.
            var board = await _context.Boards
                .Include(b => b.Columns)
                    .ThenInclude(c => c.Tasks)
                        .ThenInclude(t => t.AssignedTo)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (board == null)
            {
                TempData["ErrorMessage"] = "Board nicht gefunden.";
                return RedirectToAction(nameof(Index));
            }

            return View("Kanban", board);
        }

        // ============================================================
        // 3. CRUD AKTIONEN (Erstellen, Bearbeiten, Löschen)
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBoardViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToLogin();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Ungültige Eingabe.";
                return RedirectToAction(nameof(Index));
            }

            var board = new Board
            {
                Titel = model.Titel,
                UserId = userId!,
                CreatedAt = DateTime.UtcNow
            };

            _context.Boards.Add(board);
            await _context.SaveChangesAsync(); // Board speichern, damit es eine ID bekommt

            // Automatisch die 3 Standard-Spalten anlegen
            await CreateDefaultColumnsInternal(board.Id);

            TempData["SuccessMessage"] = "Board erfolgreich erstellt!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditBoardViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToLogin();

            // Wir laden nur das Board, um zu prüfen, ob es dem User gehört
            var board = await _context.Boards.FindAsync(id);

            if (board == null || board.UserId != userId)
            {
                TempData["ErrorMessage"] = "Zugriff verweigert oder nicht gefunden.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                board.Titel = model.Titel;
                board.Description = model.Description;
                // UpdatedAt setzen wäre hier guter Stil:
                // board.UpdatedAt = DateTime.UtcNow; 

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Board aktualisiert!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToLogin();

            var board = await _context.Boards
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (board == null) return NotFound();

            try
            {
                _context.Boards.Remove(board);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Board gelöscht.";
            }
            catch
            {
                TempData["ErrorMessage"] = "Fehler beim Löschen.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // 4. SPALTEN LOGIK
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateColumn(int boardId, string title)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToLogin();

            if (string.IsNullOrWhiteSpace(title))
                return BadRequest("Titel fehlt.");

            // Sicherheitscheck: Gehört das Board dem User?
            var boardExists = await _context.Boards
                .AnyAsync(b => b.Id == boardId && b.UserId == userId);

            if (!boardExists) return NotFound();

            // Höchsten OrderIndex finden, damit die neue Spalte ganz rechts landet
            var maxOrder = await _context.Columns
                .Where(c => c.BoardId == boardId)
                .MaxAsync(c => (int?)c.OrderIndex) ?? 0;

            var column = new Column
            {
                BoardId = boardId,
                Titel = title,
                OrderIndex = maxOrder + 1
            };

            _context.Columns.Add(column);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = boardId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteColumn(int columnId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToLogin();

            var column = await _context.Columns
                .Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == columnId && c.Board.UserId == userId);

            if (column == null) return NotFound();

            int boardId = column.BoardId; // ID merken für Redirect

            _context.Columns.Remove(column);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = boardId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveColumn(int columnId, string direction)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToLogin();

            var column = await _context.Columns
                .Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == columnId && c.Board.UserId == userId);

            if (column == null) return NotFound();

            // Alle Nachbarn laden
            var siblings = await _context.Columns
                .Where(c => c.BoardId == column.BoardId)
                .OrderBy(c => c.OrderIndex)
                .ToListAsync();

            var currentIndex = siblings.IndexOf(column);

            // Logik zum Tauschen der Reihenfolge (Swap)
            if (direction == "left" && currentIndex > 0)
            {
                var neighbor = siblings[currentIndex - 1];
                (column.OrderIndex, neighbor.OrderIndex) = (neighbor.OrderIndex, column.OrderIndex);
            }
            else if (direction == "right" && currentIndex < siblings.Count - 1)
            {
                var neighbor = siblings[currentIndex + 1];
                (column.OrderIndex, neighbor.OrderIndex) = (neighbor.OrderIndex, column.OrderIndex);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = column.BoardId });
        }

        // ============================================================
        // 5. INTERNE HILFSMETHODEN
        // ============================================================

        /// <summary>
        /// Erstellt die Standard-Spalten (To Do, In Progress, Done)
        /// </summary>
        private async Task CreateDefaultColumnsInternal(int boardId)
        {
            // Wir brauchen hier KEIN Board laden. Die ID reicht.
            // Das spart einen Datenbank-Zugriff.
            var columns = new[]
            {
                new Column { BoardId = boardId, Titel = "To Do", OrderIndex = 1 },
                new Column { BoardId = boardId, Titel = "In Progress", OrderIndex = 2 },
                new Column { BoardId = boardId, Titel = "Done", OrderIndex = 3 }
            };

            _context.Columns.AddRange(columns);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Zentraler Redirect zum Login mit Fehlermeldung
        /// </summary>
        private IActionResult RedirectToLogin()
        {
            TempData["ErrorMessage"] = "Bitte anmelden.";
            return Redirect("/Identity/Account/Login");
        }
    }
}