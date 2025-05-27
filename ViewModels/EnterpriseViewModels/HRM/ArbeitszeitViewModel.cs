using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Percuro.Models.MitarbeiterModels;
using Percuro.Services.MitarbeiterServices;
using Percuro.ViewModels.ControlsViewModels;
using Percuro.ViewModels.EnterpriseViewModels.HRM;

namespace Percuro.ViewModels.EnterpriseViewModels.HRM;

public partial class ArbeitszeitViewModel : ViewModelBase
{
    // Basic ViewModel for ArbeitszeitView
    [RelayCommand]
    public void ToHRView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            var zeiterfassungsungsService = new ZeiterfassungsService();
            zeiterfassungsungsService.ClearCache(); // Clear the cache before switching views
            mainVm.CurrentViewModel = new HRMViewModel();
        }
    }

    private string _timeSpan = string.Empty;
    public string TimeSpan
    {
        get => _timeSpan;
        set => SetProperty(ref _timeSpan, value);
    }

    private DateTimeOffset? _startDate;
    public DateTimeOffset? StartDate
    {
        get => _startDate;
        set
        {
            if (SetProperty(ref _startDate, value))
            {
                UpdateTimeSpan();
            }
        }
    }

    private DateTimeOffset? _endDate;
    public DateTimeOffset? EndDate
    {
        get => _endDate;
        set
        {
            if (SetProperty(ref _endDate, value))
            {
                UpdateTimeSpan();
            }
        }
    }

    private ObservableCollection<Mitarbeiter> _alleMitarbeiter = new();
    public ObservableCollection<Mitarbeiter> GefilterteMitarbeiter { get; } = new();

    private string _mitarbeiterSuche = string.Empty;
    public string MitarbeiterSuche
    {
        get => _mitarbeiterSuche;
        set
        {
            if (SetProperty(ref _mitarbeiterSuche, value))
            {
                FilterAndSelectMitarbeiter();
            }
        }
    }

    private List<string> _alleMitarbeiterNamen = new();

    public async Task LadeMitarbeiterAsync()
    {
        var service = new ZeiterfassungsDatabaseService();
        var mitarbeiter = await service.LadeMitarbeiterMitZeitkontoAsync();
        _alleMitarbeiter = new ObservableCollection<Mitarbeiter>(mitarbeiter);
        FilterMitarbeiter();
    }

    private void FilterMitarbeiter()
    {
        GefilterteMitarbeiter.Clear();
        var query = MitarbeiterSuche?.ToLower() ?? string.Empty;
        var gefiltert = _alleMitarbeiter.Where(m =>
            (m.Vorname + " " + m.Nachname).ToLower().Contains(query)
        );
        foreach (var m in gefiltert)
            GefilterteMitarbeiter.Add(m);
    }

    private ObservableCollection<int> _jahre = new ObservableCollection<int>(Enumerable.Range(DateTime.Now.Year - 10, 11));
    public ObservableCollection<int> Jahre => _jahre;

    private ObservableCollection<int>? _startTage;
    public ObservableCollection<int> StartTage
    {
        get
        {
            if (_startTage == null)
            {
                _startTage = new ObservableCollection<int>(Enumerable.Range(1, 31));
            }
            return _startTage;
        }
    }

    private ObservableCollection<int>? _endTage;
    public ObservableCollection<int> EndTage
    {
        get
        {
            if (_endTage == null || DateTime.Now.Day != _endTage.LastOrDefault())
            {
                _endTage = new ObservableCollection<int>(Enumerable.Range(1, DateTime.Now.Day));
            }
            return _endTage;
        }
    }

    private ObservableCollection<string>? _startMonate;
    public ObservableCollection<string> StartMonate
    {
        get
        {
            if (_startMonate == null)
            {
                _startMonate = new ObservableCollection<string>(new[]
                {
                    "Januar", "Februar", "März", "April", "Mai", "Juni",
                    "Juli", "August", "September", "Oktober", "November", "Dezember"
                });
            }
            return _startMonate;
        }
    }

    private ObservableCollection<string>? _endMonate;
    public ObservableCollection<string> EndMonate
    {
        get
        {
            if (_endMonate == null || DateTime.Now.Month != _endMonate.Count)
            {
                _endMonate = new ObservableCollection<string>(new[]
                {
                    "Januar", "Februar", "März", "April", "Mai", "Juni",
                    "Juli", "August", "September", "Oktober", "November", "Dezember"
                }.Take(DateTime.Now.Month).ToArray());
            }
            return _endMonate;
        }
    }

    private int? _selectedStartDay;
    public int? SelectedStartDay
    {
        get => _selectedStartDay;
        set
        {
            SetProperty(ref _selectedStartDay, value);
            UpdateStartDate();
        }
    }

    private string _selectedStartMonth = DateTime.Now.ToString("MMMM");
    public string SelectedStartMonth
    {
        get => _selectedStartMonth;
        set
        {
            SetProperty(ref _selectedStartMonth, value);
            UpdateStartDate();
        }
    }

    private int? _selectedStartYear;
    public int? SelectedStartYear
    {
        get => _selectedStartYear;
        set
        {
            SetProperty(ref _selectedStartYear, value);
            UpdateStartDate();
        }
    }

    private int? _selectedEndDay;
    public int? SelectedEndDay
    {
        get => _selectedEndDay;
        set
        {
            SetProperty(ref _selectedEndDay, value);
            UpdateEndDate();
        }
    }

    private string _selectedEndMonth = DateTime.Now.ToString("MMMM");
    public string SelectedEndMonth
    {
        get => _selectedEndMonth;
        set
        {
            SetProperty(ref _selectedEndMonth, value);
            UpdateEndDate();
        }
    }

    private int? _selectedEndYear;
    public int? SelectedEndYear
    {
        get => _selectedEndYear;
        set
        {
            SetProperty(ref _selectedEndYear, value);
            UpdateEndDate();
        }
    }

    private bool _dateInputCompleted;
    public bool DateInputCompleted
    {
        get => _dateInputCompleted;
        set => SetProperty(ref _dateInputCompleted, value);
    }

    private void UpdateStartDate()
    {
        try
        {
            if (SelectedStartYear.HasValue && SelectedStartDay.HasValue && !string.IsNullOrEmpty(SelectedStartMonth))
            {
                StartDate = new DateTimeOffset(new DateTime(SelectedStartYear.Value, Array.IndexOf(StartMonate.ToArray(), SelectedStartMonth) + 1, SelectedStartDay.Value));
            }
        }
        catch
        {
            // Ignoriere ungültige Datumswerte (z. B. 30. Februar)
        }
    }

    private void UpdateEndDate()
    {
        try
        {
            if (SelectedEndYear.HasValue && SelectedEndDay.HasValue && !string.IsNullOrEmpty(SelectedEndMonth))
            {
                EndDate = new DateTimeOffset(new DateTime(SelectedEndYear.Value, Array.IndexOf(EndMonate.ToArray(), SelectedEndMonth) + 1, SelectedEndDay.Value));
            }
        }
        catch
        {
            // Ignoriere ungültige Datumswerte (z. B. 30. Februar)
        }
    }

    private void UpdateTimeSpan()
    {
        if (StartDate.HasValue && EndDate.HasValue)
        {
            TimeSpan = $"{StartDate.Value:dd.MM.yyyy HH:mm} Uhr - {EndDate.Value:dd.MM.yyyy HH:mm} Uhr";
        }
        else
        {
            TimeSpan = string.Empty;
        }
    }

    private ObservableCollection<MitarbeiterNameWithId> _mitarbeiterNamenWithIds = new ObservableCollection<MitarbeiterNameWithId>();
    public ObservableCollection<MitarbeiterNameWithId> MitarbeiterNamenWithIds
    {
        get => _mitarbeiterNamenWithIds;
        set => SetProperty(ref _mitarbeiterNamenWithIds, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private readonly ZeiterfassungsService _zeiterfassungsService = new ZeiterfassungsService();

    [ObservableProperty]
    private CustomCalendarViewModel _myCalendarViewModel;

    [ObservableProperty]
    private bool _isTagessatzModus = true; // Default to Tagessatz

    [ObservableProperty]
    private bool _isWochenberichtModus = false;

    private bool _isAlleEintraegeModus = false;
    public bool IsAlleEintraegeModus
    {
        get => _isAlleEintraegeModus;
        set => SetProperty(ref _isAlleEintraegeModus, value);
    }

    [ObservableProperty]
    private ArbeitszeitNavigatorSectionViewModel arbeitszeitNavigatorSectionViewModel = new ArbeitszeitNavigatorSectionViewModel();

    public ArbeitszeitViewModel()
    {
        arbeitszeitNavigatorSectionViewModel = new ArbeitszeitNavigatorSectionViewModel();
        _zeiterfassungsService = new ZeiterfassungsService();
        MyCalendarViewModel = new CustomCalendarViewModel();
        MyCalendarViewModel.CurrentSelectionMode = CalendarSelectionMode.Day;
        SelectedStartYear = DateTime.Today.Year;
        SelectedStartMonth = DateTime.Today.ToString("MMMM");
        SelectedStartDay = 1;
        SelectedEndYear = DateTime.Today.Year;
        SelectedEndMonth = DateTime.Today.ToString("MMMM");
        SelectedEndDay = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
        UpdateStartDate();
        UpdateEndDate();
        // PropertyChanged-Subscription für SelectedMitarbeiterWithId im Child-ViewModel
        arbeitszeitNavigatorSectionViewModel.PropertyChanged += ArbeitszeitNavigatorSectionViewModel_PropertyChanged;
    }

    // Property-Weiterleitung für die aktuelle Auswahl
    // public MitarbeiterNameWithId? SelectedMitarbeiterWithId => ArbeitszeitNavigatorSectionViewModel.SelectedMitarbeiterWithId;

    private void ArbeitszeitNavigatorSectionViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ArbeitszeitNavigatorSectionViewModel.SelectedMitarbeiterWithId))
        {
            // Sichtbarkeit des Buttons steuern
            IsZeigeEintraegeButtonVisible = ArbeitszeitNavigatorSectionViewModel.SelectedMitarbeiterWithId != null;
            OnPropertyChanged(nameof(ArbeitszeitNavigatorSectionViewModel.SelectedMitarbeiterWithId));
        }
        else if (e.PropertyName == nameof(ArbeitszeitNavigatorSectionViewModel.SelectedStartDate))
        {
            SelectedStartDate = ArbeitszeitNavigatorSectionViewModel.SelectedStartDate;
            OnPropertyChanged(nameof(SelectedStartDate));
        }
        else if (e.PropertyName == nameof(ArbeitszeitNavigatorSectionViewModel.SelectedEndDate))
        {
            SelectedEndDate = ArbeitszeitNavigatorSectionViewModel.SelectedEndDate;
            OnPropertyChanged(nameof(SelectedEndDate));
        }
    }

    private async Task FetchMitarbeiterNamenAsync()
    {
        IsLoading = true;
        MitarbeiterNamenWithIds.Clear();

        await _zeiterfassungsService.GetMitarbeiterZeitkontoImZeitraumAsync(StartDate?.DateTime, EndDate?.DateTime);
        var eintraege = _zeiterfassungsService.GetZeitkontoEntries();
        var mitarbeiterGruppiert = eintraege
            .GroupBy(e => new { e.MitarbeiterId, e.MitarbeiterVorname, e.MitarbeiterNachname })
            .Select(g => new MitarbeiterNameWithId($"{g.Key.MitarbeiterId:D2} {g.Key.MitarbeiterVorname[0]}. {g.Key.MitarbeiterNachname}", g.Key.MitarbeiterId))
            .OrderBy(m => m.Name)
            .ToList();
        foreach (var m in mitarbeiterGruppiert)
        {
            MitarbeiterNamenWithIds.Add(m);
        }
        IsLoading = false;
    }

    private void FilterAndSelectMitarbeiter()
    {
        if (string.IsNullOrWhiteSpace(MitarbeiterSuche))
        {
            // If search text is cleared, current selection remains.
            // If you'd prefer to clear the selection, you could set:
            // if (SelectedMitarbeiterWithId != null) SelectedMitarbeiterWithId = null;
            return;
        }

        // Find the first item in MitarbeiterNamenWithIds whose Name contains the search text.
        // This assumes MitarbeiterNameWithId has a public string property named 'Name'.
        var bestMatch = MitarbeiterNamenWithIds
            .FirstOrDefault(m => m.Name != null &&
                                   m.Name.IndexOf(MitarbeiterSuche, StringComparison.OrdinalIgnoreCase) >= 0);

        if (bestMatch != null)
        {
            // Update SelectedMitarbeiterWithId only if it's a new selection.
            // if (SelectedMitarbeiterWithId != bestMatch)
            // {
            //     SelectedMitarbeiterWithId = bestMatch;
            // }
        }
        else
        {
            // No match found. Current selection remains.
            // If you'd prefer to clear selection when no match is found:
            // if (SelectedMitarbeiterWithId != null) SelectedMitarbeiterWithId = null;
        }
    }

    private ObservableCollection<ZeitkontoModel> _zeitkontoEntries = new ObservableCollection<ZeitkontoModel>();
    public ObservableCollection<ZeitkontoModel> ZeitkontoEntries
    {
        get => _zeitkontoEntries;
        set => SetProperty(ref _zeitkontoEntries, value);
    }

    private bool _isCustomCalendarVisible;
    public bool IsCustomCalendarVisible
    {
        get => _isCustomCalendarVisible;
        set => SetProperty(ref _isCustomCalendarVisible, value);
    }

    private string? _displayedMitarbeiterName;
    public string? DisplayedMitarbeiterName
    {
        get => _displayedMitarbeiterName;
        set => SetProperty(ref _displayedMitarbeiterName, value);
    }

    private bool _isAllEntriesVisible;
    public bool IsAllEntriesVisible
    {
        get => _isAllEntriesVisible;
        set => SetProperty(ref _isAllEntriesVisible, value);
    }

    private int _numberOfEntriesFound;
    public int NumberOfEntriesFound
    {
        get => _numberOfEntriesFound;
        set => SetProperty(ref _numberOfEntriesFound, value);
    }

    private Dictionary<int, int> _entriesPerMitarbeiter = new();
    public Dictionary<int, int> EntriesPerMitarbeiter
    {
        get => _entriesPerMitarbeiter;
        set => SetProperty(ref _entriesPerMitarbeiter, value);
    }

    private string _entriesPerMitarbeiterDisplay = string.Empty;
    public string EntriesPerMitarbeiterDisplay
    {
        get => _entriesPerMitarbeiterDisplay;
        private set => SetProperty(ref _entriesPerMitarbeiterDisplay, value);
    }

    private void UpdateEntriesPerMitarbeiterDisplay()
    {
        // Only show the currently selected Mitarbeiter, not all
        if (ArbeitszeitNavigatorSectionViewModel.SelectedMitarbeiterWithId != null && EntriesPerMitarbeiter.TryGetValue(ArbeitszeitNavigatorSectionViewModel.SelectedMitarbeiterWithId.Id, out var count))
        {
            EntriesPerMitarbeiterDisplay = $"{count} Einträge gefunden";
        }
        else
        {
            EntriesPerMitarbeiterDisplay = string.Empty;
        }
        Console.WriteLine($"Updated EntriesPerMitarbeiterDisplay: {EntriesPerMitarbeiterDisplay}");
    }

    [RelayCommand]
    public async Task ZeigeEintraegeAsync()
    {
        var selectedMitarbeiter = ArbeitszeitNavigatorSectionViewModel.SelectedMitarbeiterWithId;
        var startDate = ArbeitszeitNavigatorSectionViewModel.SelectedStartDate;
        var endDate = ArbeitszeitNavigatorSectionViewModel.SelectedEndDate;

        Console.WriteLine($"[ZeigeEintraegeAsync] SelectedMitarbeiterWithId: {selectedMitarbeiter?.Name} (Id: {selectedMitarbeiter?.Id})");
        Console.WriteLine($"[ZeigeEintraegeAsync] Zeitraum: {startDate} bis {endDate}");

        if (selectedMitarbeiter != null && startDate != null && endDate != null)
        {
            await _zeiterfassungsService.GetMitarbeiterZeitkontoImZeitraumAsync(startDate.Value, endDate.Value);
            var eintraege = _zeiterfassungsService.GetAllEntriesForDataGrid(selectedMitarbeiter.Id);
            Console.WriteLine($"[ZeigeEintraegeAsync] GetAllEntriesForDataGrid returned {eintraege.Count} entries for Id {selectedMitarbeiter.Id}");
            ZeitkontoEntries = new ObservableCollection<ZeitkontoModel>(eintraege);
            IsAllEntriesVisible = eintraege.Count > 0;
        }
        else
        {
            ZeitkontoEntries.Clear();
            IsAllEntriesVisible = false;
        }
        Console.WriteLine($"[ZeigeEintraegeAsync] ZeitkontoEntries.Count: {ZeitkontoEntries.Count}");
    }

    // CommunityToolkit.Mvvm generiert automatisch eine Property ZeigeEintraegeAsyncCommand für das [RelayCommand] public async Task ZeigeEintraegeAsync().
    // Falls das Binding im XAML nicht funktioniert, kann man die Property explizit deklarieren:
    public IAsyncRelayCommand ZeigeEintraegeAsyncCommand => new AsyncRelayCommand(ZeigeEintraegeAsync);

    [RelayCommand]
    private void ToggleTagessatzModus()
    {
        IsTagessatzModus = true;
        IsWochenberichtModus = false;
        IsAlleEintraegeModus = false;
        MyCalendarViewModel.CurrentSelectionMode = CalendarSelectionMode.Day;
    }

    [RelayCommand]
    private void ToggleWochenberichtModus()
    {
        IsWochenberichtModus = true;
        IsTagessatzModus = false;
        IsAlleEintraegeModus = false;
        MyCalendarViewModel.CurrentSelectionMode = CalendarSelectionMode.Week;
    }

    [RelayCommand]
    public async Task CheckDateInputCommand()
    {
        // Validate all fields before using them
        if (!SelectedStartDay.HasValue || string.IsNullOrEmpty(SelectedStartMonth) || !SelectedStartYear.HasValue ||
            !SelectedEndDay.HasValue || string.IsNullOrEmpty(SelectedEndMonth) || !SelectedEndYear.HasValue)
        {
            Console.WriteLine("Bitte füllen Sie alle Felder aus.");
            DateInputCompleted = false;
            return;
        }

        try
        {
            var startDate = new DateTime(SelectedStartYear.Value, Array.IndexOf(StartMonate.ToArray(), SelectedStartMonth) + 1, SelectedStartDay.Value);
            var endDate = new DateTime(SelectedEndYear.Value, Array.IndexOf(EndMonate.ToArray(), SelectedEndMonth) + 1, SelectedEndDay.Value);

            if (startDate > endDate)
            {
                Console.WriteLine("Das Startdatum darf nicht nach dem Enddatum liegen.");
                DateInputCompleted = false;
                return;
            }

            Console.WriteLine("Eingaben sind gültig. Mitarbeiterdaten werden geladen...");
            DateInputCompleted = true;

            // Fetch MitarbeiterNamen and update cache
            await FetchMitarbeiterNamenAsync();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"Ungültiges Datum: {ex.Message}");
            DateInputCompleted = false;
        }
    }

    private void ClearCalendarHighlights()
    {
        if (MyCalendarViewModel != null)
        {
            foreach (var dayVM in MyCalendarViewModel.CalendarDays)
            {
                dayVM.HasEntry = false; // Or any other property used for highlighting
            }
        }
    }

    private void UpdateCalendarDayEntries()
    {
        if (ArbeitszeitNavigatorSectionViewModel.SelectedMitarbeiterWithId == null || MyCalendarViewModel == null)
        {
            if (MyCalendarViewModel != null)
            {
                foreach (var dayVM in MyCalendarViewModel.CalendarDays)
                {
                    dayVM.HasEntry = false;
                }
            }
            return;
        }

        var mitarbeiterId = ArbeitszeitNavigatorSectionViewModel.SelectedMitarbeiterWithId.Id;
        var allEntries = _zeiterfassungsService.GetZeitkontoEntries(); 

        var employeeEntriesForMonth = allEntries
            .Where(e => e.MitarbeiterId == mitarbeiterId &&
                        e.CheckDateTime.Year == MyCalendarViewModel.CurrentDisplayMonth.Year &&
                        e.CheckDateTime.Month == MyCalendarViewModel.CurrentDisplayMonth.Month)
            .Select(e => e.CheckDateTime.Date)
            .Distinct()
            .ToHashSet();

        foreach (var dayVM in MyCalendarViewModel.CalendarDays)
        {
            if (dayVM.IsCurrentMonth)
            {
                dayVM.HasEntry = employeeEntriesForMonth.Contains(dayVM.Date.Date);
            }
            else
            {
                dayVM.HasEntry = false;
            }
        }
    }

    private bool _isZeigeEintraegeButtonVisible;
    public bool IsZeigeEintraegeButtonVisible
    {
        get => _isZeigeEintraegeButtonVisible;
        set => SetProperty(ref _isZeigeEintraegeButtonVisible, value);
    }

    private DateTime? _selectedStartDate;
    public DateTime? SelectedStartDate
    {
        get => _selectedStartDate;
        set => SetProperty(ref _selectedStartDate, value);
    }

    private DateTime? _selectedEndDate;
    public DateTime? SelectedEndDate
    {
        get => _selectedEndDate;
        set => SetProperty(ref _selectedEndDate, value);
    }
}