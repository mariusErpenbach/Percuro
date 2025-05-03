using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using Percuro.Models.MitarbeiterModels;
using Percuro.Services.MitarbeiterServices;

namespace Percuro.ViewModels.EnterpriseViewModels.HR;

public partial class MitarbeiterViewModel : ViewModelBase
{
    private readonly MitarbeiterDatabaseService _mitarbeiterService;

    public ObservableCollection<Mitarbeiter> MitarbeiterListe { get; set; } = new();

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                FilterMitarbeiterListe();
            }
        }
    }

    public string GetFormattedDate(DateTime? date)
    {
        return date?.ToString("dd.MM.yyyy") ?? string.Empty;
    }

    public ObservableCollection<Mitarbeiter> FilteredMitarbeiterListe { get; set; } = new();

    private void FilterMitarbeiterListe()
    {
        FilteredMitarbeiterListe.Clear();
        var filtered = MitarbeiterListe.Where(m =>
            (m.Vorname?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (m.Nachname?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (m.Email?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));

        foreach (var mitarbeiter in filtered)
        {
            FilteredMitarbeiterListe.Add(new Mitarbeiter
            {
                Id = mitarbeiter.Id,
                Vorname = mitarbeiter.Vorname,
                Nachname = mitarbeiter.Nachname,
                Geburtsdatum = mitarbeiter.Geburtsdatum,
                Eintrittsdatum = mitarbeiter.Eintrittsdatum,
                PositionTitel = mitarbeiter.PositionTitel,
                Telefon = mitarbeiter.Telefon,
                Email = mitarbeiter.Email,
                Aktiv = mitarbeiter.Aktiv,
                Gehalt = mitarbeiter.Gehalt,
                IstAdmin = mitarbeiter.IstAdmin,
                BildUrl = mitarbeiter.BildUrl,
                Notizen = mitarbeiter.Notizen,
             
            });
        }
    }

    public MitarbeiterViewModel()
    {
        _mitarbeiterService = new MitarbeiterDatabaseService();
        LoadMitarbeiterListe();
    }

    private async void LoadMitarbeiterListe()
    {
        Console.WriteLine("Loading MitarbeiterListe...");
        var mitarbeiter = await _mitarbeiterService.GetAllMitarbeiterAsync();
        Console.WriteLine("MitarbeiterListe fetched with count: " + mitarbeiter.Count);
        foreach (var person in mitarbeiter)
        {
            Console.WriteLine("Adding Mitarbeiter to list: " + person.Id);
            MitarbeiterListe.Add(person);
        }

        // Initialize FilteredMitarbeiterListe with all Mitarbeiter
        FilteredMitarbeiterListe = new ObservableCollection<Mitarbeiter>(MitarbeiterListe);
        OnPropertyChanged(nameof(FilteredMitarbeiterListe));
    }

    // Basic ViewModel for MitarbeiterView
    [RelayCommand]
    public void ToHRView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new HRViewModel();
        }
    }
}