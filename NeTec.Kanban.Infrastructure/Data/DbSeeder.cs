using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NeTec.Kanban.Domain.Entities;

namespace NeTec.Kanban.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;

            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = provider.GetRequiredService<ApplicationDbContext>();

            // 1) Rollen anlegen
            var roles = new[] { "Administrator", "User" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2) Admin Benutzer anlegen (falls noch nicht vorhanden)
            var adminEmail = "admin@example.com";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Administrator"
                };

                // Passwort muss den Identity-Policies entsprechen. Beispiel: "Admin123!"
                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Administrator");
                }
            }

            // 3) Demo‑Boards anlegen, falls keine vorhanden sind
            if (!await context.Boards.AnyAsync())
            {
                var demoBoard = new Board { UserId = admin.Id, Name = "Demo Board", Description = "Beispiel-Board für Demozwecke" };
                context.Boards.Add(demoBoard);
                await context.SaveChangesAsync();

                // Beispielspalten mit BoardId setzen
                context.Columns.Add(new Column { BoardId = demoBoard.Id, Name = "Backlog", OrderIndex = 0 });
                context.Columns.Add(new Column { BoardId = demoBoard.Id, Name = "In Progress", OrderIndex = 1 });
                context.Columns.Add(new Column { BoardId = demoBoard.Id, Name = "Done", OrderIndex = 2 });
                await context.SaveChangesAsync();
            }
        }
    }
}