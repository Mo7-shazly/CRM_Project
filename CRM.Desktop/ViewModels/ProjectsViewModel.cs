using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRM.Desktop.Models;
using CRM.Desktop.Services;
namespace CRM.Desktop.ViewModels;
public partial class ProjectsViewModel : ObservableObject
{
    private readonly ProjectService _service; private AppUser? _currentUser;
    public ObservableCollection<CrmProject> Projects { get; } = new(); public ObservableCollection<Customer> Customers { get; } = new(); public ObservableCollection<string> People { get; } = new(); public Array Statuses => Enum.GetValues<ProjectStatus>();
    [ObservableProperty] private bool isEditorOpen; [ObservableProperty] private bool canAssign; [ObservableProperty] private ProjectFormViewModel editor = new(); [ObservableProperty] private string editorError = "";
    public ProjectsViewModel(ProjectService service) => _service = service;
    public void Load(AppUser user) { _currentUser = user; CanAssign = user.Role != UserRole.Sales; Customers.Clear(); foreach (var x in _service.GetCustomers()) Customers.Add(x); People.Clear(); foreach (var x in _service.GetPeople()) People.Add(x); Refresh(); }
    [RelayCommand] private void Refresh() { if (_currentUser is null) return; Projects.Clear(); foreach (var x in _service.GetProjects(_currentUser)) Projects.Add(x); }
    [RelayCommand] private void NewProject() { if (_currentUser is null) return; Editor = new ProjectFormViewModel { AssignedTo = _currentUser.DisplayName }; EditorError = ""; IsEditorOpen = true; }
    [RelayCommand] private void EditProject(CrmProject? project) { if (project is null) return; Editor = new ProjectFormViewModel(); Editor.Load(project); EditorError = ""; IsEditorOpen = true; }
    [RelayCommand] private void CancelEdit() => IsEditorOpen = false;
    [RelayCommand] private void SaveProject() { if (_currentUser is null) return; if (Editor.CustomerId == 0 || string.IsNullOrWhiteSpace(Editor.Name) || Editor.Progress is < 0 or > 100) { EditorError = "Customer, project name and a progress from 0 to 100 are required."; return; } _service.Save(Editor.ToEntity(), _currentUser); IsEditorOpen = false; Refresh(); }
}
