using CRM.Desktop.Data;
using CRM.Desktop.Models;
using Microsoft.EntityFrameworkCore;

namespace CRM.Desktop.Services;

public sealed class ProjectService
{
    public List<CrmProject> GetProjects(AppUser user) { using var db = new CrmDbContext(); IQueryable<CrmProject> query = db.Projects.AsNoTracking().Include(x => x.Customer); if (user.Role == UserRole.Sales) query = query.Where(x => x.AssignedTo == user.DisplayName); return query.OrderByDescending(x => x.StartDate).ToList(); }
    public List<Customer> GetCustomers() { using var db = new CrmDbContext(); return db.Customers.AsNoTracking().OrderBy(x => x.CompanyName).ToList(); }
    public List<string> GetPeople() { using var db = new CrmDbContext(); return db.Users.AsNoTracking().Where(x => x.IsActive).Select(x => x.DisplayName).OrderBy(x => x).ToList(); }
    public void Save(CrmProject project, AppUser user)
    {
        using var db = new CrmDbContext(); if (user.Role == UserRole.Sales) project.AssignedTo = user.DisplayName;
        if (project.Id == 0) { project.ProjectCode = $"P-{(db.Projects.Max(x => (int?)x.Id) ?? 0) + 1:000}"; if (string.IsNullOrWhiteSpace(project.AssignedTo)) project.AssignedTo = user.DisplayName; db.Projects.Add(project); }
        else { var existing = db.Projects.Single(x => x.Id == project.Id); if (user.Role == UserRole.Sales && existing.AssignedTo != user.DisplayName) throw new UnauthorizedAccessException(); existing.CustomerId = project.CustomerId; existing.Name = project.Name; existing.AssignedTo = project.AssignedTo; existing.Status = project.Status; existing.Progress = project.Progress; existing.StartDate = project.StartDate; existing.EndDate = project.EndDate; existing.Notes = project.Notes; }
        db.SaveChanges();
    }
}
