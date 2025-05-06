using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Percuro.Models.HRModels;
using Percuro.Models.MitarbeiterModels;
using Percuro.Services;
using Percuro.Services.MitarbeiterServices;

namespace Percuro.ViewModels.EnterpriseViewModels.HR.MitarbeiterNS;

public partial class EditMitarbeiterViewModel : ViewModelBase
{
    public Mitarbeiter Mitarbeiter { get; }

    public IRelayCommand SaveAsyncCommand { get; }

    public EditMitarbeiterViewModel(Mitarbeiter mitarbeiter)
    {
        Mitarbeiter = mitarbeiter;
        SaveAsyncCommand = new AsyncRelayCommand(SaveAsync);
        _ = LoadAdressbuchAsync(mitarbeiter.Id);
        _ = LoadPositionTitlesAsync();
    }

    public List<string> TypOptions { get; } = new() { "privat", "geschäftlich" };
    public ObservableCollection<string> PositionTitles { get; } = new();

    private readonly MitarbeiterDatabaseService _databaseService = new();

    public async Task LoadPositionTitlesAsync()
    {
        var titles = await _databaseService.FetchPositionTitlesAsync();
        PositionTitles.Clear();
        foreach (var title in titles)
        {
            PositionTitles.Add(title);
        }
    }

    public Adressbuch Adressbuch { get; private set; } = new();

    public async Task LoadAdressbuchAsync(int mitarbeiterId)
    {
        var adressbuch = await _databaseService.GetAdressbuchByMitarbeiterIdAsync(mitarbeiterId);
        if (adressbuch != null)
        {
            Adressbuch = adressbuch;
            OnPropertyChanged(nameof(Adressbuch));
        }
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        bool hasMitarbeiterChanges = false;
        bool hasAdressbuchChanges = false;

        try
        {
            // Validate and check for Mitarbeiter changes
            if (!string.IsNullOrWhiteSpace(EditableVorname))
            {
                if (!InputValidation.IsValidName(EditableVorname))
                {
                    VornameError = "Vorname muss aus mehr als einem Buchstaben bestehen und nur Buchstaben enthalten.";
                    UpdateError = true;
                    return;
                }
                Mitarbeiter.Vorname = EditableVorname;
                hasMitarbeiterChanges = true;
            }
            if (!string.IsNullOrWhiteSpace(EditableNachname))
            {
                if (!InputValidation.IsValidName(EditableNachname))
                {
                    NachnameError = "Nachname muss aus mehr als einem Buchstaben bestehen und nur Buchstaben enthalten.";
                    UpdateError = true;
                    return;
                }
                Mitarbeiter.Nachname = EditableNachname;
                hasMitarbeiterChanges = true;
            }
            if (!string.IsNullOrWhiteSpace(EditableGeburtsdatum))
            {
                if (!DateTime.TryParseExact(EditableGeburtsdatum, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var geburtsdatum))
                {
                    GeburtsdatumError = "Geburtsdatum muss im Format TT.MM.JJJJ sein.";
                    UpdateError = true;
                    return;
                }
                Mitarbeiter.Geburtsdatum = geburtsdatum;
                hasMitarbeiterChanges = true;
            }
            if (!string.IsNullOrWhiteSpace(EditableEintrittsdatum))
            {
                if (!DateTime.TryParseExact(EditableEintrittsdatum, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var eintrittsdatum))
                {
                    EintrittsdatumError = "Eintrittsdatum muss im Format TT.MM.JJJJ sein.";
                    UpdateError = true;
                    return;
                }
                Mitarbeiter.Eintrittsdatum = eintrittsdatum;
                hasMitarbeiterChanges = true;
            }
            if (!string.IsNullOrWhiteSpace(EditableTelefon))
            {
                if (!InputValidation.IsValidPhoneNumber(EditableTelefon))
                {
                    TelefonError = "Telefonnummer darf nur Zahlen enthalten.";
                    UpdateError = true;
                    return;
                }
                Mitarbeiter.Telefon = EditableTelefon;
                hasMitarbeiterChanges = true;
            }
            if (!string.IsNullOrWhiteSpace(EditableEmail))
            {
                if (!InputValidation.IsValidEmail(EditableEmail))
                {
                    EmailError = "Ungültige Email-Adresse.";
                    UpdateError = true;
                    return;
                }
                Mitarbeiter.Email = EditableEmail;
                hasMitarbeiterChanges = true;
            }

            // Validate and check for Adressbuch changes
            if (!string.IsNullOrWhiteSpace(EditableStrasse))
            {
                if (!InputValidation.IsValidString(EditableStrasse))
                {
                    StrasseError = "Straße darf nur Buchstaben enthalten.";
                    UpdateError = true;
                    return;
                }
                Adressbuch.Strasse = EditableStrasse;
                hasAdressbuchChanges = true;
            }
            if (!string.IsNullOrWhiteSpace(EditableHausnummer))
            {
                if (!InputValidation.IsValidNumber(EditableHausnummer))
                {
                    HausnummerError = "Hausnummer muss eine Zahl sein.";
                    UpdateError = true;
                    return;
                }
                Adressbuch.Hausnummer = EditableHausnummer;
                hasAdressbuchChanges = true;
            }
            if (!string.IsNullOrWhiteSpace(EditablePlz))
            {
                if (!InputValidation.IsValidPostalCode(EditablePlz))
                {
                    PlzError = "PLZ muss aus maximal 6 Zahlen bestehen.";
                    UpdateError = true;
                    return;
                }
                Adressbuch.Plz = EditablePlz;
                hasAdressbuchChanges = true;
            }
            if (!string.IsNullOrWhiteSpace(EditableStadt))
            {
                if (!InputValidation.IsValidString(EditableStadt))
                {
                    StadtError = "Stadt darf nur Buchstaben enthalten.";
                    UpdateError = true;
                    return;
                }
                Adressbuch.Stadt = EditableStadt;
                hasAdressbuchChanges = true;
            }
            if (!string.IsNullOrWhiteSpace(EditableLand))
            {
                if (!InputValidation.IsValidString(EditableLand))
                {
                    LandError = "Land darf nur Buchstaben enthalten.";
                    UpdateError = true;
                    return;
                }
                Adressbuch.Land = EditableLand;
                hasAdressbuchChanges = true;
            }

            // Check for Position changes
            if (!string.IsNullOrWhiteSpace(SelectedPosition) && SelectedPosition != Mitarbeiter.PositionTitel)
            {
                var positionId = await _databaseService.GetPositionIdByTitleAsync(SelectedPosition);
                if (positionId.HasValue)
                {
                    Mitarbeiter.PositionId = positionId.Value;
                    hasMitarbeiterChanges = true;
                }
            }

            // Save changes to the database
            if (hasMitarbeiterChanges)
            {
                await _databaseService.UpdateMitarbeiterAsync(Mitarbeiter);
            }
            if (hasAdressbuchChanges)
            {
                await _databaseService.UpdateAdressbuchAsync(Adressbuch);
            }

            Console.WriteLine("Änderungen erfolgreich gespeichert.");
            SaveSuccess = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Speichern: {ex.Message}");
            UpdateError = true;
            Console.WriteLine("UpdateError wurde auf true gesetzt.");
        }

        if (UpdateError)
        {
            Console.WriteLine("UpdateError is true. The error message should be visible.");
        }
    }

