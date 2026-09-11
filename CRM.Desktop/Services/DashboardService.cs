using CRM.Desktop.Data;
using CRM.Desktop.Models;
using CRM.Desktop.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CRM.Desktop.Services;

public sealed class DashboardService
{
    public DashboardSnapshot GetSnapshot(string period, AppUser? user = null)
    {
        using var db = new CrmDbContext();
        var from = period switch { "Today" => DateTime.Today, "This Week" => DateTime.Today.AddDays(-6), "This Month" => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), "This Year" => new DateTime(DateTime.Today.Year, 1, 1), _ => DateTime.MinValue };
        IQueryable<Lead> leadQuery = db.Leads.AsNoTracking();
        IQueryable<Opportunity> opportunityQuery = db.Opportunities.AsNoTracking().Include(x => x.Customer);
        IQueryable<CustomerActivity> activityQuery = db.CustomerActivities.AsNoTracking().Include(x => x.Customer);
        if (user?.Role == UserRole.Sales) { leadQuery = leadQuery.Where(x => x.AssignedTo == user.DisplayName); opportunityQuery = opportunityQuery.Where(x => x.OwnerName == user.DisplayName); activityQuery = activityQuery.Where(x => x.AssignedTo == user.DisplayName); }

        var leads = leadQuery.Where(x => x.CreatedAt >= from).ToList();
        var opportunities = opportunityQuery.Where(x => x.CreatedAt >= from).ToList();
        var activities = activityQuery.OrderByDescending(x => x.OccurredAt).Take(8).ToList();
        var customers = db.Customers.AsNoTracking().Count(x => x.CreatedAt >= from);
        var open = opportunities.Where(x => x.Stage is not DealStage.Won and not DealStage.Lost).ToList();
        var closed = opportunities.Where(x => x.Stage is DealStage.Won or DealStage.Lost).ToList();
        var won = opportunities.Where(x => x.Stage == DealStage.Won).ToList();
        var winRate = closed.Count == 0 ? 0 : Math.Round(won.Count * 100m / closed.Count, 1);
        var pipeline = Enum.GetValues<DealStage>().Select(stage => new PipelineStageItem(StageLabel(stage), opportunities.Count(x => x.Stage == stage), opportunities.Where(x => x.Stage == stage).Sum(x => x.Value), StageColor(stage))).ToList();
        var performance = opportunities.Where(x => !string.IsNullOrWhiteSpace(x.OwnerName)).GroupBy(x => x.OwnerName).Select(x => new SalesItem(x.Key, x.Count(), x.Where(y => y.Stage == DealStage.Won).Sum(y => y.Value))).OrderByDescending(x => x.Sales).ThenByDescending(x => x.Deals).Take(5).ToList();
        var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-5);
        var months = Enumerable.Range(0, 6).Select(offset => { var start = monthStart.AddMonths(offset); var end = start.AddMonths(1); return new SalesMonthItem(start.ToString("MMM"), opportunities.Where(x => x.Stage == DealStage.Won && x.CreatedAt >= start && x.CreatedAt < end).Sum(x => x.Value)); }).ToList();
        var maximum = months.Max(x => x.Value); months = months.Select(x => x with { Height = maximum == 0 ? 8 : Math.Max(8, (double)(x.Value / maximum * 145)) }).ToList();
        var dueActivities = activityQuery.Where(x => !x.IsCompleted && x.ReminderAt.HasValue && x.ReminderAt <= DateTime.Today).ToList().Select(x => new FollowUpItem("Activity", x.Title, x.Customer?.CompanyName ?? "Customer", x.AssignedTo, x.ReminderAt!.Value, x.ReminderAt.Value < DateTime.Today));
        var dueLeads = leadQuery.Where(x => x.NextFollowUpAt.HasValue && x.NextFollowUpAt <= DateTime.Today && x.Status != LeadStatus.Won && x.Status != LeadStatus.Lost).ToList().Select(x => new FollowUpItem("Lead", x.Company, x.ContactName, x.AssignedTo, x.NextFollowUpAt!.Value, x.NextFollowUpAt.Value < DateTime.Today));
        var followUps = dueActivities.Concat(dueLeads).OrderByDescending(x => x.IsOverdue).ThenBy(x => x.DueAt).Take(6).ToList();
        var recent = activities.Take(5).Select(x => new RecentActivityItem(x.Type, x.Title, x.Customer?.CompanyName ?? "Customer", x.OccurredAt)).ToList();
        return new DashboardSnapshot(leads.Count, customers, open.Count, open.Sum(x => x.Value), won.Sum(x => x.Value), winRate, pipeline, performance, months, followUps, recent);
    }
    private static string StageLabel(DealStage stage) => stage switch { DealStage.NewLead => "New Lead", DealStage.Qualified => "Qualified", DealStage.Proposal => "Proposal", DealStage.Negotiation => "Negotiation", DealStage.Won => "Won", _ => "Lost" };
    private static string StageColor(DealStage stage) => stage switch { DealStage.NewLead => "#2E90FA", DealStage.Qualified => "#12B76A", DealStage.Proposal => "#7F56D9", DealStage.Negotiation => "#F79009", DealStage.Won => "#039855", _ => "#F04438" };
}

public sealed record DashboardSnapshot(int Leads, int Customers, int OpenOpportunities, decimal PipelineValue, decimal WonRevenue, decimal WinRate, List<PipelineStageItem> Pipeline, List<SalesItem> TopSales, List<SalesMonthItem> SalesMonths, List<FollowUpItem> FollowUps, List<RecentActivityItem> RecentActivities);
