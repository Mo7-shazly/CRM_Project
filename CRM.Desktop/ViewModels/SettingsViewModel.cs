using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRM.Desktop.Models;
using CRM.Desktop.Services;

namespace CRM.Desktop.ViewModels;
public partial class SettingsViewModel : ObservableObject
{
    private readonly UserService _users; private AppUser? _actor;
    public ObservableCollection<AppUser> Users { get; } = [];
    public Array Roles => Enum.GetValues<UserRole>();
    [ObservableProperty] private string companyName = "CRM"; [ObservableProperty] private string currency = "EGP"; [ObservableProperty] private string saveMessage = "";
    [ObservableProperty] private AppUser editor = new() { IsActive = true, Role = UserRole.Sales }; [ObservableProperty] private string newPassword = ""; [ObservableProperty] private string userError = "";
    public bool CanManageUsers => _actor?.Role == UserRole.Admin;
    public SettingsViewModel(UserService users) => _users = users;
    public void Load(AppUser actor) { _actor = actor; OnPropertyChanged(nameof(CanManageUsers)); if (CanManageUsers) RefreshUsers(); }
    [RelayCommand] private void Save() => SaveMessage = "Settings saved for this session.";
    [RelayCommand] private void RefreshUsers() { if (!CanManageUsers || _actor is null) return; Users.Clear(); foreach (var user in _users.GetUsers(_actor)) Users.Add(user); }
    [RelayCommand] private void NewUser() { Editor = new AppUser { IsActive = true, Role = UserRole.Sales }; NewPassword = ""; UserError = ""; }
    [RelayCommand] private void EditUser(AppUser? user) { if (user is null) return; Editor = new AppUser { Id = user.Id, DisplayName = user.DisplayName, Email = user.Email, Role = user.Role, IsActive = user.IsActive }; NewPassword = ""; UserError = ""; }
    [RelayCommand] private void SaveUser() { if (_actor is null) return; try { _users.Save(Editor, NewPassword, _actor); NewUser(); RefreshUsers(); } catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException) { UserError = ex.Message; } }
}
