using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NeTec.Kanban.Domain.Entities;
using NeTec.Kanban.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ====================================================
// 1. SERVICE-REGISTRIERUNG (DEPENDENCY INJECTION)
// ====================================================

// Konfiguration des Datenbankkontexts für Entity Framework Core (SQL Server)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Einrichtung von ASP.NET Core Identity für Authentifizierung und Benutzerverwaltung.
// Die Konfiguration umfasst Standard-Identity-UI, Rollenunterstützung (RBAC) und EF Core Stores.
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Für interne Zwecke deaktiviert
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultUI();

// Anpassung der Cookie-Pfade für Login und Zugriffverweigerung
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Registrierung der MVC-Controller mit Views sowie Razor Pages (für Identity UI)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// API-Explorer und Swagger-Generator registrieren
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ====================================================
// 2. HTTP REQUEST PIPELINE (MIDDLEWARE)
// ====================================================

// Fehlerbehandlung und Sicherheit in Abhängigkeit der Umgebung
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // HSTS (HTTP Strict Transport Security) für Produktionsumgebungen
    app.UseHsts();
}

// Swagger-UI aktivieren (Auch im Release-Modus für die IHK-Präsentation sinnvoll)
// if (app.Environment.IsDevelopment()) // <-- Optional: if wegnehmen, damit es immer geht
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NeTec Kanban API v1");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Aktivierung der Authentifizierungs- und Autorisierungs-Middleware.
// Die Reihenfolge ist hier essenziell (Auth vor Authorization).
app.UseAuthentication();
app.UseAuthorization();

// ====================================================
// 3. ROUTING KONFIGURATION
// ====================================================

// Route für Areas (z.B. Admin-Bereich).
// Muss VOR der Standardroute definiert werden, um korrekt aufgelöst zu werden.
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Standardroute für die Anwendung (Kanban Board als Startseite)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Board}/{action=Index}/{id?}");

// Routing für Identity Razor Pages
app.MapRazorPages();

// ====================================================
// 4. DATENBANK-INITIALISIERUNG (SEEDING)
// ====================================================

// Initialisierung von Stammdaten (Rollen, Admin-User) beim Anwendungsstart
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Führt den Seeder aus, um sicherzustellen, dass Admin und Rollen existieren
        await DbSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Fehler beim Seeding der Datenbank.");
    }
}

// Starten der Anwendung
app.Run();