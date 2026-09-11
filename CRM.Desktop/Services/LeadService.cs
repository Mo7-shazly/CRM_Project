using CRM.Desktop.Data;
using CRM.Desktop.Models;
using Microsoft.EntityFrameworkCore;

namespace CRM.Desktop.Services;

public sealed class LeadService
{
    public List<Lead> GetLeads(AppUser user, string? status, string? assignee, string? source, string? query)
    {
        using var db = new CrmDbContext();
        IQueryable<Lead> leads = db.Leads.AsNoTracking().Include(x => x.Customer);
        if (user.Role == UserRole.Sales) leads = leads.Where(x => x.AssignedTo == user.DisplayName);
        if (!string.IsNullOrWhiteSpace(status) && status != "All statuses" && Enum.TryParse<LeadStatus>(status, out var parsed)) leads = leads.Where(x => x.Status == parsed);
        if (!string.IsNullOrWhiteSpace(assignee) && assignee != "All sales") leads = leads.Where(x => x.AssignedTo == assignee);
        if (!string.IsNullOrWhiteSpace(source) && source != "All sources") leads = leads.Where(x => x.Source == source);
        if (!string.IsNullOrWhiteSpace(query)) leads = leads.Where(x => x.Company.Contains(query) || (x.Customer != null && x.Customer.CompanyName.Contains(query)) || x.ContactName.Contains(query) || x.Phone.Contains(query) || x.Email.Contains(query));
        return leads.OrderByDescending(x => x.CreatedAt).ToList();
    }
    public List<string> GetSalesPeople() { using var db = new CrmDbContext(); return db.Users.AsNoTracking().Where(x => x.Role == UserRole.Sales && x.IsActive).Select(x => x.DisplayName).OrderBy(x => x).ToList(); }
    public List<string> GetSources() { using var db = new CrmDbContext(); return db.LeadSources.AsNoTracking().Where(x => x.IsActive).Select(x => x.Name).OrderBy(x => x).ToList(); }
    public List<Customer> GetCustomers() { using var db = new CrmDbContext(); return db.Customers.AsNoTracking().OrderBy(x => x.CompanyName).ToList(); }
    public void Save(Lead lead, AppUser user)
    {
        using var db = new CrmDbContext();
        if (user.Role == UserRole.Sales) lead.AssignedTo = user.DisplayName;
        if (string.IsNullOrWhiteSpace(lead.Company)) throw new InvalidOperationException("A company name is required.");
        if (lead.CustomerId.HasValue && db.Customers.Find(lead.CustomerId.Value) is null)
            throw new InvalidOperationException("The selected customer no longer exists.");
        if (lead.Id == 0)
        {
            lead.LeadCode = $"L-{(db.Leads.Max(x => (int?)x.Id) ?? 0) + 1:000}";
            lead.CreatedAt = DateTime.Now;
            db.Leads.Add(lead);
        }
        else
        {
            var existing = db.Leads.Find(lead.Id) ?? throw new InvalidOperationException("Lead not found.");
            if (user.Role == UserRole.Sales && existing.AssignedTo != user.DisplayName) throw new UnauthorizedAccessException("You can only edit leads assigned to you.");
            existing.CustomerId = lead.CustomerId; existing.Company = lead.Company; existing.ContactName = lead.ContactName; existing.Phone = lead.Phone;
            existing.WhatsApp = lead.WhatsApp; existing.Email = lead.Email; existing.Industry = lead.Industry; existing.Source = lead.Source;
            existing.AssignedTo = lead.AssignedTo; existing.Status = lead.Status; existing.Score = lead.Score; existing.EstimatedValue = lead.EstimatedValue;
            existing.Notes = lead.Notes; existing.NextFollowUpAt = lead.NextFollowUpAt;
        }
        db.SaveChanges();
    }
    public void Delete(int id, AppUser user)
    {
        using var db = new CrmDbContext(); var lead = db.Leads.Find(id);
        if (lead is null) return;
        if (user.Role == UserRole.Sales && lead.AssignedTo != user.DisplayName) throw new UnauthorizedAccessException("You can only delete leads assigned to you.");
        db.Leads.Remove(lead); db.SaveChanges();
    }
}
