using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NeTec.Kanban.Domain.Entities;

namespace NeTec.Kanban.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Rollen sicherstellen
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Benutzer erstellen (Ein ganzes Team)
            var users = new List<(string Email, string Name, string Role, string Password)>
            {
                ("admin@netec.de", "System Administrator", "Admin", "Admin123!"),
                ("max.dev@netec.de", "Max Mustermann (Dev)", "User", "User123!"),
                ("lisa.support@netec.de", "Lisa Support", "User", "User123!"),
                ("tom.marketing@netec.de", "Tom Vertrieb", "User", "User123!")
            };

            var userEntities = new Dictionary<string, ApplicationUser>();

            foreach (var u in users)
            {
                var user = await userManager.FindByEmailAsync(u.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = u.Email,
                        Email = u.Email,
                        EmailConfirmed = true,
                        FullName = u.Name,
                        CreatedAt = DateTime.UtcNow
                    };
                    await userManager.CreateAsync(user, u.Password);
                    await userManager.AddToRoleAsync(user, u.Role);
                }
                userEntities[u.Email] = user;
            }

            // 3. Boards & Aufgaben anlegen (Nur wenn DB leer ist)
            if (!await context.Boards.AnyAsync())
            {
                // --- BOARD 1: IHK Projekt (Softwareentwicklung) ---
                var boardDev = new Board
                {
                    Titel = "IHK Abschlussprojekt 2025",
                    Description = "Entwicklung des Kanban-Boards für die NeTec GmbH.",
                    UserId = userEntities["admin@netec.de"].Id, // Admin ist Owner
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                };
                context.Boards.Add(boardDev);
                await context.SaveChangesAsync();

                var colDev1 = new Column { BoardId = boardDev.Id, Titel = "Backlog", OrderIndex = 1 };
                var colDev2 = new Column { BoardId = boardDev.Id, Titel = "In Arbeit", OrderIndex = 2 };
                var colDev3 = new Column { BoardId = boardDev.Id, Titel = "Code Review", OrderIndex = 3 };
                var colDev4 = new Column { BoardId = boardDev.Id, Titel = "Fertig", OrderIndex = 4 };
                context.Columns.AddRange(colDev1, colDev2, colDev3, colDev4);
                await context.SaveChangesAsync();

                // Aufgaben für Board 1
                var tasksDev = new List<TaskItem>
                {
                    // Überfällig & High Prio
                    new TaskItem {
                        Title = "Datenbank-Schema finalisieren",
                        Description = "ER-Diagramm muss dringend mit dem Ausbilder abgestimmt werden.",
                        Priority = "High",
                        ColumnId = colDev3.Id,
                        UserId = userEntities["max.dev@netec.de"].Id,
                        DueDate = DateTime.UtcNow.AddDays(-2), // ÜBERFÄLLIG!
                        EstimatedHours = 4,
                        RemainingHours = 1 // Fast fertig
                    },
                    // Normal
                    new TaskItem {
                        Title = "Frontend-Design umsetzen",
                        Description = "Bootstrap 5 Integration und CSS Anpassungen.",
                        Priority = "Medium",
                        ColumnId = colDev2.Id,
                        UserId = userEntities["max.dev@netec.de"].Id,
                        DueDate = DateTime.UtcNow.AddDays(5), // Zukunft
                        EstimatedHours = 8,
                        RemainingHours = 6
                    },
                    // Erledigt
                    new TaskItem {
                        Title = "Projekt-Initialisierung",
                        Description = "Git Repo anlegen und Solution erstellen.",
                        Priority = "Low",
                        ColumnId = colDev4.Id,
                        UserId = userEntities["admin@netec.de"].Id,
                        DueDate = DateTime.UtcNow.AddDays(-8),
                        EstimatedHours = 2,
                        RemainingHours = 0
                    }
                };

                // --- BOARD 2: IT-Support (Operations) ---
                var boardOps = new Board
                {
                    Titel = "IT-Support & Infrastruktur",
                    Description = "Laufende Tickets und Server-Wartung.",
                    UserId = userEntities["lisa.support@netec.de"].Id, // Lisa ist Owner
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                };
                context.Boards.Add(boardOps);
                await context.SaveChangesAsync();

                var colOps1 = new Column { BoardId = boardOps.Id, Titel = "Offen", OrderIndex = 1 };
                var colOps2 = new Column { BoardId = boardOps.Id, Titel = "Warten auf Kunde", OrderIndex = 2 };
                var colOps3 = new Column { BoardId = boardOps.Id, Titel = "Gelöst", OrderIndex = 3 };
                context.Columns.AddRange(colOps1, colOps2, colOps3);
                await context.SaveChangesAsync();

                var tasksOps = new List<TaskItem>
                {
                    new TaskItem {
                        Title = "Server Update DC-01",
                        Description = "Sicherheitsupdates einspielen. Wartungsfenster beachten!",
                        Priority = "High",
                        ColumnId = colOps1.Id,
                        UserId = userEntities["lisa.support@netec.de"].Id,
                        DueDate = DateTime.UtcNow.AddDays(1), // Morgen
                        EstimatedHours = 3,
                        RemainingHours = 3
                    },
                    new TaskItem {
                        Title = "Drucker im Vertrieb defekt",
                        Description = "Papierstau oder Toner leer? Ticket #4922",
                        Priority = "Medium",
                        ColumnId = colOps2.Id,
                        UserId = null, // Noch niemandem zugewiesen (Pool)
                        DueDate = DateTime.UtcNow.AddDays(-1), // Gestern fällig
                        EstimatedHours = 1,
                        RemainingHours = 1
                    }
                };

                // --- BOARD 3: Marketing (Zeigt, dass es nicht nur für IT ist) ---
                var boardMkt = new Board
                {
                    Titel = "Marketing Kampagne Q4",
                    Description = "Planung der Weihnachtsaktion.",
                    UserId = userEntities["tom.marketing@netec.de"].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                };
                context.Boards.Add(boardMkt);
                await context.SaveChangesAsync();

                var colMkt1 = new Column { BoardId = boardMkt.Id, Titel = "Ideen", OrderIndex = 1 };
                var colMkt2 = new Column { BoardId = boardMkt.Id, Titel = "Entwurf", OrderIndex = 2 };
                var colMkt3 = new Column { BoardId = boardMkt.Id, Titel = "Freigabe", OrderIndex = 3 };
                context.Columns.AddRange(colMkt1, colMkt2, colMkt3);
                await context.SaveChangesAsync();

                var tasksMkt = new List<TaskItem>
                {
                    new TaskItem {
                        Title = "Newsletter Text entwerfen",
                        Priority = "Medium",
                        ColumnId = colMkt2.Id,
                        UserId = userEntities["tom.marketing@netec.de"].Id,
                        DueDate = DateTime.UtcNow.AddDays(3),
                        EstimatedHours = 5,
                        RemainingHours = 2.5m
                    },
                    // Ein Ticket für den Admin auf einem fremden Board (Zusammenarbeit!)
                    new TaskItem {
                        Title = "Grafiken freigeben",
                        Description = "Bitte kurz drüberschauen, ob das CI-konform ist.",
                        Priority = "High",
                        ColumnId = colMkt3.Id,
                        UserId = userEntities["admin@netec.de"].Id, // Zuweisung an Admin!
                        DueDate = DateTime.UtcNow.AddDays(2),
                        EstimatedHours = 0.5m,
                        RemainingHours = 0.5m
                    }
                };

                // Alles speichern
                context.TaskItems.AddRange(tasksDev);
                context.TaskItems.AddRange(tasksOps);
                context.TaskItems.AddRange(tasksMkt);
                await context.SaveChangesAsync();
            }
        }
    }
}