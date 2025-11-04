using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NeTec.Kanban.Domain.Entities;
using NeTec.Kanban.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Den Connection String aus appsettings.json auslesen
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 2. Den DbContext als Dienst registrieren (Unsere Brücke zur Datenbank)
//    Wir sagen ihm, er soll SQL Server verwenden und wo er die Datenbank findet.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. ASP.NET Core Identity als Dienst registrieren
//    Wir konfigurieren es, um unsere ApplicationUser-Klasse zu verwenden
//    und sagen ihm, dass es den ApplicationDbContext als Speicher nutzen soll.
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Vorhandene Dienste für Controller und Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// WICHTIG: Authentifizierung aktivieren
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 4. Die Razor Pages für die Identity-UI (Login, etc.) aktivieren
app.MapRazorPages();

app.Run();