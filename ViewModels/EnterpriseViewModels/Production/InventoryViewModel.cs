using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Percuro.Models;
using Percuro.Services;
using Avalonia.Threading;

namespace Percuro.ViewModels.EnterpriseViewModels.Production
{
    public partial class InventoryViewModel : ViewModelBase
    {
        private readonly StorageLocationService _storageLocationService = new();
        private readonly InventoryStockService _inventoryStockService = new();

        public ObservableCollection<StorageLocation> StorageLocations { get; set; } = new();
        public ObservableCollection<InventoryStock> InventoryStocks { get; set; } = new ObservableCollection<InventoryStock>();

        public class InventoryStockGroup
        {
            public string LagerName { get; set; } = string.Empty;
            public ObservableCollection<InventoryStock> Items { get; set; } = new();
        }

        public ObservableCollection<InventoryStockGroup> GroupedInventoryStocks { get; set; } = new();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public InventoryViewModel()
        {
            LoadInventoryStocks();
            Console.WriteLine("InventoryViewModel initialized. InventoryStocks count: " + InventoryStocks.Count);
        }

        private async void LoadStorageLocations()
        {
            var locations = await _storageLocationService.GetStorageLocationsAsync();
            foreach (var location in locations)
            {
                StorageLocations.Add(location);
            }
        }

        private async void LoadInventoryStocks()
        {
            IsLoading = true;

            var inventoryStocks = await _inventoryStockService.GetInventoryStocksAsync();

            var groupedStocks = inventoryStocks
                .GroupBy(stock => stock.LagerName ?? "Unbekannt")
                .OrderBy(group => group.Key)
                .Select(group => new InventoryStockGroup
                {
                    LagerName = group.Key,
                    Items = new ObservableCollection<InventoryStock>(group)
                });

            GroupedInventoryStocks.Clear();
            foreach (var group in groupedStocks)
            {
                GroupedInventoryStocks.Add(group);
            }

            IsLoading = false;
        }
        [RelayCommand]
        public void ToProductionView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new ProductionViewModel();
            }
        }

        private string _searchQuery = string.Empty;

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                SetProperty(ref _searchQuery, value);
                if (!string.IsNullOrEmpty(value))
                {
                    FilterGroupedInventoryStocks();
                }
            }
        }

        [RelayCommand]
        public void FilterGroupedInventoryStocksCommand()
        {
            FilterGroupedInventoryStocks();
        }

        private void FilterGroupedInventoryStocks()
        {
            if (string.IsNullOrWhiteSpace(_searchQuery))
            {
                // Reset to original grouped stocks if no search query
                LoadInventoryStocks();
                return;
            }

            var filteredStocks = GroupedInventoryStocks
                .Select(group => new InventoryStockGroup
                {
                    LagerName = group.LagerName,
                    Items = new ObservableCollection<InventoryStock>(
                        group.Items.Where(item =>
                            (int.TryParse(_searchQuery, out var artikelId) && item.ArtikelId == artikelId) ||
                            (!int.TryParse(_searchQuery, out _) && item.ArtikelBezeichnung?.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) == true)
                        )
                    )
                })
                .Where(group => group.Items.Any())
                .ToList();

            GroupedInventoryStocks.Clear();
            foreach (var group in filteredStocks)
            {
                GroupedInventoryStocks.Add(group);
            }
        }
    }
}