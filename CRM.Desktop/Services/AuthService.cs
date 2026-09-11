using CRM.Desktop.Data;
using CRM.Desktop.Models;
using Microsoft.EntityFrameworkCore;

namespace CRM.Desktop.Services;

public sealed class AuthService
{
    public AppUser? Authenticate(string email, string password)
    {
        using var db = new CrmDbContext();
        var user = db.Users.AsNoTracking().SingleOrDefault(x => x.Email == email.Trim().ToLower() && x.IsActive);
        return user?.PasswordHash == DbSeeder.Hash(password) ? user : null;
    }
}
