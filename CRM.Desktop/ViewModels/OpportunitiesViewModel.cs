using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRM.Desktop.Models;
using CRM.Desktop.Services;

namespace CRM.Desktop.ViewModels;

public partial class OpportunitiesViewModel : ObservableObject
{
    private readonly OpportunityService _service;
    private AppUser? _currentUser;
    public ObservableCollection<OpportunityColumn> Columns { get; } = new();
    public ObservableCollection<Customer> Customers { get; } = new();
    public ObservableCollection<string> SalesPeople { get; } = new();
    public Array Stages => Enum.GetValues<DealStage>();
    [ObservableProperty] private bool isEditorOpen;
    [ObservableProperty] private bool canAssign;
    [ObservableProperty] private OpportunityFormViewModel editor = new();
    [ObservableProperty] private string editorError = "";

    public OpportunitiesViewModel(OpportunityService service)
    {
        _service = service;
        Columns.Add(new OpportunityColumn(DealStage.NewLead, "New lead")); Columns.Add(new OpportunityColumn(DealStage.Qualified, "Qualified")); Columns.Add(new OpportunityColumn(DealStage.Proposal, "Proposal"));
        Columns.Add(new OpportunityColumn(DealStage.Negotiation, "Negotiation")); Columns.Add(new OpportunityColumn(DealStage.Won, "Won")); Columns.Add(new OpportunityColumn(DealStage.Lost, "Lost"));
    }
    public void Load(AppUser user)
    {
        _currentUser = user; CanAssign = user.Role != UserRole.Sales;
        Customers.Clear(); foreach (var customer in _service.GetCustomers()) Customers.Add(customer);
        SalesPeople.Clear(); foreach (var name in _service.GetSalesPeople()) SalesPeople.Add(name);
        Refresh();
    }
    [RelayCommand] private void Refresh()
    {
        if (_currentUser is null) return;
        foreach (var column in Columns) column.Opportunities.Clear();
        foreach (var opportunity in _service.GetOpportunities(_currentUser)) Columns.Single(x => x.Stage == opportunity.Stage).Opportunities.Add(opportunity);
        OnPropertyChanged(nameof(Columns));
    }
    [RelayCommand] private void NewOpportunity()
    {
        if (_currentUser is null) return;
        Editor = new OpportunityFormViewModel { OwnerName = _currentUser.DisplayName }; EditorError = ""; IsEditorOpen = true;
    }
    [RelayCommand] private void EditOpportunity(Opportunity? opportunity) { if (opportunity is null) return; Editor = new OpportunityFormViewModel(); Editor.Load(opportunity); EditorError = ""; IsEditorOpen = true; }
    [RelayCommand] private void CancelEdit() => IsEditorOpen = false;
    [RelayCommand] private void SaveOpportunity()
    {
        if (_currentUser is null) return;
        if (string.IsNullOrWhiteSpace(Editor.Name) || Editor.Value <= 0) { EditorError = "Opportunity name and a value greater than zero are required."; return; }
        _service.Save(Editor.ToEntity(), _currentUser); IsEditorOpen = false; Refresh();
    }
    [RelayCommand] private void MovePrevious(Opportunity? opportunity) => Move(opportunity, -1);
    [RelayCommand] private void MoveNext(Opportunity? opportunity) => Move(opportunity, 1);
    private void Move(Opportunity? opportunity, int offset)
    {
        if (_currentUser is null || opportunity is null) return;
        var target = (int)opportunity.Stage + offset;
        if (target < (int)DealStage.NewLead || target > (int)DealStage.Lost) return;
        _service.ChangeStage(opportunity.Id, (DealStage)target, _currentUser); Refresh();
    }
}
