using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Percuro.Models.MitarbeiterModels;
using Percuro.Services.MitarbeiterServices;


namespace Percuro.ViewModels.EnterpriseViewModels.HR.MitarbeiterNS;

public partial class NewMitarbeiterViewModel : ViewModelBase
{
    private readonly MitarbeiterDatabaseService _mitarbeiterService;

    public NewMitarbeiterViewModel()
    {
        _mitarbeiterService = new MitarbeiterDatabaseService();
        SaveMitarbeiterAsyncCommand = new AsyncRelayCommand(SaveMitarbeiterAsync);
    }

    // Adressbuch-Felder
    [ObservableProperty]
    private string? strasse;

    [ObservableProperty]
    private string? hausnummer;

    [ObservableProperty]
    private string? plz;

    [ObservableProperty]
    private string? stadt;

    [ObservableProperty]
    private string? land;

    [ObservableProperty]
    private string? adresszusatz;

    [ObservableProperty]
    private string? typ;

    // Mitarbeiter-Felder
    [ObservableProperty]
    private string? vorname;

    [ObservableProperty]
    private string? nachname;

    [ObservableProperty]
    private DateTime? geburtsdatum;

    [ObservableProperty]
    private DateTime? eintrittsdatum;

    [ObservableProperty]
    private int? positionId;

    [ObservableProperty]
    private string? telefon;

    [ObservableProperty]
    private string? email;

    [ObservableProperty]
    private bool aktiv = true;

    [ObservableProperty]
    private decimal? gehalt;

    [ObservableProperty]
    private bool istAdmin = false;

    [ObservableProperty]
    private string? bildUrl;

    [ObservableProperty]
    private string? notizen;
    [ObservableProperty]
    private int? adressbuchId;


    [RelayCommand]
    public async Task SaveMitarbeiterAsync()
    {
        try
        {
            // Save address first
            int adressbuchId = await _mitarbeiterService.SaveAdressbuchAsync(Strasse, Hausnummer, Plz, Stadt, Land, Adresszusatz, Typ);

            // Create a new Mitarbeiter object
            var mitarbeiter = new Mitarbeiter
            {
                Vorname = Vorname,
                Nachname = Nachname,
                Geburtsdatum = Geburtsdatum,
                Eintrittsdatum = Eintrittsdatum,
                PositionId = PositionId,
                Telefon = Telefon,
                Email = Email,
                Aktiv = Aktiv,
                Gehalt = Gehalt,
                IstAdmin = IstAdmin,
                BildUrl = BildUrl,
                Notizen = Notizen,
                AdressbuchId = adressbuchId
            };

            // Save Mitarbeiter
            await _mitarbeiterService.AddMitarbeiterAsync(mitarbeiter);

            // Optionally, navigate back or show success message
            Console.WriteLine("Mitarbeiter erfolgreich gespeichert.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Speichern des Mitarbeiters: {ex.Message}");
        }
    }
    [RelayCommand]
    public void ToMitarbeiterView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new MitarbeiterViewModel();
        }
    }

    public IRelayCommand SaveMitarbeiterAsyncCommand { get; private set; }
}