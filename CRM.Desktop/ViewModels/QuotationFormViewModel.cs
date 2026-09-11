using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CRM.Desktop.Models;

namespace CRM.Desktop.ViewModels;

public partial class QuotationFormViewModel : ObservableObject
{
    [ObservableProperty] private int id;
    [ObservableProperty] private int customerId;
    [ObservableProperty] private QuotationStatus status = QuotationStatus.Draft;
    [ObservableProperty] private DateTime issueDate = DateTime.Today;
    [ObservableProperty] private DateTime? validUntil = DateTime.Today.AddDays(30);
    [ObservableProperty] private string notes = "";
    public ObservableCollection<QuotationItem> Items { get; } = new();
    public void Load(Quotation quote) { Id = quote.Id; CustomerId = quote.CustomerId; Status = quote.Status; IssueDate = quote.IssueDate; ValidUntil = quote.ValidUntil; Notes = quote.Notes; Items.Clear(); foreach (var item in quote.Items) Items.Add(new QuotationItem { Id = item.Id, Description = item.Description, Quantity = item.Quantity, UnitPrice = item.UnitPrice }); }
    public Quotation ToEntity() => new() { Id = Id, CustomerId = CustomerId, Status = Status, IssueDate = IssueDate, ValidUntil = ValidUntil, Notes = Notes.Trim(), Items = Items.Select(x => new QuotationItem { Id = x.Id, Description = x.Description.Trim(), Quantity = x.Quantity, UnitPrice = x.UnitPrice }).ToList() };
}
