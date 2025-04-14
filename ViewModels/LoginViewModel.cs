using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Threading.Tasks;
using Percuro.Services;
using BCrypt.Net;
using Percuro.Views;


namespace Percuro.ViewModels;

public partial class LoginViewModel : ViewModelBase{
    
    [ObservableProperty]
    private string _username ="";

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private string _selectedRole = "Role A";
    [ObservableProperty]
    private List<string> _availableRoles = new() {"Role A","Role B"};

   private readonly DatabaseService _databaseService;

    public LoginViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
    }
[RelayCommand]
private async Task Login()
{
    Console.WriteLine($"Login attempt as {Username} with role {SelectedRole}");

    if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
    {
        Console.WriteLine("Fehler: Benutzername oder Passwort ist leer.");
        return;
    }

    var user = await _databaseService.GetUserByUsernameAsync(Username);

    if (user == null)
    {
        Console.WriteLine("Fehler: Benutzer nicht gefunden.");
        return;
    }

    if (BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash))
    {
        Console.WriteLine("Login erfolgreich!");
          // UI-Thread erzwingen und Parent nutzen
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            (Parent as MainWindowViewModel)!.CurrentViewModel = new DashboardViewModel();
        });
    }
    else
    {
        Console.WriteLine("Fehler: Falsches Passwort.");
    }
}
 
        [RelayCommand]
        private async Task CreateAccount()
        {
            // Hier die Logik für das Erstellen eines neuen Benutzerkontos
            bool success = await _databaseService.CreateAccountAsync(Username, Password, SelectedRole);

            if (success)
            {
                Console.WriteLine($"Account für {Username} erfolgreich erstellt!");
                // Du kannst hier auch eine Bestätigungsmeldung im UI anzeigenc
            }
            else
            {
                Console.WriteLine("Fehler beim Erstellen des Benutzerkontos!");
            }
        }

}

