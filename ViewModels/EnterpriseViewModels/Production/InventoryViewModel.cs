using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Percuro.Models;
using Percuro.Services;

namespace Percuro.ViewModels.EnterpriseViewModels.Production
{
    public partial class InventoryViewModel : ViewModelBase
    {
        private readonly StorageLocationService _storageLocationService = new();

        public ObservableCollection<StorageLocation> StorageLocations { get; set; } = new();

        public InventoryViewModel()
        {
            LoadStorageLocations();
        }

        private async void LoadStorageLocations()
        {
            var locations = await _storageLocationService.GetStorageLocationsAsync();
            foreach (var location in locations)
            {
                StorageLocations.Add(location);
                Console.WriteLine($"ID: {location.Id}, Name: {location.Name}, Standort: {location.Standort}, Kapazität: {location.Kapazitaet}, Aktiv: {location.AktiverStatus}");
            }
        }
        [RelayCommand]
        public void ToProductionView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new ProductionViewModel();
            }
        }
    }
}