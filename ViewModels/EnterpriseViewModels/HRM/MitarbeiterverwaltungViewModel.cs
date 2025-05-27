using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using Percuro.Models.MitarbeiterModels;
using Percuro.Services.MitarbeiterServices;
using Percuro.ViewModels.EnterpriseViewModels.HRM;
using Percuro.ViewModels.EnterpriseViewModels.HRM.MitarbeiterNS;

namespace Percuro.ViewModels.EnterpriseViewModels.HRM;

public partial class MitarbeiterverwaltungViewModel : ViewModelBase
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
    }    public MitarbeiterverwaltungViewModel()
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

        // Ensure MitarbeiterListe uses the updated Mitarbeiter class
        MitarbeiterListe = new ObservableCollection<Mitarbeiter>(MitarbeiterListe.Select(m => new Mitarbeiter
        {
            Id = m.Id,
            Vorname = m.Vorname,
            Nachname = m.Nachname,
            Geburtsdatum = m.Geburtsdatum,
            Eintrittsdatum = m.Eintrittsdatum,
            PositionId = m.PositionId,
            Telefon = m.Telefon,
            Email = m.Email,
            Aktiv = m.Aktiv,
            Gehalt = m.Gehalt,
            IstAdmin = m.IstAdmin,
            BildUrl = m.BildUrl,
            Notizen = m.Notizen,
            Strasse = m.Strasse,
            Stadt = m.Stadt,
            PLZ = m.PLZ,
            Land = m.Land,
            PositionTitel = m.PositionTitel
        }));

        // Initialize FilteredMitarbeiterListe with all Mitarbeiter
        FilteredMitarbeiterListe = new ObservableCollection<Mitarbeiter>(MitarbeiterListe);
        OnPropertyChanged(nameof(FilteredMitarbeiterListe));

        // Subscribe to property changes immediately after loading
        SubscribeToMitarbeiterPropertyChanges();
        SubscribeToIsDeletedChanges();
        SubscribeToEditCandidateChanges();
    }

    private void SubscribeToMitarbeiterPropertyChanges()
    {
        foreach (var mitarbeiter in MitarbeiterListe)
        {
            mitarbeiter.PropertyChanged -= Mitarbeiter_PropertyChanged;
            mitarbeiter.PropertyChanged += Mitarbeiter_PropertyChanged;
        }
    }

    private void Mitarbeiter_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is Mitarbeiter mitarbeiter && e.PropertyName == nameof(Mitarbeiter.IsDeleted))
        {
            Console.WriteLine($"PropertyChanged Event received: {e.PropertyName} for Mitarbeiter ID {mitarbeiter.Id}");
            if (mitarbeiter.IsDeleted)
            {
                RemoveDeletedMitarbeiter();
            }
        }
    }

    private void SubscribeToIsDeletedChanges()
    {
        Console.WriteLine("Subscribing to IsDeleted changes for all Mitarbeiter.");
        foreach (var mitarbeiter in MitarbeiterListe)
        {
            mitarbeiter.PropertyChanged -= Mitarbeiter_IsDeletedChanged;
            mitarbeiter.PropertyChanged += Mitarbeiter_IsDeletedChanged;
            Console.WriteLine($"Subscribed to IsDeleted changes for Mitarbeiter: {mitarbeiter.Vorname} {mitarbeiter.Nachname} (ID: {mitarbeiter.Id}).");
        }
    }

    private void Mitarbeiter_IsDeletedChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is Mitarbeiter mitarbeiter && e.PropertyName == nameof(Mitarbeiter.IsDeleted))
        {
            Console.WriteLine($"IsDeleted changed for Mitarbeiter: {mitarbeiter.Vorname} {mitarbeiter.Nachname} (ID: {mitarbeiter.Id})");
            if (mitarbeiter.IsDeleted)
            {
                RemoveDeletedMitarbeiter();
            }
        }
    }

    private void SubscribeToEditCandidateChanges()
    {
        foreach (var mitarbeiter in MitarbeiterListe)
        {
            mitarbeiter.PropertyChanged -= Mitarbeiter_EditCandidateChanged;
            mitarbeiter.PropertyChanged += Mitarbeiter_EditCandidateChanged;
        }
    }

    private void Mitarbeiter_EditCandidateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is Mitarbeiter mitarbeiter && e.PropertyName == nameof(Mitarbeiter.EditCandidate))
        {
            Console.WriteLine($"EditCandidate changed for Mitarbeiter: {mitarbeiter.Vorname} {mitarbeiter.Nachname} (ID: {mitarbeiter.Id})");
            if (mitarbeiter.EditCandidate)
            {
                ToEditMitarbeiterView(mitarbeiter);
            }
        }
    }

    [RelayCommand]
    public void InitializeEditMode()
    {
        foreach (var mitarbeiter in MitarbeiterListe)
        {
            mitarbeiter.EditButtonVisible = true;
        }

        Console.WriteLine("Edit mode initialized for all Mitarbeiter.");
    }

    private bool _deleteModeActivated = false;
    public bool DeleteModeActivated
    {
        get => _deleteModeActivated;
        set
        {
            if (_deleteModeActivated != value)
            {
                _deleteModeActivated = value;
                OnPropertyChanged();
            }
        }
    }

    [RelayCommand]
    public void InitializeDeleteMode()
    {
        foreach (var mitarbeiter in MitarbeiterListe)
        {
            mitarbeiter.DeleteModeActivated = true;
        }

        // No need to reassign FilteredMitarbeiterListe
        OnPropertyChanged(nameof(MitarbeiterListe));

        Console.WriteLine("Delete mode initialized for all Mitarbeiter.");
    }

        // Basic ViewModel for MitarbeiterView
    [RelayCommand]
    public void ToHRView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new HRMViewModel();
        }
    }

    [RelayCommand]
    public void ToNewMitarbeiterView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new NewMitarbeiterViewModel();
        }
    }

    [RelayCommand]
    public void ToEditMitarbeiterView(Mitarbeiter mitarbeiter)
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            Console.WriteLine($"Switching to EditMitarbeiterView for Mitarbeiter: {mitarbeiter.Vorname} {mitarbeiter.Nachname} (ID: {mitarbeiter.Id})");
            mainVm.CurrentViewModel = new EditMitarbeiterViewModel(mitarbeiter);
        }
    }

    [RelayCommand]
    public void EditMitarbeiter(Mitarbeiter mitarbeiter)
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            Console.WriteLine($"Switching to EditMitarbeiterView for Mitarbeiter: {mitarbeiter.Vorname} {mitarbeiter.Nachname} (ID: {mitarbeiter.Id})");
            mainVm.CurrentViewModel = new EditMitarbeiterViewModel(mitarbeiter);
        }
    }

    [RelayCommand]
    public void RemoveDeletedMitarbeiter()
    {
        var deletedMitarbeiter = MitarbeiterListe.Where(m => m.IsDeleted).ToList();
        foreach (var mitarbeiter in deletedMitarbeiter)
        {
            MitarbeiterListe.Remove(mitarbeiter);
        }

        // Refresh the FilteredMitarbeiterListe
        FilteredMitarbeiterListe = new ObservableCollection<Mitarbeiter>(MitarbeiterListe);
        OnPropertyChanged(nameof(FilteredMitarbeiterListe));
        OnPropertyChanged(nameof(MitarbeiterListe));

        Console.WriteLine("Deleted Mitarbeiter removed from the list.");
    }

    private void UpdateEditColumnVisibility()
    {
        var editColumnVisible = FilteredMitarbeiterListe.Any(m => m.EditButtonVisible);

        foreach (var mitarbeiter in FilteredMitarbeiterListe)
        {
            mitarbeiter.EditButtonVisible = !editColumnVisible;
        }

        OnPropertyChanged(nameof(FilteredMitarbeiterListe));
    }

}