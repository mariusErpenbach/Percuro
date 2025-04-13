using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Threading.Tasks;
using Percuro.Services;
using BCrypt.Net; 


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

    // Validierung der Eingaben
    if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
    {
        Console.WriteLine("Fehler: Benutzername oder Passwort ist leer.");
        return;
    }

    // Benutzer aus der DB abfragen (wir gehen davon aus, dass du die Logik zum Abfragen der DB hast)
    var user = await _databaseService.GetUserByUsernameAsync(Username);

    if (user == null)
    {
        Console.WriteLine("Fehler: Benutzer nicht gefunden.");
        return;
    }

    // Prüfen, ob das Passwort korrekt ist
    if (BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash))
    {
        Console.WriteLine("Login erfolgreich!");
        // Weiteren Code für erfolgreiche Anmeldung hier hinzufügen
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
                // Du kannst hier auch eine Bestätigungsmeldung im UI anzeigen
            }
            else
            {
                Console.WriteLine("Fehler beim Erstellen des Benutzerkontos!");
            }
        }

}

