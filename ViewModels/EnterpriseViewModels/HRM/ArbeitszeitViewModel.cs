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
using Percuro.ViewModels.EnterpriseViewModels;
using Percuro.ViewModels.EnterpriseViewModels.HRM.Arbeitszeit;

namespace Percuro.ViewModels.EnterpriseViewModels.HRM;

public partial class ArbeitszeitViewModel : ViewModelBase
{
    // Basic ViewModel for ArbeitszeitView
    
    [RelayCommand]
    public void ToHRMView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            var zeiterfassungsungsService = new ZeiterfassungsService();
            zeiterfassungsungsService.ClearCache(); // Clear the cache before switching views
            mainVm.CurrentViewModel = new HRMViewModel();
        }
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
    private ArbeitszeitNavigatorSectionViewModel arbeitszeitNavigatorSectionViewModel = new ArbeitszeitNavigatorSectionViewModel();    public ArbeitszeitViewModel()
    {
        arbeitszeitNavigatorSectionViewModel = new ArbeitszeitNavigatorSectionViewModel();
        _zeiterfassungsService = new ZeiterfassungsService();
        MyCalendarViewModel = new CustomCalendarViewModel();
        MyCalendarViewModel.CurrentSelectionMode = CalendarSelectionMode.Day;
        // PropertyChanged-Subscription für SelectedMitarbeiterWithId im Child-ViewModel
        arbeitszeitNavigatorSectionViewModel.PropertyChanged += ArbeitszeitNavigatorSectionViewModel_PropertyChanged;
    }    // Property-Weiterleitung für die aktuelle Auswahl
    // public MitarbeiterNameWithId? SelectedMitarbeiterWithId => ArbeitszeitNavigatorSectionViewModel.SelectedMitarbeiterWithId;

    private void ArbeitszeitNavigatorSectionViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ArbeitszeitNavigatorSectionViewModel.SelectedMitarbeiterWithId))
        {
            // Sichtbarkeit des Buttons steuern
            IsZeigeEintraegeButtonVisible = ArbeitszeitNavigatorSectionViewModel.SelectedMitarbeiterWithId != null;
            OnPropertyChanged(nameof(ArbeitszeitNavigatorSectionViewModel.SelectedMitarbeiterWithId));
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
    }    [RelayCommand]
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
    }    [RelayCommand]
    private void ToggleWochenberichtModus()
    {
        IsWochenberichtModus = true;
        IsTagessatzModus = false;
        IsAlleEintraegeModus = false;
        MyCalendarViewModel.CurrentSelectionMode = CalendarSelectionMode.Week;
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
    }    private bool _isZeigeEintraegeButtonVisible;
    public bool IsZeigeEintraegeButtonVisible
    {
        get => _isZeigeEintraegeButtonVisible;
        set => SetProperty(ref _isZeigeEintraegeButtonVisible, value);
    }
}