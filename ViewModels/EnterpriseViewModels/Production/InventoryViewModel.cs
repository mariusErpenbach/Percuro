using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Percuro.Models;
using Percuro.Services;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Percuro.ViewModels.EnterpriseViewModels.Production
{
    public partial class InventoryViewModel : ViewModelBase
    {
        // Fields and Constants
        private readonly StorageLocationService _storageLocationService = new();
        private readonly InventoryStockService _inventoryStockService = new();
        private bool _isLoading;
        private string _selectedLager = "Alle (Lager)";
        private string _searchQuery = string.Empty;

        // Properties
        public ObservableCollection<StorageLocation> StorageLocations { get; set; } = new();
        public ObservableCollection<InventoryStock> InventoryStocks { get; set; } = new ObservableCollection<InventoryStock>();
        public ObservableCollection<InventoryStockGroup> GroupedInventoryStocks { get; set; } = new();
        public ObservableCollection<string> LagerOptions { get; set; } = new ObservableCollection<string> { "Alle (Lager)" };
        public ObservableCollection<string> TransferTypeOptions { get; set; } = new ObservableCollection<string> { "Umlagern", "Verkauf" };
        public static ObservableCollection<string> SortOptions { get; set; } = new ObservableCollection<string>
        {
            "Sortieren nach...",
            "Menge ▲",
            "Menge ▼",
            "Letzte Änderung ▲",
            "Letzte Änderung ▼"
        };

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string SelectedLager
        {
            get => _selectedLager;
            set
            {
                SetProperty(ref _selectedLager, value);
                FilterGroupedInventoryStocks();
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                SetProperty(ref _searchQuery, value);
                FilterGroupedInventoryStocks();
            }
        }

        private string selectedSortOption = "Sortieren nach...";
        public string SelectedSortOption
        {
            get => selectedSortOption;
            set
            {
                SetProperty(ref selectedSortOption, value);
                ApplySorting();
            }
        }

        // Constructor
        public InventoryViewModel()
        {
            LoadStorageLocations();
            LoadInventoryStocks();
        }

        // Commands
        [RelayCommand]
        public void ToProductionView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new ProductionViewModel();
            }
        }

        [RelayCommand]
        public void FilterGroupedInventoryStocksCommand()
        {
            FilterGroupedInventoryStocks();
        }

        // Methods
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

                Console.WriteLine("Loading inventory stocks...");
                var inventoryStocks = await _inventoryStockService.GetInventoryStocksAsync();
                Console.WriteLine($"Fetched {inventoryStocks.Count} inventory stocks from the service.");

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
                Console.WriteLine(GroupedInventoryStocks);
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

        private async void FilterGroupedInventoryStocks()
        {
            var filteredStocks = await _inventoryStockService.FilterAndGroupInventoryStocksAsync(SelectedLager, SearchQuery);

            // Apply sorting to the filtered stocks
            var sortedGroups = filteredStocks.Select(group => new InventoryStockGroup
            {
                LagerName = group.LagerName,
                Items = new ObservableCollection<InventoryStock>(
                    SelectedSortOption switch
                    {
                        "Menge ▼" => group.Items.OrderByDescending(item => item.Bestand),
                        "Menge ▲" => group.Items.OrderBy(item => item.Bestand),
                        "Letzte Änderung ▼" => group.Items.OrderByDescending(item => item.LetzteAenderung),
                        "Letzte Änderung ▲" => group.Items.OrderBy(item => item.LetzteAenderung),
                        _ => group.Items
                    })
            }).ToList();

            // Update the GroupedInventoryStocks collection
            GroupedInventoryStocks.Clear();
            foreach (var group in sortedGroups)
            {
                GroupedInventoryStocks.Add(group);
            }
        }

        private void ApplySorting()
        {
            // Create a new collection for sorted groups
            var sortedGroups = GroupedInventoryStocks.Select(group => new InventoryStockGroup
            {
                LagerName = group.LagerName,
                Items = new ObservableCollection<InventoryStock>(
                    SelectedSortOption switch
                    {
                        "Menge ▼" => group.Items.OrderByDescending(item => item.Bestand),
                        "Menge ▲" => group.Items.OrderBy(item => item.Bestand),
                        "Letzte Änderung ▼" => group.Items.OrderByDescending(item => item.LetzteAenderung),
                        "Letzte Änderung ▲" => group.Items.OrderBy(item => item.LetzteAenderung),
                        _ => group.Items
                    })
            }).ToList();

            // Clear and re-add the sorted groups
            GroupedInventoryStocks.Clear();
            foreach (var group in sortedGroups)
            {
                GroupedInventoryStocks.Add(group);
            }
        }

        [ObservableProperty]
        private InventoryStockGroup? selectedRightSideLagerGroup;

        [RelayCommand]
        private void TransferStock()
        {
            // Logic to transfer stock will be implemented here.
        }

        [RelayCommand]
        private void LoadRightSideLagerOptions()
        {
            // Logic to load right-side lager options will be implemented here.
        }

        partial void OnSelectedRightSideLagerGroupChanged(InventoryStockGroup? value)
        {
            if (value != null)
            {
                Console.WriteLine($"Selected LagerName: {value.LagerName}");
                var matchingGroup = GroupedInventoryStocks.FirstOrDefault(group => group.LagerName == value.LagerName);
                if (matchingGroup != null)
                {
                    Console.WriteLine($"Matching group found: {matchingGroup.LagerName} with {matchingGroup.Items.Count} items.");
                    SelectedRightSideLagerGroup = matchingGroup;
                }
                else
                {
                    Console.WriteLine("No matching group found.");
                }
            }
            else
            {
                Console.WriteLine("SelectedRightSideLagerGroup is null.");
            }
        }
    }
}