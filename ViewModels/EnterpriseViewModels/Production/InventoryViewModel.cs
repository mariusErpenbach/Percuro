using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Percuro.Services.InventoryServices;

namespace Percuro.ViewModels.EnterpriseViewModels.Production
{
    public partial class InventoryViewModel : ViewModelBase
    {
        private readonly InventoryService _inventoryService = new();
        private readonly InventoryStockService _inventoryStockService = new();
        private readonly InventoryProcessingService _inventoryProcessingService = new();

        private bool _isLoading;
        private string _selectedLager = "Alle (Lager)";
        private string _searchQuery = string.Empty;
        private string selectedSortOption = "Sortieren nach...";

        public ObservableCollection<InventoryStockGroup> GroupedInventoryStocks { get; set; } = new();
        public ObservableCollection<string> LagerOptions { get; set; } = new ObservableCollection<string> { "Alle (Lager)" };
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

        public string SelectedSortOption
        {
            get => selectedSortOption;
            set
            {
                SetProperty(ref selectedSortOption, value);
                if (!string.IsNullOrEmpty(value) && value != "Sortieren nach...")
                {
                    FilterGroupedInventoryStocks();
                }
            }
        }

        [ObservableProperty]
        private bool isRightPanelVisible = false;

        [ObservableProperty]
        private InventoryStock? selectedTransferStock;

        [ObservableProperty]
        private InventoryStock? transferCandidate;

        [ObservableProperty]
        private int transferAmount;

        [ObservableProperty]
        private string? transferReason;

        [ObservableProperty]
        private TargetInventoryStock? selectedTargetStock;

        [ObservableProperty]
        private ObservableCollection<TargetInventoryStock> targetInventoryStocks = new();

        [ObservableProperty]
        private long? selectedItemId;

        [ObservableProperty]
        private int? selectedItemBestand;

        [ObservableProperty]
        private string? selectedItemLagerName;

        [ObservableProperty]
        private string? selectedItemArtikelBezeichnung;

        public InventoryViewModel()
        {
            _ = InitializeTargetInventoryStocksAsync();
            _ = LoadInventoryStocks();
        }

