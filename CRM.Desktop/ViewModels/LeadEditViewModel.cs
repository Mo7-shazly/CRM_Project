using CommunityToolkit.Mvvm.ComponentModel;
using CRM.Desktop.Models;

namespace CRM.Desktop.ViewModels;

public partial class LeadEditViewModel : ObservableObject
{
    [ObservableProperty] private int id;
    [ObservableProperty] private int? customerId;
    [ObservableProperty] private string company = "";
    [ObservableProperty] private string contactName = "";
    [ObservableProperty] private string phone = "";
    [ObservableProperty] private string whatsApp = "";
    [ObservableProperty] private string email = "";
    [ObservableProperty] private string industry = "";
    [ObservableProperty] private string source = "";
    [ObservableProperty] private string assignedTo = "";
    [ObservableProperty] private LeadStatus status = LeadStatus.New;
    [ObservableProperty] private int score;
    [ObservableProperty] private decimal estimatedValue;
    [ObservableProperty] private string notes = "";
    [ObservableProperty] private DateTime? nextFollowUpAt;
    public void Load(Lead lead) { Id = lead.Id; CustomerId = lead.CustomerId; Company = lead.Company; ContactName = lead.ContactName; Phone = lead.Phone; WhatsApp = lead.WhatsApp; Email = lead.Email; Industry = lead.Industry; Source = lead.Source; AssignedTo = lead.AssignedTo; Status = lead.Status; Score = lead.Score; EstimatedValue = lead.EstimatedValue; Notes = lead.Notes; NextFollowUpAt = lead.NextFollowUpAt; }
    public Lead ToEntity() => new() { Id = Id, CustomerId = CustomerId, Company = Company.Trim(), ContactName = ContactName.Trim(), Phone = Phone.Trim(), WhatsApp = WhatsApp.Trim(), Email = Email.Trim(), Industry = Industry.Trim(), Source = Source, AssignedTo = AssignedTo, Status = Status, Score = Score, EstimatedValue = EstimatedValue, Notes = Notes.Trim(), NextFollowUpAt = NextFollowUpAt };
}
