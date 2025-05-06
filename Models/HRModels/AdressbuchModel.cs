namespace Percuro.Models.HRModels;

public class Adressbuch
{
    public int Id { get; set; }
    public string Strasse { get; set; } = string.Empty;
    public string? Hausnummer { get; set; }
    public string Plz { get; set; } = string.Empty;
    public string Stadt { get; set; } = string.Empty;
    public string Land { get; set; } = string.Empty;
    public string? Adresszusatz { get; set; }
    public string? Typ { get; set; }
}