using CommunityToolkit.Mvvm.ComponentModel;
using CRM.Desktop.Models;
namespace CRM.Desktop.ViewModels;
public partial class ProjectFormViewModel : ObservableObject
{
    [ObservableProperty] private int id; [ObservableProperty] private int customerId; [ObservableProperty] private string name = ""; [ObservableProperty] private string assignedTo = ""; [ObservableProperty] private ProjectStatus status = ProjectStatus.Planned; [ObservableProperty] private int progress; [ObservableProperty] private DateTime startDate = DateTime.Today; [ObservableProperty] private DateTime? endDate; [ObservableProperty] private string notes = "";
    public void Load(CrmProject value) { Id = value.Id; CustomerId = value.CustomerId; Name = value.Name; AssignedTo = value.AssignedTo; Status = value.Status; Progress = value.Progress; StartDate = value.StartDate; EndDate = value.EndDate; Notes = value.Notes; }
    public CrmProject ToEntity() => new() { Id = Id, CustomerId = CustomerId, Name = Name.Trim(), AssignedTo = AssignedTo, Status = Status, Progress = Progress, StartDate = StartDate, EndDate = EndDate, Notes = Notes.Trim() };
}
