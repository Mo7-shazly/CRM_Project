using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CRM.Desktop.Models;
using CRM.Desktop.Services;

namespace CRM.Desktop.ViewModels;

public sealed partial class DashboardViewModel : ObservableObject
{
    private readonly DashboardService _service;
    private AppUser? _user;
    public ObservableCollection<string> Periods { get; } = new(new[] { "Today", "This Week", "This Month", "This Year", "All time" });
    public ObservableCollection<KpiItem> Kpis { get; } = new();
    public ObservableCollection<PipelineStageItem> Pipeline { get; } = new();
    public ObservableCollection<SalesItem> TopSales { get; } = new();
    public ObservableCollection<SalesMonthItem> SalesMonths { get; } = new();
    public ObservableCollection<FollowUpItem> FollowUps { get; } = new();
    public ObservableCollection<RecentActivityItem> RecentActivities { get; } = new();
    [ObservableProperty] private string selectedPeriod = "This Month";
    [ObservableProperty] private string dashboardScope = "Company overview";
    public DashboardViewModel(DashboardService service) => _service = service;
    partial void OnSelectedPeriodChanged(string value) => Load();
    public void Load(AppUser? user = null)
    {
        if (user is not null) _user = user;
        DashboardScope = _user?.Role == UserRole.Sales ? "My sales overview" : "Company overview";
        var data = _service.GetSnapshot(SelectedPeriod, _user); Kpis.Clear(); Pipeline.Clear(); TopSales.Clear(); SalesMonths.Clear(); FollowUps.Clear(); RecentActivities.Clear();
        Kpis.Add(new KpiItem("◉", "New leads", data.Leads.ToString(), "Created in selected period", "#2E90FA"));
        Kpis.Add(new KpiItem("♟", "New customers", data.Customers.ToString(), "Added in selected period", "#7F56D9"));
        Kpis.Add(new KpiItem("▣", "Open pipeline", data.PipelineValue.ToString("C0"), $"{data.OpenOpportunities} active opportunities", "#F79009"));
        Kpis.Add(new KpiItem("✓", "Won revenue", data.WonRevenue.ToString("C0"), $"Win rate {data.WinRate}%", "#039855"));
        foreach (var item in data.Pipeline) Pipeline.Add(item); foreach (var item in data.TopSales) TopSales.Add(item); foreach (var item in data.SalesMonths) SalesMonths.Add(item); foreach (var item in data.FollowUps) FollowUps.Add(item); foreach (var item in data.RecentActivities) RecentActivities.Add(item);
    }
}
