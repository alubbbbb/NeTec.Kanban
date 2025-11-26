using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeTec.Kanban.Domain.Entities;
using NeTec.Kanban.Domain.Entities.ViewModel;
using NeTec.Kanban.Infrastructure.Data;
using NeTec.Kanban.Application.DTOs;

namespace NeTec.Kanban.Web.Controllers
{
    /// <summary>
    /// Controller zur Verwaltung von Aufgaben (Tasks).
    /// Implementiert Dashboard-Ansichten, CRUD-Logik und API-Endpunkte für Drag & Drop.
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
        // VIEWS & DASHBOARDS
        // ============================================================

        /// <summary>
        /// Zeigt ein Dashboard aller Aufgaben an, die dem aktuellen Benutzer zugewiesen sind.
        /// Diese Ansicht aggregiert Aufgaben über alle Boards hinweg.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MyTasks()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Redirect("/Identity/Account/Login");

            var myTasks = await _context.TaskItems
                .Include(t => t.Column).ThenInclude(c => c.Board)
                .Include(t => t.AssignedTo)
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.DueDate)
                .AsNoTracking()
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            return View(myTasks);
        }

        /// <summary>
        /// Lädt die Detailansicht einer Aufgabe.
        /// Berechtigung: Zugriff haben Board-Besitzer ODER zugewiesene Bearbeiter.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Redirect("/Identity/Account/Login");

            var task = await _context.TaskItems
                .Include(t => t.Column).ThenInclude(c => c.Board)
                .Include(t => t.AssignedTo)
                .Include(t => t.Comments).ThenInclude(c => c.User)
                .AsNoTracking()
                // Erweiterte Prüfung: Owner oder Assignee
                .FirstOrDefaultAsync(t => t.Id == id && (t.Column.Board.UserId == userId || t.UserId == userId));

            if (task == null) return NotFound();

            // ViewModel Mapping
            var vm = new TaskDetailsViewModel
            {
                TaskId = task.Id,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                DueDate = task.DueDate,
                PlannedTime = task.EstimatedHours,
                ActualTime = task.RemainingHours,

                ColumnName = task.Column?.Titel ?? "Unbekannt",
                ColumnId = task.ColumnId,
                BoardId = task.Column.BoardId,

                AssignedUserId = task.UserId,
                AssignedUserName = task.AssignedTo?.FullName ?? task.AssignedTo?.UserName ?? "Nicht zugewiesen",

                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,

                Comments = task.Comments.OrderByDescending(c => c.CreatedAt).Select(c => new TaskCommentViewModel
                {
                    UserName = c.User?.FullName ?? c.User?.UserName ?? "Unbekannt",
                    Text = c.Content,
                    CreatedAt = c.CreatedAt
                }).ToList()
            };

            return View(vm);
        }

        // ============================================================
        // INTERAKTIONEN (Kommentare, Löschen)
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int taskId, string text)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Redirect("/Identity/Account/Login");

            if (string.IsNullOrWhiteSpace(text)) return RedirectToAction("Details", new { id = taskId });

            // Prüfung, ob Benutzer berechtigt ist, auf diesem Ticket zu kommentieren
            var taskExists = await _context.TaskItems
                .Include(t => t.Column).ThenInclude(c => c.Board)
                .AnyAsync(t => t.Id == taskId && (t.Column.Board.UserId == userId || t.UserId == userId));

            if (!taskExists) return Unauthorized();

            _context.Comments.Add(new Comment
            {
                TaskItemId = taskId,
                UserId = userId,
                Content = text,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = taskId });
        }

        /// <summary>
        /// Löscht eine Aufgabe. Nur der Board-Besitzer ist hierzu berechtigt.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Redirect("/Identity/Account/Login");

            var task = await _context.TaskItems
                .Include(t => t.Column).ThenInclude(c => c.Board)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            bool isAuthorized = task.Column.Board.UserId == userId || task.UserId == userId;

            if (!isAuthorized)
            {
                return Unauthorized();
            }

            int boardId = task.Column.BoardId;
            _context.TaskItems.Remove(task);
            TempData["SuccessMessage"] = "Ticket gelöscht"; 
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Board", new { id = boardId });
        }

        // ============================================================
        // API ENDPOINTS (AJAX/JSON für Frontend-Interaktion)
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> GetTask(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var t = await _context.TaskItems
                .Include(t => t.Column).ThenInclude(c => c.Board)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && (x.Column.Board.UserId == userId || x.UserId == userId));

            if (t == null) return NotFound(); // Wenn keine Berechtigung -> Nicht gefunden

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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EditTaskRequest req)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();
            if (string.IsNullOrWhiteSpace(req.Title)) return BadRequest("Titel fehlt.");

            // Prüfen, ob User Zugriff auf das Board hat
            var column = await _context.Columns
                .Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == req.ColumnId && c.Board!.UserId == userId);

            if (column == null) return NotFound();

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

        [HttpPost]
        public async Task<IActionResult> EditTask([FromBody] EditTaskRequest req)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var t = await _context.TaskItems
                .Include(x => x.Column).ThenInclude(c => c.Board)
                .FirstOrDefaultAsync(x => x.Id == req.Id && (x.Column.Board.UserId == userId || x.UserId == userId));

            if (t == null) return NotFound();

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

        [HttpPost]
        public async Task<IActionResult> UpdateTaskColumn([FromBody] UpdateTaskRequest request)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var task = await _context.TaskItems
                .Include(t => t.Column).ThenInclude(c => c.Board)
                .FirstOrDefaultAsync(t => t.Id == request.TaskId && (t.Column.Board.UserId == userId || t.UserId == userId));

            if (task == null) return NotFound();

            // Spaltenwechsel: Sicherstellen, dass Zielspalte valide ist
            if (task.ColumnId != request.NewColumnId)
            {
                var targetCol = await _context.Columns
                    .Include(c => c.Board)
                    .FirstOrDefaultAsync(c => c.Id == request.NewColumnId);

                // Sicherheit: Zielspalte muss zum selben Board gehören (oder User ist Owner)
                if (targetCol == null || (targetCol.BoardId != task.Column.BoardId && targetCol.Board.UserId != userId))
                    return NotFound();

                task.ColumnId = request.NewColumnId;
            }

            // Neusortierung via Pattern Matching
            if (request.NewOrderIndex is int newIndex && newIndex >= 0)
            {
                var siblings = await _context.TaskItems
                    .Where(t => t.ColumnId == request.NewColumnId && t.Id != task.Id)
                    .OrderBy(t => t.OrderIndex)
                    .ToListAsync();

                for (int i = 0; i < siblings.Count; i++)
                {
                    siblings[i].OrderIndex = (i >= newIndex) ? i + 1 : i;
                }
                task.OrderIndex = newIndex;
            }

            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Liefert die Liste der Benutzer für Dropdowns.
        /// Bevorzugt den vollen Namen (FullName) vor dem Benutzernamen (Email).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAssignableUsers()
        {
            var users = await _context.Users
                .AsNoTracking()
                .Select(u => new
                {
                    id = u.Id,
                    userName = (u.FullName != null && u.FullName != "") ? u.FullName : u.UserName
                })
                .OrderBy(x => x.userName)
                .ToListAsync();

            return Json(users);
        }
    }
}