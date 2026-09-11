using CRM.Desktop.Data;
using CRM.Desktop.Models;
using Microsoft.EntityFrameworkCore;

namespace CRM.Desktop.Services;
public sealed class UserService
{
    public List<AppUser> GetUsers(AppUser actor) { RequireAdmin(actor); using var db = new CrmDbContext(); return db.Users.AsNoTracking().OrderBy(x => x.DisplayName).ToList(); }
    public void Save(AppUser user, string password, AppUser actor)
    {
        RequireAdmin(actor); using var db = new CrmDbContext(); if (string.IsNullOrWhiteSpace(user.DisplayName) || string.IsNullOrWhiteSpace(user.Email)) throw new InvalidOperationException("Name and email are required."); var email = user.Email.Trim().ToLowerInvariant();
        if (user.Id == 0) { if (string.IsNullOrWhiteSpace(password)) throw new InvalidOperationException("A password is required for a new user."); if (db.Users.Any(x => x.Email == email)) throw new InvalidOperationException("This email is already in use."); db.Users.Add(new AppUser { DisplayName = user.DisplayName.Trim(), Email = email, PasswordHash = DbSeeder.Hash(password), Role = user.Role, IsActive = user.IsActive }); }
        else { var existing = db.Users.Find(user.Id) ?? throw new InvalidOperationException("User not found."); if (existing.Id == actor.Id && (!user.IsActive || user.Role != UserRole.Admin)) throw new InvalidOperationException("You cannot remove your own admin access."); existing.DisplayName = user.DisplayName.Trim(); existing.Email = email; existing.Role = user.Role; existing.IsActive = user.IsActive; if (!string.IsNullOrWhiteSpace(password)) existing.PasswordHash = DbSeeder.Hash(password); }
        db.SaveChanges();
    }
    private static void RequireAdmin(AppUser actor) { if (actor.Role != UserRole.Admin) throw new UnauthorizedAccessException("Only Admin can manage users."); }
}