    [RelayCommand]
    public void Cancel()
    {
ToMitarbeiterView();
    }

    [ObservableProperty]
    private string? vornameError;

    [ObservableProperty]
    private string? nachnameError;

    [ObservableProperty]
    private string? strasseError;

    [ObservableProperty]
    private string? hausnummerError;

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
    private string? geburtsdatumError;

    [ObservableProperty]
    private string? eintrittsdatumError;

    [ObservableProperty]
    private string? editableVorname;

    [ObservableProperty]
    private string? editableNachname;

    [ObservableProperty]
    private string? editableGeburtsdatum;

    [ObservableProperty]
    private string? editableEintrittsdatum;

    [ObservableProperty]
    private string? editableTelefon;

    [ObservableProperty]
    private string? editableEmail;

    [ObservableProperty]
    private string? editableStrasse;

    [ObservableProperty]
    private string? editableHausnummer;

    [ObservableProperty]
    private string? editablePlz;

    [ObservableProperty]
    private string? editableStadt;

    [ObservableProperty]
    private string? editableLand;

    [ObservableProperty]
    private string? editableAdresszusatz;

    [ObservableProperty]
    private string? editableTyp;

    [ObservableProperty]
    private string? editableGehalt;

    [ObservableProperty]
    private string? selectedPosition;

    private bool updateError;
    public bool UpdateError
    {
        get => updateError;
        set
        {
            if (updateError != value)
            {
                updateError = value;
                OnPropertyChanged();
            }
        }
    }

    [ObservableProperty]
    private bool saveSuccess = false;

        [RelayCommand]
    public void ToMitarbeiterView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new MitarbeiterViewModel();
        }
    }
}