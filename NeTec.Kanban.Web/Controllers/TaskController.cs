using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeTec.Kanban.Domain.Entities;
using NeTec.Kanban.Domain.Entities.ViewModel;
using NeTec.Kanban.Infrastructure.Data;

namespace NeTec.Kanban.Web.Controllers
{
    /// <summary>
    /// Controller zur Verwaltung von Aufgaben (Tasks).
    /// Behandelt die Anzeige von Details, CRUD-Operationen sowie API-Endpunkte
    /// für Drag & Drop und asynchrone Bearbeitung.
    /// </summary>
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ============================================================
        // MVC VIEW ACTIONS (Seitenaufrufe)
        // ============================================================

        /// <summary>
        /// Lädt die Detailansicht einer spezifischen Aufgabe inklusive aller relationalen Daten
        /// (Spalte, Board, zugewiesener Benutzer, Kommentare).
        /// </summary>
        /// <param name="id">Die ID der anzuzeigenden Aufgabe.</param>
        /// <returns>View mit TaskDetailsViewModel oder NotFound.</returns>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Redirect("/Identity/Account/Login");

            // Eager Loading der Entität zur Vermeidung von N+1 Problemen.
            // AsNoTracking wird zur Performance-Optimierung verwendet, da hier nur Lesezugriff erfolgt.
            var task = await _context.TaskItems
                .Include(t => t.Column).ThenInclude(c => c.Board)
                .Include(t => t.AssignedTo)
                .Include(t => t.Comments).ThenInclude(c => c.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && t.Column.Board.UserId == userId);

            if (task == null) return NotFound();

