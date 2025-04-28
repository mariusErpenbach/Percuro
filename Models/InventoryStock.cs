using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

public partial class InventoryStock : ObservableObject
{
    public long Id { get; set; }
    public int ArtikelId { get; set; }
    public int LagerortId { get; set; }
    public int Bestand { get; set; }
    public int? Mindestbestand { get; set; } = 0;
    public string? Platzbezeichnung { get; set; }
    public DateTime? LetzteAenderung { get; set; } = DateTime.Now;
    public string? LagerName { get; set; }
    public string? ArtikelBezeichnung { get; set; }
    public string ArtikelIdDescription => $"Artikel ID: {ArtikelId}";
    public string ArtikelBezeichnungDescription => $"Artikel Bezeichnung: {ArtikelBezeichnung}";
    public string BestandDescription => $"Bestand: {Bestand}";
    public string MindestbestandDescription => $"Mindestbestand: {Mindestbestand}";
    public string PlatzbezeichnungDescription => $"Platzbezeichnung: {Platzbezeichnung}";
    public string LetzteAenderungDescription => $"Letzte Änderung: {LetzteAenderung}";
    public string ArtikelShortInfo => $"{ArtikelId}, {ArtikelBezeichnung}, {Bestand}";
    public string? AlternatingRowColor { get; set; }
    

    [ObservableProperty]
    private bool isTransferCandidate;

    public void SetAlternatingRowColor(int index)
    {
        AlternatingRowColor = index % 2 == 0 ? "LightGray" : "White";
    }

    public void SetAsTransferCandidate()
    {
        // Umschalten zwischen true und false
        IsTransferCandidate = !IsTransferCandidate;
        Console.WriteLine($"Artikel ID {ArtikelId} wurde als Transferkandidat {(IsTransferCandidate ? "markiert" : "demarkiert")}.");
    }
}