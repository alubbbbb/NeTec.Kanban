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

        [HttpPost]
        public async Task<IActionResult> UpdateTaskColumn([FromBody] UpdateTaskRequest request)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            var task = await _context.TaskItems
                .Include(t => t.Column)
                .ThenInclude(c => c.Board)
                .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.Column.Board.UserId == currentUserId);

            if (task == null)
            {
                return NotFound();
            }

            task.ColumnId = request.NewColumnId;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            // Spalte ohne User-Filter laden
            var column = await _context.Columns
                .Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == request.ColumnId);

            if (column == null)
            {
                return NotFound(new { message = "Spalte nicht gefunden." });
            }

            // Sicherheits-Check erst nach dem Laden
            if (column.Board.UserId != currentUserId)
            {
                return Forbid(); // anstatt 404 → klareres Feedback
            }

            var task = new TaskItem
            {
                Title = request.Title,
                ColumnId = request.ColumnId,
                UserId = currentUserId,
                Priority = "Medium",
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = task.Id,
                title = task.Title,
                priority = task.Priority,
                columnId = task.ColumnId
            });
        }
    }

        public class UpdateTaskRequest
    {
        public int TaskId { get; set; }
        public int NewColumnId { get; set; }
    }

    public class CreateTaskRequest
    {
        public string Title { get; set; } = string.Empty;
        public int ColumnId { get; set; }
    }
}