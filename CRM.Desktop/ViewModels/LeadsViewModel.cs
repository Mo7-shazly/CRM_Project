using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRM.Desktop.Models;
using CRM.Desktop.Services;

namespace CRM.Desktop.ViewModels;

public partial class LeadsViewModel : ObservableObject
{
    private readonly LeadService _service;
    private AppUser? _user;
    public ObservableCollection<Lead> Leads { get; } = [];
    public ObservableCollection<string> Statuses { get; } = ["All statuses", .. Enum.GetNames<LeadStatus>()];
    public ObservableCollection<string> Assignees { get; } = ["All sales"];
    public ObservableCollection<string> Sources { get; } = ["All sources"];
    public ObservableCollection<string> SalesPeople { get; } = [];
    public ObservableCollection<string> SourceNames { get; } = [];
    public Array StatusValues => Enum.GetValues<LeadStatus>();
    [ObservableProperty] private string selectedStatus = "All statuses";
    [ObservableProperty] private string selectedAssignee = "All sales";
    [ObservableProperty] private string selectedSource = "All sources";
    [ObservableProperty] private string searchText = "";
    [ObservableProperty] private Lead? selectedLead;
    [ObservableProperty] private LeadEditViewModel editor = new();
    [ObservableProperty] private bool isEditorOpen;
    [ObservableProperty] private bool isNewLead;
    [ObservableProperty] private string editorError = "";
    public bool CanAssign => _user?.Role is UserRole.Admin or UserRole.Manager;
    public bool CanAddOrEdit => _user is not null;
    public LeadsViewModel(LeadService service) => _service = service;
    public void Load(AppUser user)
    {
        _user = user; Assignees.Clear(); Assignees.Add("All sales"); SalesPeople.Clear(); foreach (var item in _service.GetSalesPeople()) { Assignees.Add(item); SalesPeople.Add(item); }
        Sources.Clear(); Sources.Add("All sources"); SourceNames.Clear(); foreach (var item in _service.GetSources()) { Sources.Add(item); SourceNames.Add(item); }
        OnPropertyChanged(nameof(CanAssign)); OnPropertyChanged(nameof(CanAddOrEdit)); Refresh();
    }
    partial void OnSelectedStatusChanged(string value) => Refresh(); partial void OnSelectedAssigneeChanged(string value) => Refresh(); partial void OnSelectedSourceChanged(string value) => Refresh(); partial void OnSearchTextChanged(string value) => Refresh();
    [RelayCommand] private void Refresh()
    {
        if (_user is null) return; Leads.Clear(); foreach (var lead in _service.GetLeads(_user, SelectedStatus, SelectedAssignee, SelectedSource, SearchText)) Leads.Add(lead);
    }
    [RelayCommand] private void NewLead() { Editor = new LeadEditViewModel { AssignedTo = _user?.Role == UserRole.Sales ? _user.DisplayName : SalesPeople.FirstOrDefault() ?? "", Source = SourceNames.FirstOrDefault() ?? "" }; IsNewLead = true; EditorError = ""; IsEditorOpen = true; }
    [RelayCommand] private void EditLead(Lead? lead) { if (lead is null) return; Editor = new LeadEditViewModel(); Editor.Load(lead); IsNewLead = false; EditorError = ""; IsEditorOpen = true; }
    [RelayCommand] private void CancelEdit() => IsEditorOpen = false;
    [RelayCommand] private void SaveLead()
    {
        if (string.IsNullOrWhiteSpace(Editor.Company) || string.IsNullOrWhiteSpace(Editor.ContactName)) { EditorError = "Company and contact person are required."; return; }
        if (_user?.Role == UserRole.Sales) Editor.AssignedTo = _user.DisplayName;
        try { _service.Save(Editor.ToEntity(), _user!); IsEditorOpen = false; Refresh(); }
        catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException) { EditorError = ex.Message; }
    }
    [RelayCommand] private void DeleteLead(Lead? lead)
    {
        if (lead is null || _user is null) return;
        try { _service.Delete(lead.Id, _user); Refresh(); }
        catch (UnauthorizedAccessException ex) { EditorError = ex.Message; }
    }
}
