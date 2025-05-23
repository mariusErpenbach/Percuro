using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Percuro.Models.MitarbeiterModels;
using Percuro.Services.MitarbeiterServices;

namespace Percuro.ViewModels.EnterpriseViewModels.HR;

public partial class ArbeitszeitNavigatorSectionViewModel : ObservableObject
{
    // Zeitraum-Auswahl
    public ObservableCollection<int> Jahre { get; set; } = new(Enumerable.Range(DateTime.Now.Year - 10, 11));
    public ObservableCollection<int> StartTage { get; set; } = new(Enumerable.Range(1, 31));
    public ObservableCollection<int> EndTage { get; set; } = new(Enumerable.Range(1, 31));
    public ObservableCollection<string> StartMonate { get; set; } = new(new[] { "Januar", "Februar", "März", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober", "November", "Dezember" });
    public ObservableCollection<string> EndMonate { get; set; } = new(new[] { "Januar", "Februar", "März", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober", "November", "Dezember" });
    
    [ObservableProperty] private int? selectedStartDay;
    [ObservableProperty] private string selectedStartMonth = DateTime.Now.ToString("MMMM");
    [ObservableProperty] private int? selectedStartYear = DateTime.Now.Year;
    [ObservableProperty] private int? selectedEndDay;
    [ObservableProperty] private string selectedEndMonth = DateTime.Now.ToString("MMMM");
    [ObservableProperty] private int? selectedEndYear = DateTime.Now.Year;

    // Mitarbeiter-Auswahl
    [ObservableProperty] private string mitarbeiterSuche = string.Empty;
    [ObservableProperty] private ObservableCollection<MitarbeiterNameWithId> mitarbeiterNamenWithIds = new();
    [ObservableProperty] private MitarbeiterNameWithId? selectedMitarbeiterWithId;
    [ObservableProperty] private string entriesPerMitarbeiterDisplay = string.Empty;
    [ObservableProperty] private int numberOfEntriesFound;
    [ObservableProperty] private bool dateInputCompleted;

    private readonly ZeiterfassungsService _zeiterfassungsService = new ZeiterfassungsService();

    [RelayCommand]
    public async Task CheckDateInputAsync()
    {
        if (!SelectedStartDay.HasValue || string.IsNullOrEmpty(SelectedStartMonth) || !SelectedStartYear.HasValue ||
            !SelectedEndDay.HasValue || string.IsNullOrEmpty(SelectedEndMonth) || !SelectedEndYear.HasValue)
        {
            DateInputCompleted = false;
            return;
        }
        try
        {
            var startDate = new DateTime(SelectedStartYear.Value, Array.IndexOf(StartMonate.ToArray(), SelectedStartMonth) + 1, SelectedStartDay.Value);
            var endDate = new DateTime(SelectedEndYear.Value, Array.IndexOf(EndMonate.ToArray(), SelectedEndMonth) + 1, SelectedEndDay.Value);

            if (startDate > endDate)
            {
                DateInputCompleted = false;
                return;
            }

            await _zeiterfassungsService.GetMitarbeiterZeitkontoImZeitraumAsync(startDate, endDate);
            var eintraege = _zeiterfassungsService.GetZeitkontoEntries();
            NumberOfEntriesFound = eintraege.Count;
            var mitarbeiterGruppiert = eintraege
                .GroupBy(e => new { e.MitarbeiterId, e.MitarbeiterVorname, e.MitarbeiterNachname })
                .Select(g => new MitarbeiterNameWithId($"{g.Key.MitarbeiterId:D2} {g.Key.MitarbeiterVorname[0]}. {g.Key.MitarbeiterNachname}", g.Key.MitarbeiterId))
                .OrderBy(m => m.Name)
                .ToList();
            MitarbeiterNamenWithIds = new ObservableCollection<MitarbeiterNameWithId>(mitarbeiterGruppiert);
            DateInputCompleted = true;
        }
        catch
        {
            DateInputCompleted = false;
        }
    }

    private void UpdateEntriesPerMitarbeiterDisplay()
    {
        if (SelectedMitarbeiterWithId != null)
        {
            var count = _zeiterfassungsService.GetZeitkontoEntries().Count(e => e.MitarbeiterId == SelectedMitarbeiterWithId.Id);
            EntriesPerMitarbeiterDisplay = $"{count} Einträge gefunden";
        }
        else
        {
            EntriesPerMitarbeiterDisplay = string.Empty;
        }
    }

    partial void OnSelectedMitarbeiterWithIdChanged(MitarbeiterNameWithId? value)
    {
        UpdateEntriesPerMitarbeiterDisplay();
    }

    private static string NormalizeName(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return input.Replace('ø', 'o').Replace('Ø', 'O');
    }

    partial void OnMitarbeiterSucheChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;
        var normalizedSearch = NormalizeName(value).Trim();
        if (string.IsNullOrEmpty(normalizedSearch))
            return;

        MitarbeiterNameWithId? bestMatch = null;
        int bestScore = int.MaxValue;

        foreach (var m in MitarbeiterNamenWithIds)
        {
            if (string.IsNullOrEmpty(m.Name)) continue;
            var name = NormalizeName(m.Name);
            int score = 1000;

            if (string.Equals(name, normalizedSearch, StringComparison.OrdinalIgnoreCase))
                score = 0; // Exakter Match
            else if (name.StartsWith(normalizedSearch, StringComparison.OrdinalIgnoreCase))
                score = 1; // Name beginnt mit Suchbegriff
            else if (name.IndexOf(" " + normalizedSearch, StringComparison.OrdinalIgnoreCase) >= 0)
                score = 2; // Wortanfang
            else if (name.IndexOf(normalizedSearch, StringComparison.OrdinalIgnoreCase) >= 0)
                score = 3; // Substring
            else
                continue; // Kein Treffer

            if (score < bestScore || (score == bestScore && string.Compare(name, bestMatch?.Name, StringComparison.OrdinalIgnoreCase) < 0))
            {
                bestScore = score;
                bestMatch = m;
            }
        }

        if (bestMatch != null && SelectedMitarbeiterWithId != bestMatch)
        {
            SelectedMitarbeiterWithId = bestMatch;
        }
    }

    public DateTime? SelectedStartDate
    {
        get
        {
            if (SelectedStartDay.HasValue && !string.IsNullOrEmpty(SelectedStartMonth) && SelectedStartYear.HasValue)
            {
                try
                {
                    return new DateTime(SelectedStartYear.Value, Array.IndexOf(StartMonate.ToArray(), SelectedStartMonth) + 1, SelectedStartDay.Value);
                }
                catch { }
            }
            return null;
        }
    }

    public DateTime? SelectedEndDate
    {
        get
        {
            if (SelectedEndDay.HasValue && !string.IsNullOrEmpty(SelectedEndMonth) && SelectedEndYear.HasValue)
            {
                try
                {
                    return new DateTime(SelectedEndYear.Value, Array.IndexOf(EndMonate.ToArray(), SelectedEndMonth) + 1, SelectedEndDay.Value);
                }
                catch { }
            }
            return null;
        }
    }

    partial void OnSelectedStartDayChanged(int? value)
    {
        OnPropertyChanged(nameof(SelectedStartDate));
    }
    partial void OnSelectedStartMonthChanged(string value)
    {
        OnPropertyChanged(nameof(SelectedStartDate));
    }
    partial void OnSelectedStartYearChanged(int? value)
    {
        OnPropertyChanged(nameof(SelectedStartDate));
    }
    partial void OnSelectedEndDayChanged(int? value)
    {
        OnPropertyChanged(nameof(SelectedEndDate));
    }
    partial void OnSelectedEndMonthChanged(string value)
    {
        OnPropertyChanged(nameof(SelectedEndDate));
    }
    partial void OnSelectedEndYearChanged(int? value)
    {
        OnPropertyChanged(nameof(SelectedEndDate));
    }
}
