using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        // Füge eine separate Collection für die rechte Seite mit "Kein Lager" Option hinzu
        public ObservableCollection<InventoryStockGroup> RightSideLagerOptions { get; private set; } = new();

        // Constructor
        public InventoryViewModel()
        {
            // Erstelle eine "Kein Lager" Option, die tatsächlich wählbar ist
            var noneOption = new NoneInventoryStockGroup();
            
            // Initialisiere RightSideLagerOptions mit der "Kein Lager" Option
            RightSideLagerOptions = new ObservableCollection<InventoryStockGroup>() { noneOption };
            
            // Aktualisiere RightSideLagerOptions, wenn sich GroupedInventoryStocks ändert
            GroupedInventoryStocks.CollectionChanged += (s, e) =>
            {
                RightSideLagerOptions.Clear();
                RightSideLagerOptions.Add(noneOption); // Füge die "Kein Lager" Option hinzu
                foreach (var stock in GroupedInventoryStocks)
                {
                    RightSideLagerOptions.Add(stock);
                }
            };
            
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

        [ObservableProperty]
        private InventoryStockGroup? selectedRightSideItem;

        partial void OnSelectedRightSideItemChanged(InventoryStockGroup? value)
        {
            if (value is NoneInventoryStockGroup)
            {
                // Wenn "Kein Lager" ausgewählt ist, setze SelectedRightSideLagerGroup auf null
                SelectedRightSideLagerGroup = null;
                Console.WriteLine("Kein Lager ausgewählt (None option)");
            }
            else if (value != null)
            {
                Console.WriteLine($"Selected LagerName: {value.LagerName}");
                SelectedRightSideLagerGroup = value;
            }
            else
            {
                SelectedRightSideLagerGroup = null;
                Console.WriteLine("Kein Item ausgewählt (null)");
            }
        }

        [ObservableProperty]
        private bool isRightPanelVisible = false;

        [RelayCommand]
        private void ShowRightPanel()
        {
            IsRightPanelVisible = true;
        }

        [ObservableProperty]
        private InventoryStock? selectedTransferStock;

        [RelayCommand]
        private void StartTransfer(InventoryStock stock)
        {
            SelectedTransferStock = stock;
            IsRightPanelVisible = true;
        }

        public void OnTransferButtonClicked(InventoryStock stock)
        {
            SelectedTransferStock = stock;
            IsRightPanelVisible = true;
        }

        [ObservableProperty]
        private InventoryStock? transferCandidate;

        partial void OnTransferCandidateChanged(InventoryStock? value)
        {
            // Hier kannst du auf Änderungen reagieren, z.B. Logging, UI-Updates etc.
            if (value != null)
            {
                value.IsTransferCandidate = true;
            }
            // Optional: Vorherigen Kandidaten zurücksetzen, falls nötig
        }

        private void SubscribeToInventoryStockChanges()
        {
            foreach (var group in GroupedInventoryStocks)
            {
                foreach (var stock in group.Items)
                {
                    stock.PropertyChanged -= InventoryStock_PropertyChanged;
                    stock.PropertyChanged += InventoryStock_PropertyChanged;
                }
            }
        }

        private void InventoryStock_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InventoryStock.IsTransferCandidate))
            {
                var stock = sender as InventoryStock;
                if (stock != null && stock.IsTransferCandidate)
                {
                    // Hier kannst du auf die Änderung reagieren, z.B. Logging, UI-Update, etc.
                    // Beispiel: transferCandidate = stock;
                }
            }
        }

        [RelayCommand]
        private void SetTransferCandidate(InventoryStock stock)
        {
            TransferCandidate = stock;
            IsRightPanelVisible = true;
        }
    }
}