using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NeTec.Kanban.Domain.Entities;
using NeTec.Kanban.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ====================================================
// 1. DI-CONTAINER KONFIGURATION (SERVICES)
// ====================================================

// Konfiguration des Entity Framework Core Datenbankkontexts (SQL Server).
// Der ConnectionString wird aus der appsettings.json bezogen.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Verbindungszeichenfolge 'DefaultConnection' nicht gefunden.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Einrichtung des Identity-Systems für Authentifizierung und Autorisierung.
// Konfiguration umfasst Benutzerverwaltung, Rollen (RBAC) und EF-Core-Integration.
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Für interne Zwecke deaktiviert
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultUI();

// Anpassung der Cookie-Pfade für Login- und Zugriffsverweigerung.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Registrierung von MVC-Controllern mit Views sowie Razor Pages (für Identity UI).
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Konfiguration für API-Dokumentation (Swagger/OpenAPI).
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ====================================================
// 2. HTTP REQUEST PIPELINE (MIDDLEWARE)
// ====================================================

// Fehlerbehandlung in Abhängigkeit der Laufzeitumgebung.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // HTTP Strict Transport Security (HSTS) für erhöhte Sicherheit in Produktion.
    app.UseHsts();
}

// Aktivierung der Swagger-UI zur API-Dokumentation und -Testung.
// Wird hier auch im Release-Modus aktiviert, um die Schnittstelle präsentieren zu können.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NeTec Kanban API v1");
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Aktivierung der Authentifizierung und Autorisierung.
// Die Reihenfolge ist technisch zwingend: Erst authentifizieren, dann autorisieren.
app.UseAuthentication();
app.UseAuthorization();

// ====================================================
// 3. ROUTING KONFIGURATION (ENDPOINTS)
// ====================================================

// Route für Areas (z.B. Admin-Bereich).
// Muss vor der Standard-Route definiert werden, um spezifische Area-Controller korrekt aufzulösen.
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Standard-Route für die Anwendung (Kanban Board als Einstiegspunkt).
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Board}/{action=Index}/{id?}");

// Routing für Identity Razor Pages (Login/Register).
app.MapRazorPages();

// ====================================================
// 4. DATENBANK-INITIALISIERUNG (SEEDING)
// ====================================================

// Initialisierung von Stammdaten (Rollen, Admin-User) beim Anwendungsstart.
// Verwendet einen temporären Scope, um Services aufzulösen.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Fehler beim Seeding der Datenbank.");
    }
}

// Starten der Webanwendung
app.Run();