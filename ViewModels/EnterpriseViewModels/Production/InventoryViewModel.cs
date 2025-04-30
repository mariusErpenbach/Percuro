using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Percuro.Services.InventoryServices;

namespace Percuro.ViewModels.EnterpriseViewModels.Production
{
    /// <summary>
    /// ViewModel for managing inventory-related operations and UI interactions.
    /// </summary>
    public partial class InventoryViewModel : ViewModelBase
    {
        private readonly InventoryDatabaseService _inventoryDatabaseService = new();
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
        private bool isTransferPanelVisible = false;

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

        [ObservableProperty]
        private int? umlaufmenge; // Ensure Umlaufmenge is available in the ViewModel

        [ObservableProperty]
        private string umlaufmengeDescription = string.Empty; // Property to hold UmlaufmengeDescription

        [ObservableProperty]
        private int verfuegbar; // Property to hold Verfuegbar value

        [ObservableProperty]
        private string verfuegbarDescription = string.Empty; // Property to hold VerfuegbarDescription

        [ObservableProperty]
        private bool isKorrekturPanelVisible = false;

        [ObservableProperty]
        private bool isInputEnabled = true;

        [ObservableProperty]
        private List<string>? selectedStockDetails;

        [ObservableProperty]
        private BestandskorrekturModel? bestandskorrekturDetails;

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private bool _isErrorMessageVisible;
        public bool IsErrorMessageVisible
        {
            get => _isErrorMessageVisible;
            set => SetProperty(ref _isErrorMessageVisible, value);
        }

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
            IsTransferPanelVisible = true;
        }

        [RelayCommand]
        private void StartTransfer(InventoryStock stock)
        {
            SelectedTransferStock = stock;
            IsTransferPanelVisible = true;
        }

        [RelayCommand]
        private void SetAsTransferCandidate(InventoryStock stock)
        {
            Console.WriteLine("[SetAsTransferCandidate] Called with stock: " + stock.ArtikelBezeichnung);

            // Set filters to display only the selected stock in the left panel
            Console.WriteLine("[SetAsTransferCandidate] Setting SelectedLager to: " + (stock.LagerName ?? "Alle (Lager)"));
            SetProperty(ref _selectedLager, stock.LagerName ?? "Alle (Lager)", nameof(SelectedLager));

            Console.WriteLine("[SetAsTransferCandidate] Setting SearchQuery to: " + (stock.ArtikelBezeichnung ?? string.Empty));
            SetProperty(ref _searchQuery, stock.ArtikelBezeichnung ?? string.Empty, nameof(SearchQuery));

            // Disable input and show the transfer panel
            TransferCandidate = stock;
            IsInputEnabled = false;
            IsTransferPanelVisible = true;

            Console.WriteLine("[SetAsTransferCandidate] Transfer panel visibility set to true.");
        }

        // Executes a stock transfer operation and updates the database.
        /// <returns>A task that represents the asynchronous operation.</returns>
        [RelayCommand]
        private async Task ExecuteTransferAsync()
        {
            if (SelectedTargetStock == null || string.IsNullOrWhiteSpace(SelectedTargetStock.LagerName))
            {
                ErrorMessage = "Bitte wählen Sie ein gültiges Ziel-Lager aus.";
                IsErrorMessageVisible = true;
                return;
            }

            if (TransferAmount <= 0)
            {
                ErrorMessage = "Die Umlagerungsmenge muss größer als 0 sein.";
                IsErrorMessageVisible = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(TransferReason))
            {
                ErrorMessage = "Bitte geben Sie einen Beweggrund für die Umlagerung an.";
                IsErrorMessageVisible = true;
                return;
            }

            if (SelectedItemBestand.HasValue && Umlaufmenge.HasValue && TransferAmount > (SelectedItemBestand.Value - Umlaufmenge.Value))
            {
                ErrorMessage = $"Die Umlagerungsmenge ({TransferAmount}) überschreitet den verfügbaren Bestand ({SelectedItemBestand.Value - Umlaufmenge.Value}).";
                IsErrorMessageVisible = true;
                return;
            }

            try
            {
                await _inventoryDatabaseService.CreateLagerbewegungAsync(
                    ausgangslager: SelectedItemLagerName ?? "Unbekannt",
                    ziellager: SelectedTargetStock.LagerName,
                    artikelId: (int)(SelectedItemId ?? 0),
                    artikelBezeichnung: SelectedItemArtikelBezeichnung ?? "Unbekannt",
                    menge: TransferAmount,
                    beweggrund: TransferReason
                );

                Console.WriteLine($"Lagerbewegung erfolgreich erstellt: {TransferAmount} Einheiten von {SelectedItemLagerName} nach {SelectedTargetStock.LagerName}.");

                IsTransferPanelVisible = false;
                SelectedItemId = null;
                SelectedItemBestand = null;
                SelectedItemLagerName = null;
                SelectedItemArtikelBezeichnung = null;
                SelectedTargetStock = null;
                TransferAmount = 0;
                TransferReason = string.Empty;

                ErrorMessage = string.Empty;
                IsErrorMessageVisible = false;

                await InitializeTargetInventoryStocksAsync();
                await LoadInventoryStocks();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fehler beim Erstellen der Lagerbewegung: {ex.Message}";
                IsErrorMessageVisible = true;
            }
            finally
            {
                IsInputEnabled = true;
            }
        }