            // Mapping auf ViewModel zur Entkopplung von Datenbank-Entität und Präsentationsschicht
            var vm = new TaskDetailsViewModel
            {
                TaskId = task.Id,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                DueDate = task.DueDate,
                PlannedTime = task.EstimatedHours,
                ActualTime = task.RemainingHours,
                ColumnName = task.Column.Titel,
                ColumnId = task.ColumnId,
                BoardId = task.Column.BoardId,
                AssignedUserId = task.UserId,
                AssignedUserName = task.AssignedTo?.UserName ?? "Nicht zugewiesen",
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                FullName = task.AssignedTo.FullName,
                Comments = task.Comments
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new TaskCommentViewModel
                    {
                        UserName = c.User?.UserName ?? "Unbekannt",
                        Text = c.Content,
                        CreatedAt = c.CreatedAt
                    }).ToList()
            };

            return View(vm);
        }

        /// <summary>
        /// Fügt einen neuen Kommentar zu einer Aufgabe hinzu.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int taskId, string text)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Redirect("/Identity/Account/Login");

            if (string.IsNullOrWhiteSpace(text))
                return RedirectToAction("Details", new { id = taskId });

            // Validierung: Existiert der Task und hat der Benutzer Zugriff darauf?
            // AnyAsync ist performanter als das Laden des gesamten Objekts.
            var taskExists = await _context.TaskItems
                .Include(t => t.Column).ThenInclude(c => c.Board)
                .AnyAsync(t => t.Id == taskId && t.Column.Board.UserId == userId);

            if (!taskExists) return Unauthorized();

            var comment = new Comment
            {
                TaskItemId = taskId,
                UserId = userId,
                Content = text,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = taskId });
        }

        // ============================================================
        // API ENDPOINTS (AJAX / JSON)
        // ============================================================

        /// <summary>
        /// API: Liefert die Rohdaten einer Aufgabe für das Edit-Modal (Client-Side Rendering).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTask(int id)
        {
            var t = await _context.TaskItems
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (t == null) return NotFound();

            return Json(new
            {
                id = t.Id,
                title = t.Title,
                description = t.Description,
                priority = t.Priority,
                dueDate = t.DueDate?.ToString("yyyy-MM-dd"),
                plannedTime = t.EstimatedHours,
                actualTime = t.RemainingHours,
                assignedUserId = t.UserId,
                columnId = t.ColumnId
            });
        }

        /// <summary>
        /// API: Erstellt eine neue Aufgabe basierend auf JSON-Daten.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EditTaskRequest req)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(req.Title))
                return BadRequest("Titel erforderlich.");

            // Validierung der Spaltenzugehörigkeit (Sicherheitsaspekt)
            var column = await _context.Columns
                .Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == req.ColumnId && c.Board!.UserId == userId);

            if (column == null) return NotFound("Spalte nicht gefunden oder kein Zugriff.");

            // Ermittlung des aktuellen Max-Index zur korrekten Einsortierung am Ende der Liste
            var maxOrder = await _context.TaskItems
                .Where(t => t.ColumnId == req.ColumnId)
                .MaxAsync(t => (int?)t.OrderIndex) ?? 0;

            var task = new TaskItem
            {
                Title = req.Title.Trim(),
                Description = req.Description,
                Priority = req.Priority ?? "Medium",
                DueDate = req.DueDate,
                EstimatedHours = req.PlannedTime,
                RemainingHours = req.ActualTime,
                UserId = req.AssignedUserId,
                ColumnId = req.ColumnId,
                OrderIndex = maxOrder + 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            return Ok(new { id = task.Id });
        }

        /// <summary>
        /// API: Aktualisiert die Stammdaten einer Aufgabe.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> EditTask([FromBody] EditTaskRequest req)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(req.Title))
                return BadRequest("Titel erforderlich.");

            var t = await _context.TaskItems
                .Include(x => x.Column).ThenInclude(c => c.Board)
                .FirstOrDefaultAsync(x => x.Id == req.Id && x.Column.Board.UserId == userId);

            if (t == null) return NotFound();

            // Aktualisierung der Eigenschaften
            t.Title = req.Title;
            t.Description = req.Description;
            t.Priority = req.Priority ?? "Medium";
            t.DueDate = req.DueDate;
            t.EstimatedHours = req.PlannedTime;
            t.RemainingHours = req.ActualTime;
            t.UserId = req.AssignedUserId;
            t.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { id = t.Id });
        }

        /// <summary>
        /// API: Verarbeitet Drag & Drop Aktionen.
        /// Behandelt sowohl das Verschieben in eine andere Spalte als auch die Neusortierung innerhalb einer Spalte.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateTaskColumn([FromBody] UpdateTaskRequest request)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var task = await _context.TaskItems
                .Include(t => t.Column).ThenInclude(c => c.Board)
                .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.Column.Board.UserId == userId);

            if (task == null) return NotFound();

            // 1. Verarbeitung von Spaltenwechseln
            if (task.ColumnId != request.NewColumnId)
            {
                // Validierung der Zielspalte
                var targetExists = await _context.Columns
                    .Include(c => c.Board)
                    .AnyAsync(c => c.Id == request.NewColumnId && c.Board.UserId == userId);

                if (!targetExists) return NotFound();

                task.ColumnId = request.NewColumnId;
            }

            // 2. Verarbeitung der Neusortierung (Reordering)
            // Pattern Matching (is int newIndex) prüft auf null und castet sicher in einem Schritt.
            if (request.NewOrderIndex is int newIndex && newIndex >= 0)
            {
                // Laden der betroffenen Aufgaben in der Zielspalte zur Index-Verschiebung
                var siblings = await _context.TaskItems
                    .Where(t => t.ColumnId == request.NewColumnId && t.Id != task.Id)
                    .OrderBy(t => t.OrderIndex)
                    .ToListAsync();

                // Anpassung der Sortierreihenfolge nachfolgender Aufgaben
                for (int i = 0; i < siblings.Count; i++)
                {
                    // Alle Aufgaben ab der neuen Position werden um 1 nach unten verschoben
                    siblings[i].OrderIndex = (i >= newIndex) ? i + 1 : i;
                }

                // Zuweisung der neuen Position
                task.OrderIndex = newIndex;
            }

            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// API: Liefert Benutzerliste für Zuweisungs-Dropdowns.
        /// Priorisiert den vollen Namen. Falls dieser fehlt, wird der Benutzername (Email) genutzt.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAssignableUsers()
        {
            var users = await _context.Users
                .AsNoTracking()
                .Select(u => new
                {
                    id = u.Id,
                    // prüfen auf null UND auf leeren String.
                    // Wenn FullName da ist -> nimm FullName. Sonst -> nimm UserName.
                    userName = (u.FullName != null && u.FullName != "") ? u.FullName : u.UserName
                })
                // Optional: Sortieren damit es im Dropdown alphabetisch ist
                .OrderBy(x => x.userName)
                .ToListAsync();

            return Json(users);
        }
        /// <summary>
        /// Löscht eine spezifische Aufgabe und leitet zur Board-Ansicht zurück.
        /// </summary>
        /// <param name="id">Die ID der zu löschenden Aufgabe.</param>
        /// <returns>Redirect zur Board-Ansicht oder Fehlerstatus.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Redirect("/Identity/Account/Login");

            // Laden der Aufgabe inklusive Spalten-Informationen, um die Board-ID für den Redirect zu erhalten.
            // Gleichzeitig wird geprüft, ob der Benutzer berechtigt ist (Besitzer des Boards).
            var task = await _context.TaskItems
                .Include(t => t.Column)
                .ThenInclude(c => c.Board)
                .FirstOrDefaultAsync(t => t.Id == id && t.Column.Board.UserId == userId);

            if (task == null)
            {
                // Aufgabe nicht gefunden oder Benutzer hat keine Berechtigung.
                return NotFound();
            }

            // Speichern der Board-ID für den Redirect, bevor das Objekt gelöscht wird.
            int boardId = task.Column.BoardId;

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();

            // Rückleitung zur Detailansicht des Boards (Kanban-Board).
            return RedirectToAction("Details", "Board", new { id = boardId });
        }
    }
}