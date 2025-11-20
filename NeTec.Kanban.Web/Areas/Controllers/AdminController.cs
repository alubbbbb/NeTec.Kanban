using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeTec.Kanban.Domain.Entities;

namespace NeTec.Kanban.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// Haupt-Controller für den administrativen Bereich (Area).
    /// Stellt Funktionen zur Benutzerverwaltung bereit.
    /// Zugriff ist strikt auf Benutzer mit der Rolle 'Admin' beschränkt.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Lädt eine Liste aller registrierten Benutzer zur Verwaltung.
        /// </summary>
        /// <returns>View mit Benutzerliste.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Abruf aller Benutzer ohne Tracking (Read-Only Optimierung)
            var users = await _userManager.Users.AsNoTracking().ToListAsync();
            return View(users);
        }

        /// <summary>
        /// Löscht ein Benutzerkonto anhand der ID.
        /// </summary>
        /// <param name="id">Die GUID des zu löschenden Benutzers.</param>
        /// <returns>Redirect zur Listenansicht.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                // Validierung: Verhindern des Selbstlöschens durch den angemeldeten Administrator
                if (User.Identity?.Name == user.UserName)
                {
                    TempData["ErrorMessage"] = "Das eigene Administratorkonto kann nicht gelöscht werden.";
                    return RedirectToAction(nameof(Index));
                }

                // Durchführen der Löschung
                await _userManager.DeleteAsync(user);
                TempData["SuccessMessage"] = "Das Benutzerkonto wurde erfolgreich entfernt.";
            }
            else
            {
                TempData["ErrorMessage"] = "Der Benutzer wurde nicht gefunden.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}