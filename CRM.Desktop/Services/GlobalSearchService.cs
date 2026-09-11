using CRM.Desktop.Data;
using CRM.Desktop.Models;
using Microsoft.EntityFrameworkCore;
namespace CRM.Desktop.Services;
public sealed class GlobalSearchService
{
    public List<GlobalSearchResult> Search(AppUser user, string term)
    {
        if (string.IsNullOrWhiteSpace(term)) return new(); term = term.Trim(); using var db = new CrmDbContext();
        var results = new List<GlobalSearchResult>();
        results.AddRange(db.Customers.AsNoTracking().Where(x => x.CompanyName.Contains(term) || x.Phone.Contains(term) || x.Email.Contains(term)).Take(6).Select(x => new GlobalSearchResult("Customer", x.CompanyName, x.CustomerCode)).ToList());
        IQueryable<Lead> leads = db.Leads.AsNoTracking(); if (user.Role == UserRole.Sales) leads = leads.Where(x => x.AssignedTo == user.DisplayName); results.AddRange(leads.Where(x => x.Company.Contains(term) || x.ContactName.Contains(term) || x.LeadCode.Contains(term)).Take(6).Select(x => new GlobalSearchResult("Lead", x.Company, x.LeadCode)).ToList());
        IQueryable<Opportunity> opportunities = db.Opportunities.AsNoTracking(); if (user.Role == UserRole.Sales) opportunities = opportunities.Where(x => x.OwnerName == user.DisplayName); results.AddRange(opportunities.Where(x => x.Name.Contains(term) || x.OwnerName.Contains(term)).Take(6).Select(x => new GlobalSearchResult("Opportunity", x.Name, x.OwnerName)).ToList());
        return results.Take(12).ToList();
    }
}
public sealed record GlobalSearchResult(string Type, string Title, string Subtitle);
