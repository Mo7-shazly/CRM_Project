# Nexus CRM — Phase 1

WPF .NET 8 CRM foundation with MVVM, Material Design, Entity Framework Core, role-based login, and a live database-backed dashboard.

## Run locally

```powershell
dotnet run --project .\CRM.Desktop
```

Demo users all use password `123456`:

- `admin@crm.local` — Admin
- `manager@crm.local` — Manager
- `sales@crm.local` — Sales

## Shared-office setup

SQLite is used by default only for single-machine development. For multiple connected devices, install SQL Server (or SQL Server Express) on the office server, create the database, then set the same environment variable on **each** client computer before launching the app:

```powershell
[Environment]::SetEnvironmentVariable('CRM_SQLSERVER_CONNECTION', 'Server=SERVER-NAME\\SQLEXPRESS;Database=NexusCrm;Trusted_Connection=True;TrustServerCertificate=True;', 'User')
```

Restart the app after setting it. On first launch, Phase 1 creates its database tables and demo data automatically. For production, use a dedicated SQL account, restrict the firewall to the office network, and replace `EnsureCreated` with EF migrations before release.