        [RelayCommand]
        public void ToProductionView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new ProductionViewModel();
            }
        }

        [RelayCommand]
        private void ShowRightPanel()
        {
            IsRightPanelVisible = true;
        }

        [RelayCommand]
        private void StartTransfer(InventoryStock stock)
        {
            SelectedTransferStock = stock;
            IsRightPanelVisible = true;
        }

        [RelayCommand]
        private void SetTransferCandidate(InventoryStock stock)
        {
            TransferCandidate = stock;
            IsRightPanelVisible = true;
        }

        [RelayCommand]
        private async Task ExecuteTransferAsync()
        {
            if (SelectedTargetStock == null || string.IsNullOrWhiteSpace(SelectedTargetStock.LagerName) || TransferAmount <= 0 || string.IsNullOrWhiteSpace(TransferReason))
            {
                Console.WriteLine("Ungültige Eingaben für die Umlagerung.");
                return;
            }

            try
            {
                await _inventoryStockService.CreateLagerbewegungAsync(
                    ausgangslager: SelectedItemLagerName ?? "Unbekannt",
                    ziellager: SelectedTargetStock.LagerName,
                    artikelId: (int)(SelectedItemId ?? 0),
                    artikelBezeichnung: SelectedItemArtikelBezeichnung ?? "Unbekannt",
                    menge: TransferAmount,
                    beweggrund: TransferReason
                );

                Console.WriteLine($"Lagerbewegung erfolgreich erstellt: {TransferAmount} Einheiten von {SelectedItemLagerName} nach {SelectedTargetStock.LagerName}.");

                IsRightPanelVisible = false;
                SelectedItemId = null;
                SelectedItemBestand = null;
                SelectedItemLagerName = null;
                SelectedItemArtikelBezeichnung = null;
                SelectedTargetStock = null;
                TransferAmount = 0;
                TransferReason = string.Empty;

                await InitializeTargetInventoryStocksAsync();
                await LoadInventoryStocks();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Erstellen der Lagerbewegung: {ex.Message}");
            }
        }

        private async Task LoadInventoryStocks()
        {
            try
            {
                IsLoading = true;
                var inventoryStocks = await _inventoryStockService.GetInventoryStocksAsync();
                var groupedStocks = await _inventoryProcessingService.GroupInventoryStocksAsync(inventoryStocks);

                GroupedInventoryStocks.Clear();
                foreach (var group in groupedStocks)
                {
                    GroupedInventoryStocks.Add(group);
                }

                LagerOptions.Clear();
                LagerOptions.Add("Alle (Lager)");
                foreach (var group in groupedStocks)
                {
                    LagerOptions.Add(group.LagerName);
                }

                SelectedLager = "Alle (Lager)";

                SubscribeToInventoryStockChanges();
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
            try
            {
                IsLoading = true;
                var inventoryStocks = await _inventoryStockService.GetInventoryStocksAsync();
                var filteredStocks = await _inventoryProcessingService.FilterSortAndGroupInventoryStocksAsync(inventoryStocks, SelectedLager, SearchQuery, SelectedSortOption);

                GroupedInventoryStocks.Clear();
                foreach (var group in filteredStocks)
                {
                    GroupedInventoryStocks.Add(group);
                }

                SubscribeToInventoryStockChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error filtering, sorting, and grouping inventory stocks: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void SubscribeToInventoryStockChanges()
        {
            _inventoryService.SubscribeToInventoryStockChanges(GroupedInventoryStocks, (stock, propertyName) =>
            {
                InventoryStock_PropertyChanged(stock, new PropertyChangedEventArgs(propertyName));
            });
        }

        private void InventoryStock_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var stock = sender as InventoryStock;
            Console.WriteLine($"PropertyChanged Event empfangen: {e.PropertyName} für Artikel ID {stock?.ArtikelId}");

            if (e.PropertyName == nameof(InventoryStock.IsTransferCandidate))
            {
                UpdateRightPanelVisibility(stock?.IsTransferCandidate == true ? stock : null);
            }
        }

        private void UpdateRightPanelVisibility(InventoryStock? transferCandidate)
        {
            if (transferCandidate != null)
            {
                TransferCandidate = transferCandidate;
                SelectedItemId = transferCandidate.Id;
                SelectedItemBestand = transferCandidate.Bestand;
                SelectedItemLagerName = transferCandidate.LagerName;
                SelectedItemArtikelBezeichnung = transferCandidate.ArtikelBezeichnung;
                IsRightPanelVisible = true;
            }
            else
            {
                TransferCandidate = null;
                SelectedItemId = null;
                SelectedItemBestand = null;
                SelectedItemLagerName = null;
                SelectedItemArtikelBezeichnung = null;
                IsRightPanelVisible = false;
            }
        }

        private async Task InitializeTargetInventoryStocksAsync()
        {
            try
            {
                var allStocks = await _inventoryStockService.GetInventoryStocksAsync();

                var groupedStocks = allStocks
                    .GroupBy(stock => stock.LagerName ?? "Unbekannt")
                    .OrderBy(group => group.Key)
                    .Select(group => new TargetInventoryStock
                    {
                        Id = 0,
                        ArtikelBezeichnung = group.Key,
                        LagerName = group.Key,
                        Bestand = group.Sum(item => item.Bestand),
                        Items = new ObservableCollection<InventoryStock>(group)
                    });

                TargetInventoryStocks = new ObservableCollection<TargetInventoryStock>(groupedStocks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Initialisieren der TargetInventoryStocks: {ex.Message}");
            }
        }

        partial void OnTransferCandidateChanged(InventoryStock? value)
        {
            if (value != null)
            {
                value.IsTransferCandidate = true;
            }
        }
    }
}