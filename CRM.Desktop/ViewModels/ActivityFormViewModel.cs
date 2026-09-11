using CommunityToolkit.Mvvm.ComponentModel;
using CRM.Desktop.Models;

namespace CRM.Desktop.ViewModels;

public partial class ActivityFormViewModel : ObservableObject
{
    [ObservableProperty] private int customerId;
    [ObservableProperty] private string type = "Call";
    [ObservableProperty] private string title = "";
    [ObservableProperty] private string details = "";
    [ObservableProperty] private string assignedTo = "";
    [ObservableProperty] private DateTime? reminderAt;

    public CustomerActivity ToEntity() => new()
    {
        CustomerId = CustomerId, Type = Type, Title = Title.Trim(), Details = Details.Trim(), AssignedTo = AssignedTo, ReminderAt = ReminderAt
    };
}
