using Microsoft.AspNetCore.Identity;

namespace NeTec.Kanban.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    // Hier können wir später benutzerspezifische Eigenschaften hinzufügen,
    // die IdentityUser nicht von Haus aus hat.
    // z.B. public string? FirstName { get; set; }
    // z.B. public string? LastName { get; set; }
}