using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRM.Desktop.Models;
using CRM.Desktop.Services;

namespace CRM.Desktop.ViewModels;
public partial class CustomersViewModel : ObservableObject
{
    private readonly CustomerService _service;
    public ObservableCollection<Customer> Customers { get; } = new();
    public Array CustomerStatuses => Enum.GetValues<CustomerStatus>();
    public Array CustomerPriorities => Enum.GetValues<CustomerPriority>();
    public ObservableCollection<string> Tabs { get; } = new(new[] { "Overview", "Contacts", "Activities", "Opportunities", "Quotations", "Projects", "Files" });
    [ObservableProperty] private string searchText = "";
    [ObservableProperty] private Customer? selectedCustomer;
    [ObservableProperty] private Customer? detail;
    [ObservableProperty] private string selectedTab = "Overview";
    [ObservableProperty] private bool isDetailOpen;
    [ObservableProperty] private bool isEditorOpen;
    [ObservableProperty] private CustomerFormViewModel editor = new();
    [ObservableProperty] private string editorError = "";
    [ObservableProperty] private bool isNewCustomer;
    [ObservableProperty] private string newContactName = "";
    [ObservableProperty] private string newContactTitle = "";
    [ObservableProperty] private string newContactPhone = "";
    [ObservableProperty] private string newContactEmail = "";
    [ObservableProperty] private bool newContactPrimary;
    public CustomersViewModel(CustomerService service) { _service = service; Refresh(); }
    partial void OnSearchTextChanged(string value) => Refresh();
    [RelayCommand] private void Refresh() { Customers.Clear(); foreach (var customer in _service.GetCustomers(SearchText)) Customers.Add(customer); }
    [RelayCommand] private void OpenDetail(Customer? customer) { if (customer is null) return; Detail = _service.GetDetail(customer.Id); IsDetailOpen = Detail is not null; SelectedTab = "Overview"; }
    [RelayCommand] private void CloseDetail() => IsDetailOpen = false;
    [RelayCommand] private void NewCustomer() { Editor = new CustomerFormViewModel(); IsNewCustomer = true; EditorError = ""; IsEditorOpen = true; }
    [RelayCommand] private void EditCustomer(Customer? customer) { if (customer is null) return; Editor = new CustomerFormViewModel(); Editor.Load(customer); IsNewCustomer = false; EditorError = ""; IsEditorOpen = true; }
    [RelayCommand] private void SaveCustomer() { if (string.IsNullOrWhiteSpace(Editor.CompanyName)) { EditorError = "Company name is required."; return; } var customer = _service.Save(Editor.ToEntity()); IsEditorOpen = false; Refresh(); OpenDetail(customer); }
    [RelayCommand] private void CancelEdit() => IsEditorOpen = false;
    [RelayCommand] private void DeleteCustomer(Customer? customer) { if (customer is null) return; _service.Delete(customer.Id); IsDetailOpen = false; Refresh(); }
    [RelayCommand] private void AddContact()
    {
        if (Detail is null || string.IsNullOrWhiteSpace(NewContactName)) return;
        _service.AddContact(Detail.Id, NewContactName, NewContactTitle, NewContactPhone, NewContactEmail, NewContactPrimary);
        NewContactName = NewContactTitle = NewContactPhone = NewContactEmail = ""; NewContactPrimary = false; Detail = _service.GetDetail(Detail.Id); Refresh();
    }
}
