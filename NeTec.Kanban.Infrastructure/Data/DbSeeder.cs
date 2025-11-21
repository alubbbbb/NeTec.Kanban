using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NeTec.Kanban.Domain.Entities;

namespace NeTec.Kanban.Infrastructure.Data
{
    /// <summary>
    /// Klasse zur Initialisierung von Stammdaten (Seeding).
    /// Erstellt beim Anwendungsstart notwendige Rollen und Standard-Benutzerkonten,
    /// sofern diese noch nicht existieren.
    /// </summary>
    public static class DbSeeder
    {
        /// <summary>
        /// Führt den Seeding-Prozess für Rollen und den Administrator durch.
        /// </summary>
        /// <param name="serviceProvider">Der Service Provider zur Auflösung von Abhängigkeiten.</param>
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            // Auflösung der benötigten Dienste via Dependency Injection
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Definition und Erstellung der Systemrollen
            // Diese Rollen steuern den Zugriff auf Bereiche wie das Admin-Panel.
            var roles = new[] { "Admin", "User" };

            foreach (var roleName in roles)
            {
                // Prüfung, ob die Rolle bereits in der Datenbank existiert
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Erstellung des Standard-Administrators
            // Dieser Account dient als initialer Zugang für die Systemverwaltung.
            var adminEmail = "admin@netec.de";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true, // Wichtig, damit Login sofort möglich ist
                    FullName = "System Administrator", // Für die Anzeige im UI
                    CreatedAt = DateTime.UtcNow
                };

                // Erstellung des Benutzers mit einem sicheren Initialpasswort
                var result = await userManager.CreateAsync(newAdmin, "Admin123!");

                if (result.Succeeded)
                {
                    // Zuweisung der administrativen Rolle
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }
    }
}