        // Loads inventory stocks and groups them for display.
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task LoadInventoryStocks()
        {
            try
            {
                IsLoading = true;
                var inventoryStocks = await _inventoryDatabaseService.GetInventoryStocksAsync();
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

                // Fetch Umlaufmenge and Verfuegbar for the first stock (adjust as needed)
                if (inventoryStocks.Any())
                {
                    Umlaufmenge = inventoryStocks.First().Umlaufmenge;
                    Verfuegbar = inventoryStocks.First().Verfuegbar;
                    UmlaufmengeDescription = $"Umlaufmenge: {Umlaufmenge}";
                    VerfuegbarDescription = $"Verfügbar: {Verfuegbar}";
                }

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

        // Filters and sorts grouped inventory stocks based on user input.
        private async void FilterGroupedInventoryStocks()
        {
            try
            {
                Console.WriteLine("[FilterGroupedInventoryStocks] Started filtering with:");
                Console.WriteLine("[FilterGroupedInventoryStocks] SelectedLager: " + SelectedLager);
                Console.WriteLine("[FilterGroupedInventoryStocks] SearchQuery: " + SearchQuery);
                Console.WriteLine("[FilterGroupedInventoryStocks] SelectedSortOption: " + SelectedSortOption);

                IsLoading = true;
                var inventoryStocks = await _inventoryDatabaseService.GetInventoryStocksAsync();
                var filteredStocks = await _inventoryProcessingService.FilterSortAndGroupInventoryStocksAsync(inventoryStocks, SelectedLager, SearchQuery, SelectedSortOption);

                Console.WriteLine("[FilterGroupedInventoryStocks] Filtered groups count: " + filteredStocks.Count);

                GroupedInventoryStocks.Clear();
                foreach (var group in filteredStocks)
                {
                    Console.WriteLine("[FilterGroupedInventoryStocks] Adding group: " + group.LagerName);
                    GroupedInventoryStocks.Add(group);
                }

                SubscribeToInventoryStockChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FilterGroupedInventoryStocks] Error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Subscribes to property changes in inventory stocks to update the UI.
        private void SubscribeToInventoryStockChanges()
        {
            foreach (var group in GroupedInventoryStocks)
            {
                foreach (var stock in group.Items)
                {
                    stock.PropertyChanged -= InventoryStock_PropertyChanged;
                    stock.PropertyChanged += InventoryStock_PropertyChanged;

                    // Subscribe to isKorrekturCandidate and isTransferCandidate changes
                    stock.PropertyChanged -= OnIsKorrekturCandidateChanged;
                    stock.PropertyChanged -= OnIsTransferCandidateChanged;
                    stock.PropertyChanged += OnIsKorrekturCandidateChanged;
                    stock.PropertyChanged += OnIsTransferCandidateChanged;
                }
            }
        }

        private void InventoryStock_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var stock = sender as InventoryStock;
            Console.WriteLine($"PropertyChanged Event empfangen: {e.PropertyName} für Artikel ID {stock?.ArtikelId}");
        }

        private void OnIsKorrekturCandidateChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InventoryStock.IsKorrekturCandidate))
            {
                var stock = sender as InventoryStock;
                if (stock?.IsKorrekturCandidate == true)
                {
                    IsInputEnabled = false;
                }
                UpdateKorrekturPanelVisibility(stock?.IsKorrekturCandidate == true ? stock : null);
            }
        }

        private void OnIsTransferCandidateChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InventoryStock.IsTransferCandidate))
            {
                var stock = sender as InventoryStock;
                if (stock?.IsTransferCandidate == true)
                {
                    Console.WriteLine("[OnIsTransferCandidateChanged] Transfer candidate selected: " + stock.ArtikelBezeichnung);

                    // Update filters to display only the selected stock in the left panel
                    SelectedLager = stock.LagerName ?? "Alle (Lager)";
                    SearchQuery = stock.ArtikelBezeichnung ?? string.Empty;

                    IsInputEnabled = false;
                }
                UpdateTransferVisibility(stock?.IsTransferCandidate == true ? stock : null);
            }
        }

        private async void UpdateKorrekturPanelVisibility(InventoryStock? korrecturCandidate)
        {
            if (korrecturCandidate != null)
            {
                IsKorrekturPanelVisible = true;
                var stock = await _inventoryDatabaseService.GetInventoryStockByIdAsync(korrecturCandidate.Id);
                if (stock != null)
                {
                    BestandskorrekturDetails = new BestandskorrekturModel
                    {
                        Id = stock.Id,
                        ArtikelId = stock.ArtikelId,
                        Bestand = stock.Bestand,
                        Mindestbestand = stock.Mindestbestand,
                        Platzbezeichnung = stock.Platzbezeichnung,
                        LetzteAenderung = stock.LetzteAenderung,
                        LagerName = stock.LagerName,
                        ArtikelBezeichnung = stock.ArtikelBezeichnung,
                        Umlaufmenge = stock.Umlaufmenge
                    };
                }
            }
            else
            {
                IsKorrekturPanelVisible = false;
                BestandskorrekturDetails = null;
            }
        }

        private void UpdateTransferVisibility(InventoryStock? transferCandidate)
        {
            if (transferCandidate != null)
            {
                TransferCandidate = transferCandidate;
                SelectedItemId = transferCandidate.Id;
                SelectedItemBestand = transferCandidate.Bestand;
                SelectedItemLagerName = transferCandidate.LagerName;
                SelectedItemArtikelBezeichnung = transferCandidate.ArtikelBezeichnung;
                IsTransferPanelVisible = true;
            }
            else
            {
                TransferCandidate = null;
                SelectedItemId = null;
                SelectedItemBestand = null;
                SelectedItemLagerName = null;
                SelectedItemArtikelBezeichnung = null;
                IsTransferPanelVisible = false;
            }
        }

        // Initializes target inventory stocks for transfer operations.
        private async Task InitializeTargetInventoryStocksAsync()
        {
            try
            {
                var allStocks = await _inventoryDatabaseService.GetInventoryStocksAsync();

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

        [RelayCommand]
        private void SetAsKorrekturCandidate()
        {
            IsKorrekturPanelVisible = true;
            IsInputEnabled = false;
        }

        [RelayCommand]
        private void CancelInventoryAction()
        {
            IsKorrekturPanelVisible = false;
            IsTransferPanelVisible = false;

            BestandskorrekturDetails = null;

            // Reset TransferCandidate
            if (TransferCandidate != null)
            {
                TransferCandidate.IsTransferCandidate = false;
                TransferCandidate = null;
            }

            SelectedItemId = null;
            SelectedItemBestand = null;
            SelectedItemLagerName = null;
            SelectedItemArtikelBezeichnung = null;
            TransferReason = string.Empty; 
            TransferAmount = 0;
            SelectedTargetStock = null;

            IsInputEnabled = true;

            // Reset CandidateButtonsEnabled and IsKorrekturCandidate for all stocks
            foreach (var group in GroupedInventoryStocks)
            {
                foreach (var stock in group.Items)
                {
                    stock.IsKorrekturCandidate = false;
                    stock.ResetCandidateButtons();
                }
            }

            // Reset error message
            ErrorMessage = string.Empty;
            IsErrorMessageVisible = false;
        }

        [RelayCommand]
        private async Task ConfirmCorrectionAsync()
        {
            if (BestandskorrekturDetails == null)
            {
                ErrorMessage = "Keine Bestandskorrektur-Daten verfügbar.";
                IsErrorMessageVisible = true;
                return;
            }

            if (BestandskorrekturDetails.Bestand < 0)
            {
                ErrorMessage = "Der Bestand darf nicht negativ sein.";
                IsErrorMessageVisible = true;
                return;
            }

            if (BestandskorrekturDetails.Mindestbestand.HasValue && BestandskorrekturDetails.Mindestbestand > BestandskorrekturDetails.Bestand)
            {
                ErrorMessage = "Der Mindestbestand darf den aktuellen Bestand nicht überschreiten.";
                IsErrorMessageVisible = true;
                return;
            }

            try
            {
                Console.WriteLine("[ConfirmCorrectionAsync] Starting correction for ID: " + BestandskorrekturDetails.Id);

                await _inventoryDatabaseService.UpdateInventoryStockAsync(BestandskorrekturDetails);
                Console.WriteLine("[ConfirmCorrectionAsync] Bestandskorrektur erfolgreich abgeschlossen.");

                // Refresh inventory data
                await LoadInventoryStocks();

                // Reset the view by calling CancelInventoryAction
                CancelInventoryAction();

                // Reset error message
                ErrorMessage = string.Empty;
                IsErrorMessageVisible = false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fehler bei der Bestandskorrektur: {ex.Message}";
                IsErrorMessageVisible = true;
            }
        }

        [RelayCommand]
        public void ClearSearchQuery()
        {
            SearchQuery = string.Empty;
        }
    }
}