using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeTec.Kanban.Domain.Entities;
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

        // ------------------------------
        // INDEX → zeigt alle Boards des aktuellen Benutzers
        // ------------------------------
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            if (currentUserId == null)
            {
                TempData["ErrorMessage"] = "Bitte melde dich an, um deine Boards zu sehen.";
                // 🚀 FALSCH: RedirectToAction("Login", "Account")
                // ✅ RICHTIG:
                return Redirect("/Identity/Account/Login");
            }

            var boards = await _context.Boards
                .Where(b => b.UserId == currentUserId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(boards);
        }

        // ------------------------------
        // POST: Board erstellen
        // ------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Board board)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (currentUserId == null)
            {
                TempData["ErrorMessage"] = "Benutzer ist nicht angemeldet.";
                return Redirect("/Identity/Account/Login");
            }

            if (string.IsNullOrWhiteSpace(board.Titel))
            {
                TempData["ErrorMessage"] = "Bitte gib einen gültigen Namen für das Board ein.";
                return RedirectToAction(nameof(Index));
            }

             if (ModelState.IsValid)
            {
                board.UserId = currentUserId;
                board.CreatedAt = DateTime.Now;

                _context.Boards.Add(board);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Board erfolgreich erstellt!";
                return RedirectToAction(nameof(Index));
            }

            // Wenn Validierung fehlschlägt → Log + Toastr
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            Console.WriteLine("ModelState Fehler: " + errors);

            TempData["ErrorMessage"] = "Fehler beim Erstellen des Boards.";
            return RedirectToAction(nameof(Index));
        }

        // ------------------------------
        // BOARD DETAILSEITE
        // ------------------------------
        public async Task<IActionResult> Kanban(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (currentUserId == null)
            {
                TempData["ErrorMessage"] = "Bitte melde dich an, um deine Boards zu öffnen.";
                return Redirect("/Identity/Account/Login");
            }

            var board = await _context.Boards
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == currentUserId);

            if (board == null)
            {
                TempData["ErrorMessage"] = "Board wurde nicht gefunden.";
                return RedirectToAction(nameof(Index));
            }

            return View(board);
        }
    }
}
