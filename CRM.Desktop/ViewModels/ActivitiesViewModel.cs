using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRM.Desktop.Models;
using CRM.Desktop.Services;

namespace CRM.Desktop.ViewModels;

public partial class ActivitiesViewModel : ObservableObject
{
    private readonly ActivityService _service;
    private AppUser? _currentUser;
    public ObservableCollection<CustomerActivity> Activities { get; } = new();
    public ObservableCollection<Customer> Customers { get; } = new();
    public ObservableCollection<string> SalesPeople { get; } = new();
    public IReadOnlyList<string> Types { get; } = new[] { "Call", "Meeting", "Email", "Follow-up", "Task" };
    public IReadOnlyList<string> TypeFilters { get; } = new[] { "All types", "Call", "Meeting", "Email", "Follow-up", "Task" };
    public IReadOnlyList<string> StatusFilters { get; } = new[] { "Open", "Completed", "All" };
    [ObservableProperty] private string selectedType = "All types";
    [ObservableProperty] private string selectedStatus = "Open";
    [ObservableProperty] private bool isEditorOpen;
    [ObservableProperty] private ActivityFormViewModel editor = new();
    [ObservableProperty] private string editorError = "";
    [ObservableProperty] private bool canAssign;

    public ActivitiesViewModel(ActivityService service) => _service = service;
    public void Load(AppUser user)
    {
        _currentUser = user; CanAssign = user.Role != UserRole.Sales;
        Customers.Clear(); foreach (var customer in _service.GetCustomers()) Customers.Add(customer);
        SalesPeople.Clear(); foreach (var name in _service.GetSalesPeople()) SalesPeople.Add(name);
        Refresh();
    }
    partial void OnSelectedTypeChanged(string value) => Refresh();
    partial void OnSelectedStatusChanged(string value) => Refresh();
    [RelayCommand] private void Refresh()
    {
        if (_currentUser is null) return;
        Activities.Clear(); foreach (var activity in _service.GetActivities(_currentUser, SelectedType, SelectedStatus)) Activities.Add(activity);
    }
    [RelayCommand] private void NewActivity()
    {
        if (_currentUser is null) return;
        Editor = new ActivityFormViewModel { AssignedTo = _currentUser.DisplayName };
        EditorError = ""; IsEditorOpen = true;
    }
    [RelayCommand] private void CancelEdit() => IsEditorOpen = false;
    [RelayCommand] private void SaveActivity()
    {
        if (_currentUser is null) return;
        if (Editor.CustomerId == 0 || string.IsNullOrWhiteSpace(Editor.Title)) { EditorError = "Customer and activity title are required."; return; }
        _service.Save(Editor.ToEntity(), _currentUser);
        IsEditorOpen = false; Refresh();
    }
    [RelayCommand] private void Complete(CustomerActivity? activity)
    {
        if (_currentUser is null || activity is null) return;
        _service.Complete(activity.Id, _currentUser); Refresh();
    }
}
