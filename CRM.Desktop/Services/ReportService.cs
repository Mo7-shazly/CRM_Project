using CRM.Desktop.Data;
using CRM.Desktop.Models;
using Microsoft.EntityFrameworkCore;
namespace CRM.Desktop.Services;
public sealed class ReportService
{
    public ReportSnapshot GetSnapshot(AppUser user, DateTime from, DateTime to)
    {
        using var db = new CrmDbContext();
        var leads = db.Leads.AsNoTracking().Where(x => x.CreatedAt >= from && x.CreatedAt < to.AddDays(1));
        var opportunities = db.Opportunities.AsNoTracking().Where(x => x.CreatedAt >= from && x.CreatedAt < to.AddDays(1));
        var quotations = db.Quotations.AsNoTracking().Include(x => x.Items).Where(x => x.IssueDate >= from && x.IssueDate < to.AddDays(1));
        if (user.Role == UserRole.Sales) { leads = leads.Where(x => x.AssignedTo == user.DisplayName); opportunities = opportunities.Where(x => x.OwnerName == user.DisplayName); }
        var opportunityList = opportunities.ToList(); var quotationList = quotations.ToList();
        return new ReportSnapshot
        {
            Leads = leads.Count(), Customers = db.Customers.Count(x => x.CreatedAt >= from && x.CreatedAt < to.AddDays(1)),
            OpportunityValue = opportunityList.Sum(x => x.Value), WonValue = opportunityList.Where(x => x.Stage == DealStage.Won).Sum(x => x.Value), QuotationValue = quotationList.Sum(x => x.Total),
            Sales = opportunityList.GroupBy(x => x.OwnerName).Select(x => new SalesReportRow(x.Key, x.Count(), x.Sum(y => y.Value), x.Count(y => y.Stage == DealStage.Won))).OrderByDescending(x => x.Value).ToList(),
            Pipeline = opportunityList.GroupBy(x => x.Stage).Select(x => new PipelineReportRow(x.Key.ToString(), x.Count(), x.Sum(y => y.Value))).OrderBy(x => x.Stage).ToList()
        };
    }
}
public sealed class ReportSnapshot { public int Leads { get; init; } public int Customers { get; init; } public decimal OpportunityValue { get; init; } public decimal WonValue { get; init; } public decimal QuotationValue { get; init; } public List<SalesReportRow> Sales { get; init; } = new(); public List<PipelineReportRow> Pipeline { get; init; } = new(); }
public sealed record SalesReportRow(string Name, int Opportunities, decimal Value, int Won);
public sealed record PipelineReportRow(string Stage, int Count, decimal Value);
