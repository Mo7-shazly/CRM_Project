using CommunityToolkit.Mvvm.ComponentModel;
using CRM.Desktop.Models;

namespace CRM.Desktop.ViewModels;
public partial class CustomerFormViewModel : ObservableObject
{
    [ObservableProperty] private int id; [ObservableProperty] private string companyName = ""; [ObservableProperty] private string industry = ""; [ObservableProperty] private string companySize = ""; [ObservableProperty] private string website = ""; [ObservableProperty] private string phone = ""; [ObservableProperty] private string email = ""; [ObservableProperty] private string address = ""; [ObservableProperty] private CustomerStatus status = CustomerStatus.Active; [ObservableProperty] private CustomerPriority priority = CustomerPriority.Medium;
    public void Load(Customer c) { Id = c.Id; CompanyName = c.CompanyName; Industry = c.Industry; CompanySize = c.CompanySize; Website = c.Website; Phone = c.Phone; Email = c.Email; Address = c.Address; Status = c.Status; Priority = c.Priority; }
    public Customer ToEntity() => new() { Id = Id, CompanyName = CompanyName.Trim(), Industry = Industry.Trim(), CompanySize = CompanySize.Trim(), Website = Website.Trim(), Phone = Phone.Trim(), Email = Email.Trim(), Address = Address.Trim(), Status = Status, Priority = Priority };
}
