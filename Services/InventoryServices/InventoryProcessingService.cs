using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Percuro.Models.InventoryModels;

namespace Percuro.Services.InventoryServices
{
    // Handles processing of inventory data, such as filtering, sorting, and grouping.
    public class InventoryProcessingService
    {
        // Filters, sorts, and groups inventory stocks based on the provided criteria.
        public async Task<List<InventoryStockGroup>> FilterSortAndGroupInventoryStocksAsync(
            List<InventoryStock> inventoryStocks,
            string selectedLager,
            string searchQuery,
            string sortOption)
        {
            var filteredAndSortedStocks = inventoryStocks
                .Where(item =>
                    (selectedLager == "Alle (Lager)" || item.LagerName == selectedLager) &&
                    (string.IsNullOrWhiteSpace(searchQuery) ||
                     (int.TryParse(searchQuery, out var artikelId) && item.ArtikelId.ToString().StartsWith(searchQuery)) ||
                     (!int.TryParse(searchQuery, out _) && item.ArtikelBezeichnung?.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) == true)))
                // Handle nullability warnings by ensuring non-nullable types are used
                .OrderBy<InventoryStock, object>(item => sortOption switch
                {
                    "Menge ▲" => item.Bestand,
                    "Menge ▼" => -item.Bestand,
                    "Letzte Änderung ▲" => item.LetzteAenderung ?? DateTime.MinValue,
                    "Letzte Änderung ▼" => item.LetzteAenderung?.Ticks ?? long.MinValue,
                    _ => 0
                })
                .GroupBy(stock => stock.LagerName ?? "Unbekannt")
                .OrderBy(group => group.Key)
                .Select(group => new InventoryStockGroup
                {
                    LagerName = group.Key,
                    Items = new ObservableCollection<InventoryStock>(group)
                })
                .ToList();

            return await Task.FromResult(filteredAndSortedStocks);
        }

        // Groups inventory stocks by their warehouse name.
        public async Task<List<InventoryStockGroup>> GroupInventoryStocksAsync(List<InventoryStock> inventoryStocks)
        {
            return await Task.Run(() =>
                inventoryStocks
                    .GroupBy(stock => stock.LagerName ?? "Unbekannt")
                    .OrderBy(group => group.Key)
                    .Select(group => new InventoryStockGroup
                    {
                        LagerName = group.Key,
                        Items = new ObservableCollection<InventoryStock>(group)
                    })
                    .ToList());
        }
    }
}