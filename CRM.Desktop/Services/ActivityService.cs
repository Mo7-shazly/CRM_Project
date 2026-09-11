using CRM.Desktop.Data;
using CRM.Desktop.Models;
using Microsoft.EntityFrameworkCore;

namespace CRM.Desktop.Services;

public sealed class ActivityService
{
    public List<CustomerActivity> GetActivities(AppUser user, string? type = null, string? status = null)
    {
        using var db = new CrmDbContext();
        IQueryable<CustomerActivity> query = db.CustomerActivities.AsNoTracking().Include(x => x.Customer);
        if (user.Role == UserRole.Sales) query = query.Where(x => x.AssignedTo == user.DisplayName);
        if (!string.IsNullOrWhiteSpace(type) && type != "All types") query = query.Where(x => x.Type == type);
        if (status == "Open") query = query.Where(x => !x.IsCompleted);
        if (status == "Completed") query = query.Where(x => x.IsCompleted);
        return query.OrderBy(x => x.IsCompleted).ThenBy(x => x.ReminderAt ?? x.OccurredAt).ToList();
    }

    public List<Customer> GetCustomers() { using var db = new CrmDbContext(); return db.Customers.AsNoTracking().OrderBy(x => x.CompanyName).ToList(); }
    public List<string> GetSalesPeople() { using var db = new CrmDbContext(); return db.Users.AsNoTracking().Where(x => x.Role == UserRole.Sales && x.IsActive).Select(x => x.DisplayName).OrderBy(x => x).ToList(); }

    public void Save(CustomerActivity activity, AppUser user)
    {
        using var db = new CrmDbContext();
        if (user.Role == UserRole.Sales) activity.AssignedTo = user.DisplayName;
        if (activity.Id != 0)
        {
            var existing = db.CustomerActivities.SingleOrDefault(x => x.Id == activity.Id) ?? throw new InvalidOperationException("Activity was not found.");
            if (user.Role == UserRole.Sales && existing.AssignedTo != user.DisplayName) throw new UnauthorizedAccessException();
            existing.Type = activity.Type; existing.Title = activity.Title; existing.Details = activity.Details; existing.AssignedTo = activity.AssignedTo;
            existing.OccurredAt = activity.OccurredAt; existing.ReminderAt = activity.ReminderAt; existing.IsCompleted = activity.IsCompleted;
        }
        else
        {
            activity.OccurredAt = DateTime.Now;
            if (string.IsNullOrWhiteSpace(activity.AssignedTo)) activity.AssignedTo = user.DisplayName;
            db.CustomerActivities.Add(activity);
        }
        db.SaveChanges();
    }

    public void Complete(int id, AppUser user)
    {
        using var db = new CrmDbContext();
        var activity = db.CustomerActivities.SingleOrDefault(x => x.Id == id);
        if (activity is null) return;
        if (user.Role == UserRole.Sales && activity.AssignedTo != user.DisplayName) throw new UnauthorizedAccessException();
        activity.IsCompleted = true;
        db.SaveChanges();
    }
}
