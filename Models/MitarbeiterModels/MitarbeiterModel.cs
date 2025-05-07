using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Percuro.Services;
using Percuro.Services.MitarbeiterServices;

namespace Percuro.Models.MitarbeiterModels;

public partial class Mitarbeiter : ObservableObject
{
    [ObservableProperty]
    private bool editCandidate;

    [ObservableProperty]
    private bool editButtonVisible;

    [ObservableProperty]
    private bool deleteModeActivated;

    [ObservableProperty]
    private bool isDeleted;

    public int Id { get; set; }
    public string? Vorname { get; set; }
    public string? Nachname { get; set; }
    public DateTime? Geburtsdatum { get; set; }
    public DateTime? Eintrittsdatum { get; set; }
    public int? PositionId { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public bool Aktiv { get; set; } = true;
    public decimal? Gehalt { get; set; }
    public bool IstAdmin { get; set; } = false;
    public string? BildUrl { get; set; }
    public string? Notizen { get; set; }

    public int? Plz {get; set; }

        // Address properties
    public string? Strasse { get; set; }
    public string? Stadt { get; set; }
    public string? PLZ { get; set; }
    public string? Land { get; set; }
    public string? PositionTitel { get; set; }
    public string? FormattedGeburtsdatum => Geburtsdatum?.ToString("dd.MM.yyyy") ?? string.Empty;
    public string? FormattedEintrittsdatum => Eintrittsdatum?.ToString("dd.MM.yyyy");
    public int? AdressbuchId { get; set; }

    
    [RelayCommand]
    public void EditMitarbeiter()
    {
        EditCandidate = true;
        Console.WriteLine("EditMitarbeiter command executed.");
    }

    [RelayCommand]
    public async Task DeleteMitarbeiterAsync()
    {
        try
        {
            var databaseService = new MitarbeiterDatabaseService();
            await databaseService.DeleteMitarbeiterAsync(Id);
            Console.WriteLine($"Mitarbeiter with ID {Id} has been deleted from the database.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting Mitarbeiter with ID {Id}: {ex.Message}");
        }
    }
}