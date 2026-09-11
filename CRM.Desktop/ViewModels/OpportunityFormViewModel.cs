using CommunityToolkit.Mvvm.ComponentModel;
using CRM.Desktop.Models;

namespace CRM.Desktop.ViewModels;

public partial class OpportunityFormViewModel : ObservableObject
{
    [ObservableProperty] private int id;
    [ObservableProperty] private int? customerId;
    [ObservableProperty] private string name = "";
    [ObservableProperty] private string ownerName = "";
    [ObservableProperty] private DealStage stage = DealStage.NewLead;
    [ObservableProperty] private decimal value;
    [ObservableProperty] private DateTime? expectedCloseAt;
    public void Load(Opportunity value) { Id = value.Id; CustomerId = value.CustomerId; Name = value.Name; OwnerName = value.OwnerName; Stage = value.Stage; Value = value.Value; ExpectedCloseAt = value.ExpectedCloseAt; }
    public Opportunity ToEntity() => new() { Id = Id, CustomerId = CustomerId, Name = Name.Trim(), OwnerName = OwnerName, Stage = Stage, Value = Value, ExpectedCloseAt = ExpectedCloseAt };
}
