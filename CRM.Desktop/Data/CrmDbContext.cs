using CRM.Desktop.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace CRM.Desktop.Data;

public sealed class CrmDbContext : DbContext
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<LeadSource> LeadSources => Set<LeadSource>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<ContactPerson> ContactPeople => Set<ContactPerson>();
    public DbSet<CustomerActivity> CustomerActivities => Set<CustomerActivity>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationItem> QuotationItems => Set<QuotationItem>();
    public DbSet<CrmProject> Projects => Set<CrmProject>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var sharedConnection = Environment.GetEnvironmentVariable("CRM_SQLSERVER_CONNECTION");
        if (!string.IsNullOrWhiteSpace(sharedConnection))
            options.UseSqlServer(sharedConnection);
        else
            options.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "crm.db")}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Lead>().Property(x => x.EstimatedValue).HasPrecision(18, 2);
        modelBuilder.Entity<Lead>().HasIndex(x => x.LeadCode).IsUnique();
        modelBuilder.Entity<Lead>().HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Lead>().HasOne(x => x.Opportunity).WithMany().HasForeignKey(x => x.OpportunityId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Customer>().HasIndex(x => x.CustomerCode).IsUnique();
        modelBuilder.Entity<ContactPerson>().HasOne(x => x.Customer).WithMany(x => x.Contacts).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CustomerActivity>().HasOne(x => x.Customer).WithMany(x => x.Activities).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Opportunity>().HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Opportunity>().Property(x => x.Value).HasPrecision(18, 2);
        modelBuilder.Entity<Quotation>().HasIndex(x => x.QuoteNumber).IsUnique();
        modelBuilder.Entity<Quotation>().HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<QuotationItem>().HasOne(x => x.Quotation).WithMany(x => x.Items).HasForeignKey(x => x.QuotationId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<QuotationItem>().Property(x => x.Quantity).HasPrecision(18, 2);
        modelBuilder.Entity<QuotationItem>().Property(x => x.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<CrmProject>().HasIndex(x => x.ProjectCode).IsUnique();
        modelBuilder.Entity<CrmProject>().HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
    }
}
