using System.Configuration;
using System.Data;
using System.Windows;

using CRM.Desktop.Data;
using CRM.Desktop.Services;

namespace CRM.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        using var db = new CrmDbContext();
        DbSchemaUpgrader.Upgrade(db);
        DbSeeder.Seed(db);

        var window = new MainWindow(new AuthService(), new DashboardService());
        window.Show();
    }
}

