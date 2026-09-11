# Shared company deployment

The desktop program is multi-user only when every installed copy uses the same SQL Server database. SQLite (`crm.db`) is a single-device database and must not be copied between employees.

## Server preparation

1. Install SQL Server Express or SQL Server on a company server that is always on.
2. Create an empty database, for example `NexusCrm`.
3. Create a least-privilege SQL login with read/write access only to `NexusCrm`.
4. Allow TCP access to SQL Server only from the company LAN and take scheduled backups.

## Workstation configuration

Before launching the app for the first time on every workstation, set a machine environment variable named `CRM_SQLSERVER_CONNECTION` to the same connection string, for example:

```text
Server=CRM-SERVER\\SQLEXPRESS;Database=NexusCrm;User Id=crm_app;Password=replace-with-a-strong-password;TrustServerCertificate=True;
```

The first app startup creates the schema in the shared database and seeds the initial accounts only if no accounts exist. Run the first launch once on the server/admin device; then install the same build on all workstations.

## Access model

- Admin and Manager see and manage company data.
- Manager/Admin creates customers and assigns each Lead to a Sales user.
- Sales can view, edit, and delete only Leads assigned to their own account. The same ownership rule already applies to Activities, Opportunities, Projects, dashboard, and reports.
- Customers are shared company records, so Sales can select the correct customer while handling their assigned work.

## Before production

- Replace the seeded test passwords and use proper password hashing with a per-user salt.
- Add a SQL Server backup plan and test restoring it.
- Do not expose SQL Server directly to the public internet; use a VPN for offsite workers.
