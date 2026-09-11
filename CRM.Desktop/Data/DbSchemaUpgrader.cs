using Microsoft.EntityFrameworkCore;

namespace CRM.Desktop.Data;

/// <summary>Non-destructive schema upgrades for the Phase 1 SQLite development database.</summary>
public static class DbSchemaUpgrader
{
    public static void Upgrade(CrmDbContext db)
    {
        db.Database.EnsureCreated();
        if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.Sqlite") return;
        var existing = db.Database.SqlQueryRaw<string>("SELECT name AS Value FROM pragma_table_info('Leads')").ToList();
        var columns = new Dictionary<string, string>
        {
            ["LeadCode"] = "TEXT NOT NULL DEFAULT ''", ["CustomerId"] = "INTEGER NULL", ["OpportunityId"] = "INTEGER NULL", ["Phone"] = "TEXT NOT NULL DEFAULT ''", ["WhatsApp"] = "TEXT NOT NULL DEFAULT ''", ["Email"] = "TEXT NOT NULL DEFAULT ''", ["Industry"] = "TEXT NOT NULL DEFAULT ''", ["Source"] = "TEXT NOT NULL DEFAULT ''", ["Score"] = "INTEGER NOT NULL DEFAULT 0", ["Notes"] = "TEXT NOT NULL DEFAULT ''", ["LastContactAt"] = "TEXT NULL", ["NextFollowUpAt"] = "TEXT NULL"
        };
        foreach (var column in columns.Where(x => !existing.Contains(x.Key, StringComparer.OrdinalIgnoreCase)))
            // Both parts are selected from the fixed dictionary above, never user input.
            db.Database.ExecuteSqlRaw(string.Concat("ALTER TABLE Leads ADD COLUMN ", column.Key, " ", column.Value));
        db.Database.ExecuteSqlRaw("UPDATE Leads SET LeadCode = 'L-' || printf('%03d', Id) WHERE LeadCode = ''");
        db.Database.ExecuteSqlRaw("CREATE UNIQUE INDEX IF NOT EXISTS IX_Leads_LeadCode ON Leads (LeadCode)");
        db.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS LeadSources (Id INTEGER NOT NULL CONSTRAINT PK_LeadSources PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, IsActive INTEGER NOT NULL)");
        db.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS Customers (Id INTEGER NOT NULL CONSTRAINT PK_Customers PRIMARY KEY AUTOINCREMENT, CustomerCode TEXT NOT NULL, CompanyName TEXT NOT NULL, Industry TEXT NOT NULL, CompanySize TEXT NOT NULL, Website TEXT NOT NULL, Phone TEXT NOT NULL, Email TEXT NOT NULL, Address TEXT NOT NULL, Status INTEGER NOT NULL, Priority INTEGER NOT NULL DEFAULT 1, CreatedAt TEXT NOT NULL)");
        var customerColumns = db.Database.SqlQueryRaw<string>("SELECT name AS Value FROM pragma_table_info('Customers')").ToList();
        if (!customerColumns.Contains("Priority", StringComparer.OrdinalIgnoreCase)) db.Database.ExecuteSqlRaw("ALTER TABLE Customers ADD COLUMN Priority INTEGER NOT NULL DEFAULT 1");
        db.Database.ExecuteSqlRaw("CREATE UNIQUE INDEX IF NOT EXISTS IX_Customers_CustomerCode ON Customers (CustomerCode)");
        db.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS ContactPeople (Id INTEGER NOT NULL CONSTRAINT PK_ContactPeople PRIMARY KEY AUTOINCREMENT, CustomerId INTEGER NOT NULL, FullName TEXT NOT NULL, JobTitle TEXT NOT NULL, Phone TEXT NOT NULL, Email TEXT NOT NULL, IsPrimary INTEGER NOT NULL, FOREIGN KEY(CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE)");
        db.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS CustomerActivities (Id INTEGER NOT NULL CONSTRAINT PK_CustomerActivities PRIMARY KEY AUTOINCREMENT, CustomerId INTEGER NOT NULL, Type TEXT NOT NULL, Title TEXT NOT NULL, Details TEXT NOT NULL, AssignedTo TEXT NOT NULL DEFAULT '', OccurredAt TEXT NOT NULL, ReminderAt TEXT NULL, IsCompleted INTEGER NOT NULL DEFAULT 0, FOREIGN KEY(CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE)");
        var activityColumns = db.Database.SqlQueryRaw<string>("SELECT name AS Value FROM pragma_table_info('CustomerActivities')").ToList();
        var newActivityColumns = new Dictionary<string, string>
        {
            ["AssignedTo"] = "TEXT NOT NULL DEFAULT ''", ["ReminderAt"] = "TEXT NULL", ["IsCompleted"] = "INTEGER NOT NULL DEFAULT 0"
        };
        foreach (var column in newActivityColumns.Where(x => !activityColumns.Contains(x.Key, StringComparer.OrdinalIgnoreCase)))
            db.Database.ExecuteSqlRaw(string.Concat("ALTER TABLE CustomerActivities ADD COLUMN ", column.Key, " ", column.Value));
        db.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS Opportunities (Id INTEGER NOT NULL CONSTRAINT PK_Opportunities PRIMARY KEY AUTOINCREMENT, CustomerId INTEGER NULL, Name TEXT NOT NULL, OwnerName TEXT NOT NULL, Stage INTEGER NOT NULL, Value TEXT NOT NULL, CreatedAt TEXT NOT NULL, ExpectedCloseAt TEXT NULL, FOREIGN KEY(CustomerId) REFERENCES Customers(Id) ON DELETE SET NULL)");
        var opportunityColumns = db.Database.SqlQueryRaw<string>("SELECT name AS Value FROM pragma_table_info('Opportunities')").ToList();
        var newOpportunityColumns = new Dictionary<string, string> { ["CustomerId"] = "INTEGER NULL", ["ExpectedCloseAt"] = "TEXT NULL" };
        foreach (var column in newOpportunityColumns.Where(x => !opportunityColumns.Contains(x.Key, StringComparer.OrdinalIgnoreCase)))
            db.Database.ExecuteSqlRaw(string.Concat("ALTER TABLE Opportunities ADD COLUMN ", column.Key, " ", column.Value));
        db.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS Quotations (Id INTEGER NOT NULL CONSTRAINT PK_Quotations PRIMARY KEY AUTOINCREMENT, QuoteNumber TEXT NOT NULL, CustomerId INTEGER NOT NULL, OwnerName TEXT NOT NULL DEFAULT '', Status INTEGER NOT NULL, IssueDate TEXT NOT NULL, ValidUntil TEXT NULL, Notes TEXT NOT NULL, FOREIGN KEY(CustomerId) REFERENCES Customers(Id) ON DELETE RESTRICT)");
        var quotationColumns = db.Database.SqlQueryRaw<string>("SELECT name AS Value FROM pragma_table_info('Quotations')").ToList();
        if (!quotationColumns.Contains("OwnerName", StringComparer.OrdinalIgnoreCase)) db.Database.ExecuteSqlRaw("ALTER TABLE Quotations ADD COLUMN OwnerName TEXT NOT NULL DEFAULT ''");
        db.Database.ExecuteSqlRaw("CREATE UNIQUE INDEX IF NOT EXISTS IX_Quotations_QuoteNumber ON Quotations (QuoteNumber)");
        db.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS QuotationItems (Id INTEGER NOT NULL CONSTRAINT PK_QuotationItems PRIMARY KEY AUTOINCREMENT, QuotationId INTEGER NOT NULL, Description TEXT NOT NULL, Quantity TEXT NOT NULL, UnitPrice TEXT NOT NULL, FOREIGN KEY(QuotationId) REFERENCES Quotations(Id) ON DELETE CASCADE)");
        db.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS Projects (Id INTEGER NOT NULL CONSTRAINT PK_Projects PRIMARY KEY AUTOINCREMENT, ProjectCode TEXT NOT NULL, CustomerId INTEGER NOT NULL, Name TEXT NOT NULL, AssignedTo TEXT NOT NULL, Status INTEGER NOT NULL, Progress INTEGER NOT NULL, StartDate TEXT NOT NULL, EndDate TEXT NULL, Notes TEXT NOT NULL, FOREIGN KEY(CustomerId) REFERENCES Customers(Id) ON DELETE RESTRICT)");
        db.Database.ExecuteSqlRaw("CREATE UNIQUE INDEX IF NOT EXISTS IX_Projects_ProjectCode ON Projects (ProjectCode)");
    }
}
