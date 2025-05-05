using System;

namespace Percuro.Models.MitarbeiterModels;

public class Mitarbeiter
{
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

}