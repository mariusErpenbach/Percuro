using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Percuro.Models.MitarbeiterModels;
using Percuro.Services.MitarbeiterServices;

namespace Percuro.ViewModels.EnterpriseViewModels.HR;

public partial class MitarbeiterViewModel : ViewModelBase
{
    private readonly MitarbeiterDatabaseService _mitarbeiterService;

    public ObservableCollection<Mitarbeiter> MitarbeiterListe { get; set; } = new();

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