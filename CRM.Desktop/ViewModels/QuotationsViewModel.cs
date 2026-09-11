using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRM.Desktop.Models;
using CRM.Desktop.Services;
using Microsoft.Win32;

namespace CRM.Desktop.ViewModels;

public partial class QuotationsViewModel : ObservableObject
{
    private readonly QuotationService _service;
    private AppUser? _currentUser;
    public ObservableCollection<Quotation> Quotations { get; } = new();
    public ObservableCollection<Customer> Customers { get; } = new();
    public Array Statuses => Enum.GetValues<QuotationStatus>();
    [ObservableProperty] private bool isEditorOpen;
    [ObservableProperty] private QuotationFormViewModel editor = new();
    [ObservableProperty] private string editorError = "";
    public QuotationsViewModel(QuotationService service) { _service = service; }
    public void Load(AppUser user) { _currentUser = user; Customers.Clear(); foreach (var customer in _service.GetCustomers()) Customers.Add(customer); Refresh(); }
    [RelayCommand] private void Refresh() { if (_currentUser is null) return; Quotations.Clear(); foreach (var quote in _service.GetQuotations(_currentUser)) Quotations.Add(quote); }
    [RelayCommand] private void NewQuotation() { Editor = new QuotationFormViewModel(); Editor.Items.Add(new QuotationItem()); EditorError = ""; IsEditorOpen = true; }
    [RelayCommand] private void EditQuotation(Quotation? quote) { if (quote is null) return; Editor = new QuotationFormViewModel(); Editor.Load(quote); EditorError = ""; IsEditorOpen = true; }
    [RelayCommand] private void AddItem() => Editor.Items.Add(new QuotationItem());
    [RelayCommand] private void RemoveItem(QuotationItem? item) { if (item is not null) Editor.Items.Remove(item); }
    [RelayCommand] private void CancelEdit() => IsEditorOpen = false;
    [RelayCommand] private void SaveQuotation()
    {
        if (Editor.CustomerId == 0 || !Editor.Items.Any() || Editor.Items.Any(x => string.IsNullOrWhiteSpace(x.Description) || x.Quantity <= 0 || x.UnitPrice < 0)) { EditorError = "Customer and valid quotation items are required."; return; }
        if (_currentUser is null) return; try { _service.Save(Editor.ToEntity(), _currentUser); IsEditorOpen = false; Refresh(); } catch (UnauthorizedAccessException ex) { EditorError = ex.Message; }
    }
    [RelayCommand] private void ExportPdf(Quotation? quote)
    {
        if (quote is null) return;
        if (_currentUser is null) return; var fullQuote = _service.GetQuotation(quote.Id, _currentUser); if (fullQuote is null) return;
        var dialog = new SaveFileDialog { Filter = "PDF document (*.pdf)|*.pdf", FileName = $"{fullQuote.QuoteNumber}.pdf" };
        if (dialog.ShowDialog() == true) File.WriteAllBytes(dialog.FileName, _service.BuildPdf(fullQuote));
    }
}
