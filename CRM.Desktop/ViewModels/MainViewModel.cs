using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRM.Desktop.Models;
using CRM.Desktop.Services;
using System.Collections.ObjectModel;

namespace CRM.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly AuthService _auth;
    [ObservableProperty] private bool isAuthenticated;
    [ObservableProperty] private string email = "admin@crm.local";
    [ObservableProperty] private string password = "123456";
    [ObservableProperty] private string loginError = "";
    [ObservableProperty] private AppUser? currentUser;
    [ObservableProperty] private object? currentScreen;
    [ObservableProperty] private string globalSearchText = "";
    public ObservableCollection<GlobalSearchResult> GlobalResults { get; } = new();
    public bool HasGlobalResults => GlobalResults.Count > 0;
    public DashboardViewModel Dashboard { get; }
    public LeadsViewModel Leads { get; }
    public CustomersViewModel Customers { get; }
    public ActivitiesViewModel Activities { get; }
    public OpportunitiesViewModel Opportunities { get; }
    public QuotationsViewModel Quotations { get; }
    public ProjectsViewModel Projects { get; }
    public ReportsViewModel Reports { get; }
    public SettingsViewModel Settings { get; }
    private readonly GlobalSearchService _globalSearch;
    public MainViewModel(AuthService auth, DashboardService dashboard, LeadService leadService, CustomerService customerService, ActivityService activityService, OpportunityService opportunityService, QuotationService quotationService, ProjectService projectService, ReportService reportService, GlobalSearchService globalSearch)
    { _auth = auth; _globalSearch = globalSearch; Dashboard = new DashboardViewModel(dashboard); Leads = new LeadsViewModel(leadService); Customers = new CustomersViewModel(customerService); Activities = new ActivitiesViewModel(activityService); Opportunities = new OpportunitiesViewModel(opportunityService); Quotations = new QuotationsViewModel(quotationService); Projects = new ProjectsViewModel(projectService); Reports = new ReportsViewModel(reportService); Settings = new SettingsViewModel(new UserService()); GlobalResults.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasGlobalResults)); CurrentScreen = Dashboard; }
    partial void OnGlobalSearchTextChanged(string value) { GlobalResults.Clear(); if (CurrentUser is not null && value.Length >= 2) foreach (var result in _globalSearch.Search(CurrentUser, value)) GlobalResults.Add(result); }
    [RelayCommand] private void Login()
    { CurrentUser = _auth.Authenticate(Email, Password); if (CurrentUser is null) { LoginError = "Invalid email or password."; return; } LoginError = ""; IsAuthenticated = true; Dashboard.Load(CurrentUser); Leads.Load(CurrentUser); Settings.Load(CurrentUser); CurrentScreen = Dashboard; }
    [RelayCommand] private void Logout() { IsAuthenticated = false; Password = ""; }
    [RelayCommand] private void Navigate(string page)
    { if (page == "Dashboard") { Dashboard.Load(CurrentUser); CurrentScreen = Dashboard; } else if (page == "Leads" && CurrentUser is not null) { Leads.Load(CurrentUser); CurrentScreen = Leads; } else if (page == "Customers") { Customers.RefreshCommand.Execute(null); CurrentScreen = Customers; } else if (page == "Activities" && CurrentUser is not null) { Activities.Load(CurrentUser); CurrentScreen = Activities; } else if (page == "Opportunities" && CurrentUser is not null) { Opportunities.Load(CurrentUser); CurrentScreen = Opportunities; } else if (page == "Quotations" && CurrentUser is not null) { Quotations.Load(CurrentUser); CurrentScreen = Quotations; } else if (page == "Projects" && CurrentUser is not null) { Projects.Load(CurrentUser); CurrentScreen = Projects; } else if (page == "Reports" && CurrentUser is not null) { Reports.Load(CurrentUser); CurrentScreen = Reports; } else if (page == "Settings") { CurrentScreen = Settings; } }
}
