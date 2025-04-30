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
    public string UmlaufmengeDescription => $"Umlaufmenge: {Umlaufmenge}"; // Define Umlaufmenge as a descriptive text property
    public int Umlaufmenge { get; set; } // New property for Umlaufmenge
    public int Verfuegbar => Bestand - Umlaufmenge; // New computed property for Verfuegbar

    [ObservableProperty]
    private bool isTransferCandidate;

    public void SetAsTransferCandidate()
    {
        // Umschalten zwischen true und false
        IsTransferCandidate = !IsTransferCandidate;
        Console.WriteLine($"Artikel ID {ArtikelId} wurde als Transferkandidat {(IsTransferCandidate ? "markiert" : "demarkiert")}.");
    }

    [ObservableProperty]
    private bool isKorrekturCandidate;
    public void SetAsKorrekturCandidate()
    {
        // Umschalten zwischen true und false
        IsKorrekturCandidate = !IsKorrekturCandidate;
        Console.WriteLine($"Artikel ID {ArtikelId} wurde als Korrekturkandidat {(IsKorrekturCandidate ? "markiert" : "demarkiert")}.");
    }

    private bool candidateButtonsEnabled = true;

    public bool CandidateButtonsEnabled
    {
        get => candidateButtonsEnabled;
        set => SetProperty(ref candidateButtonsEnabled, value);
    }

    public void ResetCandidateButtons()
    {
        CandidateButtonsEnabled = true;
    }

    private bool isCandidateSet;
    public bool IsCandidateSet
    {
        get => isCandidateSet;
        private set => SetProperty(ref isCandidateSet, value);
    }

    partial void OnIsTransferCandidateChanged(bool value)
    {
        CandidateButtonsEnabled = !value;
        UpdateIsCandidateSet();
    }

    partial void OnIsKorrekturCandidateChanged(bool value)
    {
        CandidateButtonsEnabled = !value;
        UpdateIsCandidateSet();
    }

    private void UpdateIsCandidateSet()
    {
        IsCandidateSet = IsTransferCandidate || IsKorrekturCandidate;
    }
}