using Microsoft.AspNetCore.Authorization; // Nötig für ErrorViewModel
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeTec.Kanban.Domain.Entities;
using NeTec.Kanban.Domain.Entities.ViewModel;
using NeTec.Kanban.Infrastructure.Data;
using NeTec.Kanban.Web.Models;
using System.Diagnostics;

namespace NeTec.Kanban.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext db, UserManager<ApplicationUser> useManager)
        {
            _db = db;
            _userManager = useManager;

        }

        // Zeigt alle Boards an (deine Index-Seite)
        public async Task<IActionResult> Index()
        {
            // KORREKTUR: Nur die Boards des angemeldeten Benutzers anzeigen
            var currentUserId = _userManager.GetUserId(User);
            var boards = await _db.Boards.Where(b => b.UserId == currentUserId).ToListAsync();
            return View(boards);
        }

        // Zeigt die "Neues Board erstellen"-Seite
        public IActionResult Create()
        {
            // Die View bekommt jetzt ein leeres ViewModel
            return View(new BoardCreateVM());
        }

        // Verarbeitet das Erstellen eines neuen Boards
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BoardCreateVM viewModel)
        {
            // ModelState wird jetzt GÜLTIG sein, da das ViewModel nur die Felder enthält,
            // die auch vom Formular gesendet werden.
            if (ModelState.IsValid)
            {
                // Hole den angemeldeten Benutzer
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                // Erstelle das eigentliche Board-Objekt für die Datenbank
                var board = new Board
                {
                    Titel = viewModel.BoardName,
                    User = user, // Weise das volle User-Objekt zu
                    CreatedAt = DateTime.UtcNow // Setze das Erstellungsdatum
                };

                // Fügt Standardspalten hinzu
                board.Columns = new List<Column>
                {
                    new Column { Titel = "Aufgaben" },
                    new Column { Titel = "Wird ausgeführt" },
                    new Column { Titel = "Fertig" }
                };

                _db.Boards.Add(board);

                // KORREKTUR: Verwende die asynchrone Speichermethode
                await _db.SaveChangesAsync();

                // Leitet zur neuen Kanban-Ansicht dieses Boards weiter
                return RedirectToAction(nameof(Kanban), new { id = board.Id });
            }

            // Falls ungültig, zeige das Formular erneut mit den bisherigen Eingaben
            return View(viewModel);
        }
        

        // DIES IST DIE ACTION FÜR DEINE KANBAN-SEITE
        public IActionResult Kanban(int id)
        {
            var board = _db.Boards
                .Include(b => b.Columns)       // Lade die Spalten des Boards
                    .ThenInclude(c => c.Tasks) // Lade die Aufgaben für jede Spalte
                .FirstOrDefault(b => b.Id == id);

            if (board == null)
            {
                return NotFound(); // Falls kein Board mit dieser ID existiert
            }

            return View(board); // Übergibt das Board-Objekt an die Kanban.cshtml View
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}