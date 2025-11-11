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

        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                TempData["ErrorMessage"] = "Bitte melde dich an, um deine Boards zu sehen.";
                return Redirect("/Identity/Account/Login");
            }

            var boards = await _context.Boards
                .Where(b => b.UserId == currentUserId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(boards);
        }
        public async Task<IActionResult> Search(string q)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) 
            {
                return Redirect("/Identity/Account/Login");
            }

            if (string.IsNullOrEmpty(q))
            {
                var defaultBoard = await _context.Boards
                    .Where(x => x.UserId == currentUserId)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync(); 
                return View("Index", defaultBoard);
            }

            var boards = await _context.Boards
                .Where(x => x.UserId == currentUserId && x.Titel.Contains(q))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(); // ← ToListAsync statt ToArrayAsync

            ViewData["SearchQuery"] = q;
            return View("Index", boards);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                TempData["ErrorMessage"] = "Bitte melde dich an.";
                return Redirect("/Identity/Account/Login");
            }

            var board = await _context.Boards
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == currentUserId);

            if (board == null)
            {
                TempData["ErrorMessage"] = "Board wurde nicht gefunden oder Sie haben keine Berechtigung.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Boards.Remove(board);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Board '{board.Titel}' erfolgreich gelöscht!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Fehler beim Löschen des Boards. Bitte versuchen Sie es erneut.";
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditBoardViewModel model)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                TempData["ErrorMessage"] = "Bitte melde dich an.";
                return Redirect("/Identity/Account/Login");
            }

            var board = await _context.Boards
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == currentUserId);

            if (board == null)
            {
                TempData["ErrorMessage"] = "Board wurde nicht gefunden.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                board.Titel = model.Titel;
                board.Description = model.Description;

                _context.Update(board);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Board '{board.Titel}' erfolgreich aktualisiert!";
            }
            else
            {
                TempData["ErrorMessage"] = "Bitte korrigieren Sie die Eingabefehler.";
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBoardViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = _userManager.GetUserId(User);
                if (currentUserId == null)
                {
                    TempData["ErrorMessage"] = "Sitzung abgelaufen. Bitte erneut anmelden.";
                    return Redirect("/Identity/Account/Login");
                }

                var board = new Board
                {
                    Titel = model.Titel,
                    UserId = currentUserId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Boards.Add(board);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Board erfolgreich erstellt!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Bitte gib einen gültigen Namen für das Board ein.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                TempData["ErrorMessage"] = "Bitte melde dich an, um deine Boards zu öffnen.";
                return Redirect("/Identity/Account/Login");
            }

            var board = await _context.Boards
                .Include(b => b.Columns)
                    .ThenInclude(c => c.Tasks)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == currentUserId);

            if (board == null)
            {
                TempData["ErrorMessage"] = "Board wurde nicht gefunden.";
                return RedirectToAction(nameof(Index));
            }

            return View("Kanban", board);
        }
    }
}