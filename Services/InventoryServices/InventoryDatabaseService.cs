using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;
using Percuro.Models;
using DotNetEnv;
using System.Data;

namespace Percuro.Services.InventoryServices
{
    // Handles database operations related to inventory, such as fetching stocks and creating stock movements.
    public class InventoryDatabaseService
    {
        private readonly string _connectionString;

        public InventoryDatabaseService()
        {
            // Load environment variables
            Env.Load();
            _connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION")
                                ?? throw new Exception("MYSQL_CONNECTION is not set.");
        }

        // Retrieves all inventory stocks from the database.
        public async Task<List<InventoryStock>> GetInventoryStocksAsync()
        {
            var inventoryStocks = new List<InventoryStock>();

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = "SELECT id, artikel_id, artikel_bezeichnung, bestand, mindestbestand, platzbezeichnung, letzte_aenderung, lager_name, umlaufmenge FROM lagerbestaende";
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
                        ArtikelBezeichnung = reader.IsDBNull("artikel_bezeichnung") ? null : reader.GetString("artikel_bezeichnung"),
                        Umlaufmenge = reader.GetInt32("umlaufmenge")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Abrufen der Lagerbestände: {ex.Message}");
            }

            return inventoryStocks;
        }

        public async Task<InventoryStock?> GetInventoryStockByIdAsync(long id)
        {
            // Simulate fetching the inventory stock from the database
            var inventoryStocks = await GetInventoryStocksAsync();
            return inventoryStocks.FirstOrDefault(stock => stock.Id == id);
        }

        // Transfers stock to a target warehouse by updating the database.
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

        // Creates a stock movement record in the database and updates stock quantities.
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

                string updateQuery = @"UPDATE lagerbestaende 
                                       SET umlaufmenge = umlaufmenge + @menge 
                                       WHERE id = @bestandId";

                using var updateCmd = new MySqlCommand(updateQuery, connection);
                // Ensure correct data types are used for parameters in AddWithValue calls
                updateCmd.Parameters.AddWithValue("@menge", menge);
                updateCmd.Parameters.AddWithValue("@bestandId", artikelId); // Ensure artikelId is an integer

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

        public async Task UpdateInventoryStockAsync(BestandskorrekturModel correctionDetails)
        {
            if (correctionDetails == null)
            {
                Console.WriteLine("[UpdateInventoryStockAsync] Correction details are null.");
                throw new ArgumentNullException(nameof(correctionDetails), "Correction details cannot be null.");
            }

            try
            {
                Console.WriteLine("[UpdateInventoryStockAsync] Starting update for ID: " + correctionDetails.Id);

                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                Console.WriteLine("[UpdateInventoryStockAsync] Database connection opened.");

                string query = @"UPDATE lagerbestaende 
                                 SET bestand = @bestand, 
                                     mindestbestand = @mindestbestand, 
                                     platzbezeichnung = @platzbezeichnung, 
                                     artikel_bezeichnung = @artikelBezeichnung, 
                                     umlaufmenge = @umlaufmenge, 
                                     letzte_aenderung = NOW() 
                                 WHERE id = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@bestand", correctionDetails.Bestand);
                cmd.Parameters.AddWithValue("@mindestbestand", correctionDetails.Mindestbestand ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@platzbezeichnung", correctionDetails.Platzbezeichnung ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@artikelBezeichnung", correctionDetails.ArtikelBezeichnung ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@umlaufmenge", correctionDetails.Umlaufmenge);
                cmd.Parameters.AddWithValue("@id", correctionDetails.Id);

                Console.WriteLine("[UpdateInventoryStockAsync] Executing query: " + query);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                Console.WriteLine("[UpdateInventoryStockAsync] Rows affected: " + rowsAffected);

                if (rowsAffected == 0)
                {
                    Console.WriteLine("[UpdateInventoryStockAsync] No rows were updated. The inventory stock might not exist.");
                    throw new Exception("No rows were updated. The inventory stock might not exist.");
                }

                Console.WriteLine("[UpdateInventoryStockAsync] Inventory stock successfully updated.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateInventoryStockAsync] Error: {ex.Message}");
                throw;
            }
        }
    }
}