using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Percuro.Models.MitarbeiterModels;
using Percuro.Services.MitarbeiterServices;

namespace Percuro.ViewModels.EnterpriseViewModels.HR;

public partial class ArbeitszeitViewModel : ViewModelBase
{
    // Basic ViewModel for ArbeitszeitView
    [RelayCommand]
    public void ToHRView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            var zeiterfassungsService = new ZeiterfassungsService();
            zeiterfassungsService.ClearCache(); // Clear the cache before switching views
            mainVm.CurrentViewModel = new HRViewModel();
        }
    }

    private DateTimeOffset? _startDate = DateTimeOffset.Now.AddDays(-7);
    public DateTimeOffset? StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value);
    }

    private DateTimeOffset? _endDate = DateTimeOffset.Now;
    public DateTimeOffset? EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
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

    private string? _ausgewaehlterMitarbeiter;
    public string? AusgewaehlterMitarbeiter
    {
        get => _ausgewaehlterMitarbeiter;
        set => SetProperty(ref _ausgewaehlterMitarbeiter, value);
    }

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
            UpdateEmployeeSelectionVisibility();
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
            UpdateEmployeeSelectionVisibility();
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
            UpdateEmployeeSelectionVisibility();
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
            UpdateEmployeeSelectionVisibility();
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
            UpdateEmployeeSelectionVisibility();
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
            UpdateEmployeeSelectionVisibility();
        }
    }

    private bool _dateInputCompleted;
    public bool DateInputCompleted
    {
        get => _dateInputCompleted;
        set
        {
            if (SetProperty(ref _dateInputCompleted, value) && value)
            {
                FetchMitarbeiterNamen();
            }
        }
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

    private void UpdateEmployeeSelectionVisibility()
    {
        DateInputCompleted = SelectedStartDay != null && SelectedStartMonth != null && SelectedStartYear != null &&
                             SelectedEndDay != null && SelectedEndMonth != null && SelectedEndYear != null;
    }

    private ObservableCollection<string> _mitarbeiterNamen = new ObservableCollection<string>();
    public ObservableCollection<string> MitarbeiterNamen
    {
        get => _mitarbeiterNamen;
        set
        {
            SetProperty(ref _mitarbeiterNamen, value);
            _alleMitarbeiterNamen = value.ToList(); // Keep the original list updated
        }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private async void FetchMitarbeiterNamen()
    {
        if (DateInputCompleted)
        {
            IsLoading = true;
            var zeiterfassungsService = new ZeiterfassungsService();
            await zeiterfassungsService.GetMitarbeiterZeitkontoImZeitraumAsync(StartDate?.DateTime, EndDate?.DateTime);
            var namen = zeiterfassungsService.GetMitarbeiterNamenImZeitraum();
            MitarbeiterNamen = new ObservableCollection<string>(namen);
            IsLoading = false;

            // Debugging: Log the fetched names
            Console.WriteLine("Fetched Mitarbeiter Namen:");
            foreach (var name in namen)
            {
                Console.WriteLine(name);
            }
        }
    }

    private string _lastSearchInput = string.Empty;

    private void FilterAndSelectMitarbeiter()
    {
        if (string.IsNullOrWhiteSpace(MitarbeiterSuche))
        {
            // Clear the selection if the search input is empty
            AusgewaehlterMitarbeiter = null;
            return;
        }

        // Normalize the search input by replacing 'ø' with 'o'
        var normalizedSearch = MitarbeiterSuche.Replace("ø", "o", StringComparison.OrdinalIgnoreCase);

        // Find the best-matching name by normalizing names in the list
        var bestMatch = _alleMitarbeiterNamen
            .FirstOrDefault(name => name.Replace("ø", "o", StringComparison.OrdinalIgnoreCase)
                                        .Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));

        // Select the best match if available
        AusgewaehlterMitarbeiter = bestMatch;
    }
}