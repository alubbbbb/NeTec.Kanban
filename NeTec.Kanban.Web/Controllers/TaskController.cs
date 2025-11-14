using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeTec.Kanban.Domain.Entities;
using NeTec.Kanban.Infrastructure.Data;

namespace NeTec.Kanban.Web.Controllers
{
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================
        // Drag & Drop: Spalte ändern + Reihenfolge setzen
        // =========================
        [HttpPost]
        public async Task<IActionResult> UpdateTaskColumn([FromBody] UpdateTaskRequest request)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            var task = await _context.TaskItems
                .Include(t => t.Column)
                .ThenInclude(c => c.Board)
                .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.Column!.Board!.UserId == currentUserId);

            if (task == null) return NotFound("Aufgabe nicht gefunden.");

            // Zielspalte validieren (gehört dem selben User?)
            var targetColumn = await _context.Columns
                .Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == request.NewColumnId && c.Board!.UserId == currentUserId);

            if (targetColumn == null) return NotFound("Zielspalte nicht gefunden.");

            // Spalte setzen
            task.ColumnId = request.NewColumnId;

            // Neuen OrderIndex bestimmen (entweder vom Client übergeben oder ans Ende)
            if (request.NewOrderIndex.HasValue && request.NewOrderIndex.Value >= 0)
            {
                // Platz schaffen: alle Aufgaben in der Zielspalte ab NewOrderIndex nach hinten schieben
                var siblings = await _context.TaskItems
                    .Where(t => t.ColumnId == request.NewColumnId && t.Id != task.Id)
                    .OrderBy(t => t.OrderIndex)
                    .ToListAsync();

                int desired = request.NewOrderIndex.Value;
                for (int i = 0; i < siblings.Count; i++)
                {
                    if (i >= desired) siblings[i].OrderIndex = i + 1;
                    else siblings[i].OrderIndex = i;
                }
                task.OrderIndex = desired;
            }
            else
            {
                // Kein Index mitgegeben → ans Ende
                var maxOrder = await _context.TaskItems
                    .Where(t => t.ColumnId == request.NewColumnId)
                    .MaxAsync(t => (int?)t.OrderIndex) ?? 0;
                task.OrderIndex = maxOrder + 1;
            }

            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest("Titel ist erforderlich.");

            // Spalte validieren (dem User zugeordnet?)
            var column = await _context.Columns
                .Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == request.ColumnId && c.Board!.UserId == currentUserId);

            if (column == null) return NotFound(new { message = "Spalte nicht gefunden." });

            // Neuen OrderIndex ans Ende setzen
            var maxOrder = await _context.TaskItems
                .Where(t => t.ColumnId == request.ColumnId)
                .MaxAsync(t => (int?)t.OrderIndex) ?? 0;

            var task = new TaskItem
            {
                Title = request.Title.Trim(),
                Description = request.Description,
                ColumnId = request.ColumnId,
                UserId = currentUserId,
                Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Medium" : request.Priority,
                EstimatedHours = request.EstimatedHours,
                RemainingHours = request.RemainingHours,
                OrderIndex = maxOrder + 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = task.Id,
                title = task.Title,
                priority = task.Priority,
                columnId = task.ColumnId,
                orderIndex = task.OrderIndex
            });
        }

        // =========================
        // Kommentar hinzufügen
        // =========================
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] AddCommentRequest request)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Kommentar darf nicht leer sein.");

            var taskExists = await _context.TaskItems.AnyAsync(t => t.Id == request.TaskId);
            if (!taskExists) return NotFound("Aufgabe nicht gefunden.");

            var comment = new Comment
            {
                TaskItemId = request.TaskId,
                UserId = currentUserId,
                Content = request.Text.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new { id = comment.Id, createdAt = comment.CreatedAt });
        }

        // =========================
        // Zeit buchen
        // =========================
        [HttpPost]
        public async Task<IActionResult> AddTimeEntry([FromBody] AddTimeEntryRequest request)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            if (request.HoursSpent < 0.01m)
                return BadRequest("Mindestens 0.01 Stunden.");

            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == request.TaskId);
            if (task == null) return NotFound("Aufgabe nicht gefunden.");

            var te = new TimeTracking
            {
                TaskItemId = request.TaskId,
                UserId = currentUserId,
                HoursSpent = request.HoursSpent,
                Description = request.Note,
                LoggedAt = DateTime.UtcNow
            };

            // Ist-Zeit aufsummieren
            var currentActual = task.RemainingHours.HasValue && task.EstimatedHours.HasValue
                ? (task.EstimatedHours.Value - task.RemainingHours.Value)
                : 0m;

            var newActual = currentActual + request.HoursSpent;
            if (task.EstimatedHours.HasValue)
            {
                var remaining = task.EstimatedHours.Value - newActual;
                task.RemainingHours = remaining < 0 ? 0 : remaining;
            }

            task.UpdatedAt = DateTime.UtcNow;

            _context.TimeTrackings.Add(te);
            await _context.SaveChangesAsync();

            return Ok(new { id = te.Id, remaining = task.RemainingHours });
        }

       
    }

}