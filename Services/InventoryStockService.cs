using System;
using System.Collections.Generic;
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

                string query = "SELECT id, artikel_id, bestand, mindestbestand, platzbezeichnung, letzte_aenderung, lager_name FROM lagerbestaende";
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
                        LagerName = reader.IsDBNull("lager_name") ? null : reader.GetString("lager_name")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Abrufen der Lagerbestände: {ex.Message}");
            }

            return inventoryStocks;
        }
    }
}