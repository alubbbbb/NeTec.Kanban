using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NeTec.Kanban.Domain.Entities;
using NeTec.Kanban.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ====================================================
// 🔹 Datenbankverbindung
// ====================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ====================================================
// 🔹 Identity-Setup (mit Rollen & integrierter UI)
// ====================================================
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultUI(); // <- Aktiviert die integrierte Login/Register-UI aus der Cloud

// Login-Routing global festlegen
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// ====================================================
// 🔹 MVC + Razor Pages
// ====================================================
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ====================================================
// 🔹 Pipeline-Konfiguration
// ====================================================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Optional: Datenbank-Seeding
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            DbSeeder.SeedAsync(services).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Seeding failed");
        }
    }
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Auth Middleware muss in dieser Reihenfolge bleiben!
app.UseAuthentication();
app.UseAuthorization();

// ====================================================
// 🔹 Standardrouten
// ====================================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Board}/{action=Index}/{id?}");

// RazorPages für Identity aktivieren
app.MapRazorPages();

// ====================================================
// 🔹 App starten
// ====================================================
app.Run();
