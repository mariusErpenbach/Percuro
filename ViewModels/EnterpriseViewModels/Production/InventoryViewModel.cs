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
        private string _selectedRightSideLager = string.Empty;

        // Properties
        public ObservableCollection<StorageLocation> StorageLocations { get; set; } = new();
        public ObservableCollection<InventoryStock> InventoryStocks { get; set; } = new ObservableCollection<InventoryStock>();
        public ObservableCollection<InventoryStockGroup> GroupedInventoryStocks { get; set; } = new();
        public ObservableCollection<string> LagerOptions { get; set; } = new ObservableCollection<string> { "Alle (Lager)" };
        public ObservableCollection<string> RightSideLagerOptions { get; set; } = new ObservableCollection<string>();
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

        public string SelectedRightSideLager
        {
            get => _selectedRightSideLager;
            set
            {
                SetProperty(ref _selectedRightSideLager, value);
                UpdateRightSideLagerGroup();
            }
        }

        public InventoryStockGroup? SelectedRightSideLagerGroup { get; set; }

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
            LoadRightSideLagerOptions();
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

        private void UpdateRightSideLagerGroup()
        {
            if (!string.IsNullOrEmpty(SelectedRightSideLager))
            {
                SelectedRightSideLagerGroup = GroupedInventoryStocks.FirstOrDefault(group => group.LagerName?.Equals(SelectedRightSideLager, StringComparison.OrdinalIgnoreCase) == true);
            }
            else
            {
                SelectedRightSideLagerGroup = null;
            }
        }

        private async void LoadRightSideLagerOptions()
        {
            try
            {
                var locations = await _storageLocationService.GetStorageLocationsAsync();
                RightSideLagerOptions.Clear();
                RightSideLagerOptions.Insert(0, "Kein ziel");
                foreach (var location in locations)
                {
                    if (!RightSideLagerOptions.Contains(location.Name))
                    {
                        RightSideLagerOptions.Add(location.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading right-side storage locations: {ex.Message}");
            }
        }

        private string _selectedTransferTypeOption = "Umlagern";
        public string SelectedTransferTypeOption
        {
            get => _selectedTransferTypeOption;
            set
            {
                SetProperty(ref _selectedTransferTypeOption, value);
                OnSelectedTransferTypeOptionChanged(value);
            }
        }

        public bool IsRightSideLagerOptionsVisible
        {
            get => SelectedTransferTypeOption == "Umlagern";
        }

        partial void OnSelectedTransferTypeOptionChanged(string value);

        partial void OnSelectedTransferTypeOptionChanged(string value)
        {
            // Notify the view about the visibility change of the RightSideLagerOptions
            OnPropertyChanged(nameof(IsRightSideLagerOptionsVisible));
        }
    }
}