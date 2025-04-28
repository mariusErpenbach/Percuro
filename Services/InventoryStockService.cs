using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MySqlConnector;
using Percuro.Models;
using DotNetEnv;
using System.Data;

namespace Percuro.Services
{
    public class InventoryStockService
    {
        private readonly string _connectionString;

        public InventoryStockService()
        {
            // Load environment variables
            Env.Load();
            _connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION")
                                ?? throw new Exception("MYSQL_CONNECTION is not set.");
        }

        public async Task<List<InventoryStock>> GetInventoryStocksAsync()
        {
            var inventoryStocks = new List<InventoryStock>();

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = "SELECT id, artikel_id, artikel_bezeichnung, bestand, mindestbestand, platzbezeichnung, letzte_aenderung, lager_name FROM lagerbestaende";
                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    inventoryStocks.Add(new InventoryStock
                    {
                        Id = reader.GetInt64("id"),
                        ArtikelId = reader.GetInt32("artikel_id"),
                        Bestand = reader.GetInt32("bestand"),
                        Mindestbestand = reader.IsDBNull("mindestbestand") ? null : reader.GetInt32("mindestbestand"),
                        Platzbezeichnung = reader.IsDBNull("platzbezeichnung") ? null : reader.GetString("platzbezeichnung"),
                        LetzteAenderung = reader.IsDBNull("letzte_aenderung") ? null : reader.GetDateTime("letzte_aenderung"),
                        LagerName = reader.IsDBNull("lager_name") ? null : reader.GetString("lager_name"),
                        ArtikelBezeichnung = reader.IsDBNull("artikel_bezeichnung") ? null : reader.GetString("artikel_bezeichnung")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Abrufen der Lagerbestände: {ex.Message}");
            }

            return inventoryStocks;
        }

        public async Task<List<InventoryStockGroup>> FilterAndGroupInventoryStocksAsync(string selectedLager, string searchQuery)
        {
            var inventoryStocks = await GetInventoryStocksAsync();

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

            return filteredStocks;
        }

        public List<InventoryStock> SortInventoryStocks(List<InventoryStock> stocks, string sortOption)
        {
            return sortOption switch
            {
                "Menge \\u25b2" => stocks.OrderBy(stock => stock.Bestand).ToList(),
                "Menge \\u25bc" => stocks.OrderByDescending(stock => stock.Bestand).ToList(),
                "Letzte \\u00c4nderung \\u25b2" => stocks.OrderBy(stock => stock.LetzteAenderung).ToList(),
                "Letzte \\u00c4nderung \\u25bc" => stocks.OrderByDescending(stock => stock.LetzteAenderung).ToList(),
                _ => stocks
            };
        }

        public async Task TransferStockAsync(string targetLagerName, int quantity)
        {
            if (string.IsNullOrEmpty(targetLagerName))
            {
                throw new ArgumentException("Target warehouse name cannot be null or empty.", nameof(targetLagerName));
            }

            if (quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
            }

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                // Example query to update stock in the target warehouse
                string query = @"UPDATE lagerbestaende 
                                 SET bestand = bestand + @quantity 
                                 WHERE lager_name = @targetLagerName";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@quantity", quantity);
                cmd.Parameters.AddWithValue("@targetLagerName", targetLagerName);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    throw new Exception("No rows were updated. The target warehouse might not exist.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during stock transfer: {ex.Message}");
                throw;
            }
        }

        public async Task CreateLagerbewegungAsync(string ausgangslager, string ziellager, int artikelId, string artikelBezeichnung, int menge, string beweggrund)
        {
            if (string.IsNullOrEmpty(ausgangslager) || string.IsNullOrEmpty(ziellager))
            {
                throw new ArgumentException("Ausgangslager und Ziellager dürfen nicht leer sein.");
            }

            if (menge <= 0)
            {
                throw new ArgumentException("Die Menge muss größer als null sein.");
            }

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                // Füge die Lagerbewegung hinzu
                string insertQuery = @"INSERT INTO lagerbewegungen (ausgangslager, ziellager, artikel_id, artikel_bezeichnung, menge, datum, beweggrund)
                                       VALUES (@ausgangslager, @ziellager, @artikelId, @artikelBezeichnung, @menge, NOW(), @beweggrund)";

                using var insertCmd = new MySqlCommand(insertQuery, connection);
                insertCmd.Parameters.AddWithValue("@ausgangslager", ausgangslager);
                insertCmd.Parameters.AddWithValue("@ziellager", ziellager);
                insertCmd.Parameters.AddWithValue("@artikelId", artikelId);
                insertCmd.Parameters.AddWithValue("@artikelBezeichnung", artikelBezeichnung);
                insertCmd.Parameters.AddWithValue("@menge", menge);
                insertCmd.Parameters.AddWithValue("@beweggrund", beweggrund);

                await insertCmd.ExecuteNonQueryAsync();
                Console.WriteLine("Lagerbewegung erfolgreich gespeichert.");

                // Aktualisiere die umlaufmenge im Ausgangslager anhand der Bestands-ID
                string updateQuery = @"UPDATE lagerbestaende 
                                       SET umlaufmenge = umlaufmenge + @menge 
                                       WHERE id = @bestandId";

                using var updateCmd = new MySqlCommand(updateQuery, connection);
                updateCmd.Parameters.AddWithValue("@menge", menge);
                updateCmd.Parameters.AddWithValue("@bestandId", artikelId); // artikelId repräsentiert hier die Bestands-ID

                int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    Console.WriteLine($"umlaufmenge erfolgreich aktualisiert: {rowsAffected} Zeile(n) betroffen.");
                }
                else
                {
                    Console.WriteLine("Warnung: Keine Zeilen wurden aktualisiert. Überprüfen Sie, ob die Kombination aus Lagername und Artikel-ID existiert.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Erstellen der Lagerbewegung oder Aktualisieren der umlaufmenge: {ex.Message}");
                throw;
            }
        }
    }
}