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
        // Check if all editable fields are empty
        if (string.IsNullOrWhiteSpace(EditableVorname) &&
            string.IsNullOrWhiteSpace(EditableNachname) &&
            string.IsNullOrWhiteSpace(EditableGeburtsdatum) &&
            string.IsNullOrWhiteSpace(EditableEintrittsdatum) &&
            string.IsNullOrWhiteSpace(EditableTelefon) &&
            string.IsNullOrWhiteSpace(EditableEmail) &&
            string.IsNullOrWhiteSpace(EditableStrasse) &&
            string.IsNullOrWhiteSpace(EditableHausnummer) &&
            string.IsNullOrWhiteSpace(EditablePlz) &&
            string.IsNullOrWhiteSpace(EditableStadt) &&
            string.IsNullOrWhiteSpace(EditableLand) &&
            string.IsNullOrWhiteSpace(EditableAdresszusatz) &&
            string.IsNullOrWhiteSpace(EditableTyp) &&
            string.IsNullOrWhiteSpace(EditableGehalt))
        {
            Console.WriteLine("Keine Änderungen vorgenommen. Zurück zur Mitarbeiteransicht.");
            ToMitarbeiterView();
            return;
        }

        ValidateFields();

        // Check for any error messages
        if (!string.IsNullOrEmpty(VornameError) || !string.IsNullOrEmpty(NachnameError) ||
            !string.IsNullOrEmpty(StrasseError) || !string.IsNullOrEmpty(HausnummerError) ||
            !string.IsNullOrEmpty(PlzError) || !string.IsNullOrEmpty(StadtError) ||
            !string.IsNullOrEmpty(LandError) || !string.IsNullOrEmpty(TelefonError) ||
            !string.IsNullOrEmpty(EmailError) || !string.IsNullOrEmpty(GeburtsdatumError) ||
            !string.IsNullOrEmpty(EintrittsdatumError))
        {
            Console.WriteLine("Es gibt Fehler in den Eingabefeldern. Bitte korrigieren Sie diese.");
            return;
        }

        Console.WriteLine("Alle Felder sind gültig. Speichere Mitarbeiter...");

        // Logic to save changes to the database or service
        await Task.CompletedTask;
    }

    [RelayCommand]
    public void Cancel()
    {
        Console.WriteLine("Edit canceled.");
        // Logic to navigate back or discard changes
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

    private void ValidateFields()
    {
        VornameError = string.IsNullOrWhiteSpace(EditableVorname) ? null : (!InputValidation.IsValidName(EditableVorname) ? "Vorname muss aus mehr als einem Buchstaben bestehen und nur Buchstaben enthalten." : null);
        NachnameError = string.IsNullOrWhiteSpace(EditableNachname) ? null : (!InputValidation.IsValidName(EditableNachname) ? "Nachname muss aus mehr als einem Buchstaben bestehen und nur Buchstaben enthalten." : null);
        GeburtsdatumError = string.IsNullOrWhiteSpace(EditableGeburtsdatum) ? null : (!DateTime.TryParseExact(EditableGeburtsdatum, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _) ? "Geburtsdatum muss im Format TT.MM.JJJJ sein." : null);
        EintrittsdatumError = string.IsNullOrWhiteSpace(EditableEintrittsdatum) ? null : (!DateTime.TryParseExact(EditableEintrittsdatum, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _) ? "Eintrittsdatum muss im Format TT.MM.JJJJ sein." : null);
        TelefonError = string.IsNullOrWhiteSpace(EditableTelefon) ? null : (!InputValidation.IsValidPhoneNumber(EditableTelefon) ? "Telefonnummer darf nur Zahlen enthalten." : null);
        EmailError = string.IsNullOrWhiteSpace(EditableEmail) ? null : (!InputValidation.IsValidEmail(EditableEmail) ? "Ungültige Email-Adresse." : null);
        StrasseError = string.IsNullOrWhiteSpace(EditableStrasse) ? null : (!InputValidation.IsValidString(EditableStrasse) ? "Straße darf nur Buchstaben enthalten." : null);
        HausnummerError = string.IsNullOrWhiteSpace(EditableHausnummer) ? null : (!InputValidation.IsValidNumber(EditableHausnummer) ? "Hausnummer muss eine Zahl sein." : null);
        PlzError = string.IsNullOrWhiteSpace(EditablePlz) ? null : (!InputValidation.IsValidPostalCode(EditablePlz) ? "PLZ muss aus maximal 6 Zahlen bestehen." : null);
        StadtError = string.IsNullOrWhiteSpace(EditableStadt) ? null : (!InputValidation.IsValidString(EditableStadt) ? "Stadt darf nur Buchstaben enthalten." : null);
        LandError = string.IsNullOrWhiteSpace(EditableLand) ? null : (!InputValidation.IsValidString(EditableLand) ? "Land darf nur Buchstaben enthalten." : null);
    }

        [RelayCommand]
    public void ToMitarbeiterView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new MitarbeiterViewModel();
        }
    }
}