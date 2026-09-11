using CRM.Desktop.Data;
using CRM.Desktop.Models;
using Microsoft.EntityFrameworkCore;

namespace CRM.Desktop.Services;

public sealed class CustomerService
{
    public List<Customer> GetCustomers(string query = "")
    {
        using var db = new CrmDbContext();
        var customers = db.Customers.AsNoTracking().Include(x => x.Contacts).AsQueryable();
        if (!string.IsNullOrWhiteSpace(query)) customers = customers.Where(x => x.CompanyName.Contains(query) || x.Phone.Contains(query) || x.Email.Contains(query));
        return customers.OrderBy(x => x.CompanyName).ToList();
    }
    public Customer? GetDetail(int id) { using var db = new CrmDbContext(); return db.Customers.Include(x => x.Contacts).Include(x => x.Activities.OrderByDescending(a => a.OccurredAt)).AsNoTracking().SingleOrDefault(x => x.Id == id); }
    public Customer Save(Customer customer)
    {
        using var db = new CrmDbContext();
        if (customer.Id == 0) { customer.CustomerCode = $"C-{(db.Customers.Max(x => (int?)x.Id) ?? 0) + 1:000}"; customer.CreatedAt = DateTime.Now; db.Customers.Add(customer); }
        else db.Customers.Update(customer);
        db.SaveChanges(); return customer;
    }
    public void Delete(int id) { using var db = new CrmDbContext(); var customer = db.Customers.Find(id); if (customer is not null) { db.Customers.Remove(customer); db.SaveChanges(); } }
    public void AddContact(int customerId, string name, string jobTitle, string phone, string email, bool primary)
    {
        using var db = new CrmDbContext();
        if (primary) foreach (var contact in db.ContactPeople.Where(x => x.CustomerId == customerId)) contact.IsPrimary = false;
        db.ContactPeople.Add(new ContactPerson { CustomerId = customerId, FullName = name.Trim(), JobTitle = jobTitle.Trim(), Phone = phone.Trim(), Email = email.Trim(), IsPrimary = primary }); db.SaveChanges();
    }
}
