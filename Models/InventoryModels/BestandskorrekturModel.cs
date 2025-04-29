using System;

public class BestandskorrekturModel
{
    public long Id { get; set; }
    public int ArtikelId { get; set; }
    public int Bestand { get; set; }
    public int? Mindestbestand { get; set; }
    public string? Platzbezeichnung { get; set; }
    public DateTime? LetzteAenderung { get; set; }
    public string? LagerName { get; set; }
    public string? ArtikelBezeichnung { get; set; }
    public int Umlaufmenge { get; set; }
    public string ArtikelIdDescription => $"Artikel ID: {ArtikelId}";
    public string BestandDescription => $"Bestand: {Bestand}";
    public string MindestbestandDescription => $"Mindestbestand: {Mindestbestand}";
    public string PlatzbezeichnungDescription => $"Platz: {Platzbezeichnung}";
    public string LetzteAenderungDescription => $"Letzte Änderung: {LetzteAenderung}";
    public string LagerNameDescription => $"Lager: {LagerName}";
    public string ArtikelBezeichnungDescription => $"Bez.: {ArtikelBezeichnung}";
    public string UmlaufmengeDescription => $"Umlaufmenge: {Umlaufmenge}";
}