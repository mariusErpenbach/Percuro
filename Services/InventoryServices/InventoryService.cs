using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Percuro.Models;

namespace Percuro.Services.InventoryServices
{
    public class InventoryService
    {
        public ObservableCollection<InventoryStock> SortInventoryItems(ObservableCollection<InventoryStock> items, string sortOption)
        {
            return new ObservableCollection<InventoryStock>(
                sortOption switch
                {
                    "Menge ▼" => items.OrderByDescending(item => item.Bestand),
                    "Menge ▲" => items.OrderBy(item => item.Bestand),
                    "Letzte Änderung ▼" => items.OrderByDescending(item => item.LetzteAenderung),
                    "Letzte Änderung ▲" => items.OrderBy(item => item.LetzteAenderung),
                    _ => items
                });
        }

        public List<InventoryStock> SortInventoryStocks(List<InventoryStock> stocks, string sortOption)
        {
            return sortOption switch
            {
                "Menge ▲" => stocks.OrderBy(stock => stock.Bestand).ToList(),
                "Menge ▼" => stocks.OrderByDescending(stock => stock.Bestand).ToList(),
                "Letzte Änderung ▲" => stocks.OrderBy(stock => stock.LetzteAenderung).ToList(),
                "Letzte Änderung ▼" => stocks.OrderByDescending(stock => stock.LetzteAenderung).ToList(),
                _ => stocks
            };
        }

        public async Task<List<InventoryStockGroup>> FilterAndGroupInventoryStocksAsync(
            List<InventoryStock> inventoryStocks,
            string selectedLager,
            string searchQuery)
        {
            var filteredStocks = inventoryStocks
                .Where(item =>
                    (selectedLager == "Alle (Lager)" || item.LagerName == selectedLager) &&
                    (string.IsNullOrWhiteSpace(searchQuery) ||
                     (int.TryParse(searchQuery, out var artikelId) && item.ArtikelId.ToString().StartsWith(searchQuery)) ||
                     (!int.TryParse(searchQuery, out _) && item.ArtikelBezeichnung?.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) == true)))
                .GroupBy(stock => stock.LagerName ?? "Unbekannt")
                .OrderBy(group => group.Key)
                .Select(group => new InventoryStockGroup
                {
                    LagerName = group.Key,
                    Items = new ObservableCollection<InventoryStock>(group)
                })
                .ToList();

            return await Task.FromResult(filteredStocks);
        }

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

        public void SubscribeToInventoryStockChanges(ObservableCollection<InventoryStockGroup> groupedInventoryStocks, Action<InventoryStock, string> onPropertyChanged)
        {
            foreach (var group in groupedInventoryStocks)
            {
                foreach (var stock in group.Items)
                {
                    stock.PropertyChanged -= (sender, e) =>
                    {
                        if (sender is InventoryStock inventoryStock && e.PropertyName != null)
                        {
                            onPropertyChanged(inventoryStock, e.PropertyName);
                        }
                    };

                    stock.PropertyChanged += (sender, e) =>
                    {
                        if (sender is InventoryStock inventoryStock && e.PropertyName != null)
                        {
                            onPropertyChanged(inventoryStock, e.PropertyName);
                        }
                    };
                }
            }
        }
    }
}