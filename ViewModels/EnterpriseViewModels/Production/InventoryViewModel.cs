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
            LoadStorageLocations();
            LoadInventoryStocks();
        }

        private string _selectedLager = "Alle (Lager)";
        public string SelectedLager
        {
            get => _selectedLager;
            set
            {
                SetProperty(ref _selectedLager, value);
                FilterGroupedInventoryStocks(); // Reapply filter when the selected Lager changes
            }
        }

        public ObservableCollection<string> LagerOptions { get; set; } = new ObservableCollection<string> { "Alle (Lager)" };

        private async void LoadStorageLocations()
        {
            try
            {
                var locations = await _storageLocationService.GetStorageLocationsAsync();
                foreach (var location in locations)
                {
                    StorageLocations.Add(location);
                    if (!LagerOptions.Contains(location.Name))
                    {
                        LagerOptions.Add(location.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading storage locations: {ex.Message}");
            }
        }

        private async void LoadInventoryStocks()
        {
            try
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading inventory stocks: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
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

        private string _searchQuery = string.Empty;

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                SetProperty(ref _searchQuery, value);
                FilterGroupedInventoryStocks(); // Always filter the list, even if the query is empty
            }
        }

        [RelayCommand]
        public void FilterGroupedInventoryStocksCommand()
        {
            FilterGroupedInventoryStocks();
        }

        private async void FilterGroupedInventoryStocks()
        {
            var inventoryStocks = await _inventoryStockService.GetInventoryStocksAsync();

            var filteredStocks = inventoryStocks
                .Where(item =>
                    (SelectedLager == "Alle (Lager)" || item.LagerName == SelectedLager) &&
                    (string.IsNullOrWhiteSpace(_searchQuery) ||
                     (int.TryParse(_searchQuery, out var artikelId) && item.ArtikelId.ToString().StartsWith(_searchQuery)) ||
                     (!int.TryParse(_searchQuery, out _) && item.ArtikelBezeichnung?.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) == true)))
                .GroupBy(stock => stock.LagerName ?? "Unbekannt")
                .OrderBy(group => group.Key)
                .Select(group => new InventoryStockGroup
                {
                    LagerName = group.Key,
                    Items = new ObservableCollection<InventoryStock>(group)
                });

            GroupedInventoryStocks.Clear();
            foreach (var group in filteredStocks)
            {
                GroupedInventoryStocks.Add(group);
            }
        }

        public static ObservableCollection<string> SortOptions { get; set; } = new ObservableCollection<string>
        {
            "Sortieren nach...",
            "Menge ▲",
            "Menge ▼",
            "Letzte Änderung ▲",
            "Letzte Änderung ▼"
        };

        private string selectedSortOption = "Sortieren nach...";
        public string SelectedSortOption
        {
            get => selectedSortOption;
            set
            {
                SetProperty(ref selectedSortOption, value);
                ApplySorting(); // Apply sorting whenever the sort option changes
            }
        }

        private void ApplySorting()
        {
            var sortedStocks = GroupedInventoryStocks.SelectMany(group => group.Items).ToList();

            switch (selectedSortOption)
            {
                case "Menge ▲":
                    sortedStocks = sortedStocks.OrderBy(stock => stock.Bestand).ToList();
                    break;
                case "Menge ▼":
                    sortedStocks = sortedStocks.OrderByDescending(stock => stock.Bestand).ToList();
                    break;
                case "Letzte Änderung ▲":
                    sortedStocks = sortedStocks.OrderBy(stock => stock.LetzteAenderung).ToList();
                    break;
                case "Letzte Änderung ▼":
                    sortedStocks = sortedStocks.OrderByDescending(stock => stock.LetzteAenderung).ToList();
                    break;
                default:
                    return; // No sorting applied
            }

            // Re-group the sorted stocks
            var groupedStocks = sortedStocks
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
        }
    }
}