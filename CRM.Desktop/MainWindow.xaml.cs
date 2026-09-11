using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using CRM.Desktop.Services;
using CRM.Desktop.ViewModels;

namespace CRM.Desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(AuthService authService, DashboardService dashboardService)
    {
        InitializeComponent();
        DataContext = new MainViewModel(authService, dashboardService, new LeadService(), new CustomerService(), new ActivityService(), new OpportunityService(), new QuotationService(), new ProjectService(), new ReportService(), new GlobalSearchService());
    }

    private void PasswordInput_OnPasswordChanged(object sender, RoutedEventArgs e)
        => ((MainViewModel)DataContext).Password = PasswordInput.Password;
}
