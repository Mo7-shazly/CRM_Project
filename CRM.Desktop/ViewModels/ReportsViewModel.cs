using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRM.Desktop.Models;
using CRM.Desktop.Services;
namespace CRM.Desktop.ViewModels;
public partial class ReportsViewModel : ObservableObject
{
    private readonly ReportService _service; private AppUser? _currentUser;
    public ObservableCollection<SalesReportRow> Sales { get; } = new(); public ObservableCollection<PipelineReportRow> Pipeline { get; } = new();
    [ObservableProperty] private DateTime fromDate = DateTime.Today.AddDays(-30); [ObservableProperty] private DateTime toDate = DateTime.Today;
    [ObservableProperty] private int leads; [ObservableProperty] private int customers; [ObservableProperty] private decimal opportunityValue; [ObservableProperty] private decimal wonValue; [ObservableProperty] private decimal quotationValue;
    public ReportsViewModel(ReportService service) => _service = service;
    public void Load(AppUser user) { _currentUser = user; Refresh(); }
    [RelayCommand] private void Refresh() { if (_currentUser is null || FromDate > ToDate) return; var s = _service.GetSnapshot(_currentUser, FromDate.Date, ToDate.Date); Leads = s.Leads; Customers = s.Customers; OpportunityValue = s.OpportunityValue; WonValue = s.WonValue; QuotationValue = s.QuotationValue; Sales.Clear(); foreach (var x in s.Sales) Sales.Add(x); Pipeline.Clear(); foreach (var x in s.Pipeline) Pipeline.Add(x); }
}
