using System.Security.Cryptography;
using System.Text;
using CRM.Desktop.Models;

namespace CRM.Desktop.Data;

public static class DbSeeder
{
    public static void Seed(CrmDbContext db)
    {
        var password = Hash("123456");
        if (!db.Users.Any()) db.Users.AddRange(
            new AppUser { DisplayName = "Ahmed Admin", Email = "admin@crm.local", PasswordHash = password, Role = UserRole.Admin },
            new AppUser { DisplayName = "Mona Manager", Email = "manager@crm.local", PasswordHash = password, Role = UserRole.Manager },
            new AppUser { DisplayName = "Omar Sales", Email = "sales@crm.local", PasswordHash = password, Role = UserRole.Sales });
        if (!db.LeadSources.Any()) db.LeadSources.AddRange(new[] { "Website", "LinkedIn", "Facebook", "Referral", "Instagram", "Walk-in" }.Select(x => new LeadSource { Name = x }));
        if (!db.Leads.Any()) db.Leads.AddRange(
            new Lead { LeadCode = "L-001", Company = "ABC Engineering", ContactName = "Ahmed Mohamed", Phone = "01012345678", WhatsApp = "01012345678", Email = "ahmed@abc.com", Industry = "Construction", Source = "LinkedIn", Status = LeadStatus.Qualified, Score = 80, EstimatedValue = 220000, CreatedAt = DateTime.Today.AddDays(-3), NextFollowUpAt = DateTime.Today.AddDays(2), AssignedTo = "Omar Sales" },
            new Lead { LeadCode = "L-002", Company = "Future Tech", ContactName = "Sara Ibrahim", Phone = "01098765432", Email = "sara@futuretech.com", Industry = "Technology", Source = "Website", Status = LeadStatus.New, Score = 50, EstimatedValue = 150000, CreatedAt = DateTime.Today.AddDays(-12), AssignedTo = "Omar Sales" },
            new Lead { LeadCode = "L-003", Company = "Global Solutions", ContactName = "Mohamed Hassan", Phone = "01122334455", Email = "mohamed@global.com", Industry = "Consulting", Source = "Facebook", Status = LeadStatus.Contacted, Score = 65, EstimatedValue = 80000, CreatedAt = DateTime.Today.AddDays(-25), AssignedTo = "Mona Manager" });
        if (!db.Opportunities.Any()) db.Opportunities.AddRange(
            new Opportunity { Name = "Factory Automation", Stage = DealStage.Negotiation, Value = 220000, OwnerName = "Omar Sales", CreatedAt = DateTime.Today.AddDays(-7) },
            new Opportunity { Name = "ERP Integration", Stage = DealStage.Proposal, Value = 180000, OwnerName = "Mona Manager", CreatedAt = DateTime.Today.AddDays(-14) },
            new Opportunity { Name = "Cloud Migration", Stage = DealStage.Won, Value = 350000, OwnerName = "Omar Sales", CreatedAt = DateTime.Today.AddDays(-30) });
        if (!db.Customers.Any())
        {
            var customer = new Customer { CustomerCode = "C-001", CompanyName = "ABC Engineering", Industry = "Construction & Engineering", CompanySize = "50 - 200 employees", Website = "www.abc-eng.com", Phone = "01012345678", Email = "info@abc-eng.com", Address = "Cairo, Egypt", Status = CustomerStatus.Active, CreatedAt = DateTime.Today.AddDays(-45) };
            customer.Contacts.AddRange(new[] { new ContactPerson { FullName = "Ahmed Mohamed", JobTitle = "CEO", Phone = "01012345678", Email = "ahmed@abc-eng.com", IsPrimary = true }, new ContactPerson { FullName = "Omar Hassan", JobTitle = "Technical Manager", Phone = "01234567890", Email = "omar@abc-eng.com" } });
            customer.Activities.AddRange(new[] { new CustomerActivity { Type = "Meeting", Title = "Requirements workshop", Details = "Discussed the automation scope and next steps.", OccurredAt = DateTime.Today.AddDays(-2).AddHours(11) }, new CustomerActivity { Type = "Call", Title = "Follow-up call", Details = "Customer requested the proposal before Thursday.", OccurredAt = DateTime.Today.AddDays(-6).AddHours(14) }, new CustomerActivity { Type = "Note", Title = "Budget confirmed", Details = "Indicative budget approved by management.", OccurredAt = DateTime.Today.AddDays(-9) } });
            db.Customers.Add(customer);
        }
        db.SaveChanges();
    }

    public static string Hash(string password) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
}
