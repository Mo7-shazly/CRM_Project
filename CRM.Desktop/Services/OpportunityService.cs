using CRM.Desktop.Data;
using CRM.Desktop.Models;
using Microsoft.EntityFrameworkCore;

namespace CRM.Desktop.Services;

public sealed class OpportunityService
{
    public List<Opportunity> GetOpportunities(AppUser user)
    {
        using var db = new CrmDbContext();
        IQueryable<Opportunity> query = db.Opportunities.AsNoTracking().Include(x => x.Customer);
        if (user.Role == UserRole.Sales) query = query.Where(x => x.OwnerName == user.DisplayName);
        // SQLite cannot translate decimal ORDER BY; perform that presentation ordering in memory.
        return query.ToList().OrderByDescending(x => x.Value).ToList();
    }
    public List<Customer> GetCustomers() { using var db = new CrmDbContext(); return db.Customers.AsNoTracking().OrderBy(x => x.CompanyName).ToList(); }
    public List<string> GetSalesPeople() { using var db = new CrmDbContext(); return db.Users.AsNoTracking().Where(x => x.Role == UserRole.Sales && x.IsActive).Select(x => x.DisplayName).OrderBy(x => x).ToList(); }
    public void Save(Opportunity opportunity, AppUser user)
    {
        using var db = new CrmDbContext();
        if (user.Role == UserRole.Sales) opportunity.OwnerName = user.DisplayName;
        if (opportunity.Id == 0)
        {
            opportunity.CreatedAt = DateTime.Now;
            if (string.IsNullOrWhiteSpace(opportunity.OwnerName)) opportunity.OwnerName = user.DisplayName;
            db.Opportunities.Add(opportunity);
        }
        else
        {
            var existing = db.Opportunities.SingleOrDefault(x => x.Id == opportunity.Id) ?? throw new InvalidOperationException("Opportunity was not found.");
            if (user.Role == UserRole.Sales && existing.OwnerName != user.DisplayName) throw new UnauthorizedAccessException();
            existing.CustomerId = opportunity.CustomerId; existing.Name = opportunity.Name; existing.OwnerName = opportunity.OwnerName; existing.Stage = opportunity.Stage; existing.Value = opportunity.Value; existing.ExpectedCloseAt = opportunity.ExpectedCloseAt;
        }
        db.SaveChanges();
    }
    public void ChangeStage(int id, DealStage stage, AppUser user)
    {
        using var db = new CrmDbContext();
        var opportunity = db.Opportunities.SingleOrDefault(x => x.Id == id);
        if (opportunity is null) return;
        if (user.Role == UserRole.Sales && opportunity.OwnerName != user.DisplayName) throw new UnauthorizedAccessException();
        opportunity.Stage = stage; db.SaveChanges();
    }
}
