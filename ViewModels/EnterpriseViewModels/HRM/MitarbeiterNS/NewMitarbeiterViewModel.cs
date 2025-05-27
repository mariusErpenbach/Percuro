using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Percuro.Models.HRModels;
using Percuro.Models.MitarbeiterModels;
using Percuro.Services;
using Percuro.Services.MitarbeiterServices;
using Percuro.ViewModels.EnterpriseViewModels;


namespace Percuro.ViewModels.EnterpriseViewModels.HRM.MitarbeiterNS;

public partial class NewMitarbeiterViewModel : ViewModelBase
{
    private readonly MitarbeiterDatabaseService _mitarbeiterService;

    public NewMitarbeiterViewModel()
    {
        _mitarbeiterService = new MitarbeiterDatabaseService();
        SaveMitarbeiterAsyncCommand = new AsyncRelayCommand(SaveMitarbeiterAsync);
        LoadPositionTitlesAsync().ConfigureAwait(false);
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
    private string? typ = "privat";

    // Mitarbeiter-Felder
    [ObservableProperty]
    private string? vorname;

    [ObservableProperty]
    private string? nachname;

    [ObservableProperty]
    private string? geburtsdatum;

    [ObservableProperty]
    private string? eintrittsdatum;

    [ObservableProperty]
    private string? telefon;

    [ObservableProperty]
    private string? email;

    [ObservableProperty]
    private bool aktiv = true;

    [ObservableProperty]
    private string? gehalt;

    [ObservableProperty]
    private bool istAdmin = false;

    [ObservableProperty]
    private string? bildUrl;

    [ObservableProperty]
    private string? notizen;
    [ObservableProperty]
    private int? adressbuchId;

   

    [ObservableProperty]
    private string? vornameError;

    [ObservableProperty]
    private string? nachnameError;

    [ObservableProperty]
    private string? strasseError;

    [ObservableProperty]
    private string? plzError;

    [ObservableProperty]
    private string? stadtError;

    [ObservableProperty]
    private string? landError;

    [ObservableProperty]
    private string? telefonError;

    [ObservableProperty]
    private string? emailError;

    [ObservableProperty]
    private string? gehaltError;

    [ObservableProperty]
    private string? positionIdError;

    [ObservableProperty]
    private string? geburtsdatumError;

    [ObservableProperty]
    private string? eintrittsdatumError;

    [ObservableProperty]
    private string? typError;

    [ObservableProperty]
    private string? hausnummerError;

    private readonly Dictionary<string, string?> fieldErrors = new();
    public IReadOnlyDictionary<string, string?> FieldErrors => fieldErrors;


    private void ValidateFields()
    {
        VornameError = !InputValidation.IsValidName(Vorname) ? "Vorname muss aus mehr als einem Buchstaben bestehen und nur Buchstaben enthalten." : null;
        NachnameError = !InputValidation.IsValidName(Nachname) ? "Nachname muss aus mehr als einem Buchstaben bestehen und nur Buchstaben enthalten." : null;
        StrasseError = !InputValidation.IsValidString(Strasse) ? "Straße darf nur Buchstaben enthalten." : null;
        HausnummerError = !InputValidation.IsValidNumber(Hausnummer) ? "Hausnummer muss eine Zahl sein." : null;
        PlzError = !InputValidation.IsValidPostalCode(Plz) ? "PLZ muss aus maximal 6 Zahlen bestehen." : null;
        StadtError = !InputValidation.IsValidString(Stadt) ? "Stadt darf nur Buchstaben enthalten." : null;
        LandError = !InputValidation.IsValidString(Land) ? "Land darf nur Buchstaben enthalten." : null;
        TelefonError = !InputValidation.IsValidPhoneNumber(Telefon) ? "Telefonnummer darf nur Zahlen enthalten." : null;
        EmailError = !InputValidation.IsValidEmail(Email) ? "Ungültige Email-Adresse." : null;
        GehaltError = (GehaltAsDecimal == null || GehaltAsDecimal <= 0) ? "Gehalt muss größer als 0 sein." : null;
        GeburtsdatumError = GeburtsdatumAsDateTime == null ? "Geburtsdatum muss im Format TT.MM.JJJJ sein." : null;
        EintrittsdatumError = EintrittsdatumAsDateTime == null ? "Eintrittsdatum muss im Format TT.MM.JJJJ sein." : null;
        TypError = string.IsNullOrWhiteSpace(Typ) ? "Typ ist erforderlich." : null;
        PositionError = string.IsNullOrEmpty(SelectedPosition) ? "Bitte wählen Sie eine Position aus." : null;
    }

    [RelayCommand]
    public async Task SaveMitarbeiterAsync()
    {
        try
        {
            ValidateFields();
            if (!string.IsNullOrEmpty(VornameError) || !string.IsNullOrEmpty(NachnameError) ||
                !string.IsNullOrEmpty(StrasseError) || !string.IsNullOrEmpty(PlzError) ||
                !string.IsNullOrEmpty(StadtError) || !string.IsNullOrEmpty(LandError) ||
                !string.IsNullOrEmpty(TelefonError) || !string.IsNullOrEmpty(EmailError) ||
                !string.IsNullOrEmpty(GehaltError) || !string.IsNullOrEmpty(PositionIdError) ||
                !string.IsNullOrEmpty(GeburtsdatumError) || !string.IsNullOrEmpty(EintrittsdatumError) ||
                !string.IsNullOrEmpty(TypError) || !string.IsNullOrEmpty(HausnummerError))
            {
                return;
            }

            Console.WriteLine("SaveMitarbeiterAsync started.");

            // Validate fields
            ValidateFields();
            Console.WriteLine("Validation completed. FieldErrors count: " + FieldErrors.Count);

            if (FieldErrors.Count > 0)
            {
                Console.WriteLine("Validation failed. Errors:");
                foreach (var error in FieldErrors)
                {
                    Console.WriteLine($"Field: {error.Key}, Error: {error.Value}");
                }
                return;
            }

            // Create and validate Adressbuch model
            var adressbuch = new Adressbuch
            {
                Strasse = Strasse ?? string.Empty,
                Hausnummer = Hausnummer,
                Plz = Plz ?? string.Empty,
                Stadt = Stadt ?? string.Empty,
                Land = Land ?? string.Empty,
                Adresszusatz = Adresszusatz,
                Typ = Typ
            };

            Console.WriteLine("Adressbuch model created.");

            // Save address first
            int adressbuchId = await _mitarbeiterService.SaveAdressbuchAsync(adressbuch);
            Console.WriteLine("Adressbuch saved with ID: " + adressbuchId);

            // Create a new Mitarbeiter object
            var mitarbeiter = new Mitarbeiter
            {
                Vorname = Vorname,
                Nachname = Nachname,
                Geburtsdatum = GeburtsdatumAsDateTime,
                Eintrittsdatum = EintrittsdatumAsDateTime,
                Telefon = Telefon,
                Email = Email,
                Aktiv = Aktiv,
                Gehalt = GehaltAsDecimal,
                IstAdmin = IstAdmin,
                BildUrl = BildUrl,
                Notizen = Notizen,
                AdressbuchId = adressbuchId
            };

            Console.WriteLine("Mitarbeiter model created.");

            // If a position is selected, fetch the PositionId and assign it to the Mitarbeiter
            if (!string.IsNullOrEmpty(SelectedPosition))
            {
                var positionId = await _mitarbeiterService.GetPositionIdByTitleAsync(SelectedPosition);
                if (positionId.HasValue)
                {
                    mitarbeiter.PositionId = positionId.Value;
                }
                else
                {
                    PositionError = "Ungültige Position ausgewählt.";
                    return;
                }
            }

            // Save Mitarbeiter
            await _mitarbeiterService.AddMitarbeiterAsync(mitarbeiter);
            Console.WriteLine("Mitarbeiter successfully saved.");

            // Optionally, navigate back or show success message
            Console.WriteLine("SaveMitarbeiterAsync completed successfully.");
            ShowSuccessMessage();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Speichern des Mitarbeiters: {ex.Message}");
        }
    }    [RelayCommand]
    public void ToMitarbeiterverwaltungView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new MitarbeiterverwaltungViewModel();
        }
    }

    public IRelayCommand SaveMitarbeiterAsyncCommand { get; private set; }

    public DateTime? GeburtsdatumAsDateTime =>
        DateTime.TryParseExact(Geburtsdatum, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var date) ? date : null;

    public DateTime? EintrittsdatumAsDateTime =>
        DateTime.TryParseExact(Eintrittsdatum, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var date) ? date : null;

    private decimal? GehaltAsDecimal => decimal.TryParse(Gehalt, out var value) ? value : null;

    private List<string> positionTitles = new List<string>();
    public List<string> PositionTitles
    {
        get => positionTitles;
        set => SetProperty(ref positionTitles, value);
    }

    private string? selectedPosition;
    public string? SelectedPosition
    {
        get => selectedPosition;
        set => SetProperty(ref selectedPosition, value);
    }

    private string? positionError;
    public string? PositionError
    {
        get => positionError;
        set => SetProperty(ref positionError, value);
    }

    private readonly List<string> typOptions = new() { "privat", "geschäftlich" };
    public List<string> TypOptions => typOptions;

    public async Task LoadPositionTitlesAsync()
    {
        var titles = await _mitarbeiterService.FetchPositionTitlesAsync();
        PositionTitles = titles;
    }

    public void ShowSuccessMessage()
    {
        IsInputFormVisible = false;
        IsSuccessMessageVisible = true;
    }

    [ObservableProperty]
    private bool isInputFormVisible = true;

    [ObservableProperty]
    private bool isSuccessMessageVisible = false;
}