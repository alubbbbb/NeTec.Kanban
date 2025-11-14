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

        // -------------------------------------------------------
        // USERS für "Zuständig"
        // -------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAssignableUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    id = u.Id,
                    userName = u.UserName
                })
                .ToListAsync();

            return Json(users);
        }

        // -------------------------------------------------------
        // DRAG & DROP Column-Update
        // -------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> UpdateTaskColumn([FromBody] UpdateTaskRequest request)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var task = await _context.TaskItems
                .Include(t => t.Column)
                .ThenInclude(c => c.Board)
                .FirstOrDefaultAsync(t =>
                    t.Id == request.TaskId &&
                    t.Column!.Board!.UserId == userId);

            if (task == null) return NotFound();

            var targetColumn = await _context.Columns
                .Include(c => c.Board)
                .FirstOrDefaultAsync(c =>
                    c.Id == request.NewColumnId &&
                    c.Board!.UserId == userId);

            if (targetColumn == null) return NotFound();

            task.ColumnId = request.NewColumnId;

            if (request.NewOrderIndex is int newIndex && newIndex >= 0)
            {
                var siblings = await _context.TaskItems
                    .Where(t => t.ColumnId == request.NewColumnId && t.Id != task.Id)
                    .OrderBy(t => t.OrderIndex)
                    .ToListAsync();

                for (int i = 0; i < siblings.Count; i++)
                    siblings[i].OrderIndex = (i >= newIndex) ? i + 1 : i;

                task.OrderIndex = newIndex;
            }
            else
            {
                var max = await _context.TaskItems
                    .Where(t => t.ColumnId == request.NewColumnId)
                    .MaxAsync(t => (int?)t.OrderIndex) ?? 0;

                task.OrderIndex = max + 1;
            }

            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // -------------------------------------------------------
        // TASK erstellen
        // -------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EditTaskRequest req)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(req.Title))
                return BadRequest("Titel erforderlich.");

            var column = await _context.Columns
                .Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == req.ColumnId && c.Board!.UserId == userId);

            if (column == null)
                return NotFound("Spalte nicht gefunden.");

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

        // -------------------------------------------------------
        // TASK Laden
        // -------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetTask(int id)
        {
            var t = await _context.TaskItems
                .Include(x => x.Column)
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

        // -------------------------------------------------------
        // TASK bearbeiten
        // -------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> EditTask([FromBody] EditTaskRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Title))
                return BadRequest("Titel erforderlich.");

            var t = await _context.TaskItems.FindAsync(req.Id);
            if (t == null)
                return NotFound();

            t.Title = req.Title;
            t.Description = req.Description;
            t.Priority = req.Priority ?? "Medium";
            t.DueDate = req.DueDate;
            t.EstimatedHours = req.PlannedTime;
            t.RemainingHours = req.ActualTime;
            t.UserId = req.AssignedUserId;
            t.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
