using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using Percuro.Models;
using DotNetEnv;
using System.Data;
using Percuro.Models.InventoryModels;

namespace Percuro.Services.InventoryServices
{
    public class StorageLocationService
    {
        private readonly string _connectionString;

        public StorageLocationService()
        {
            // Load environment variables
            Env.Load();
            _connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION")
                                ?? throw new Exception("MYSQL_CONNECTION is not set.");
        }

        public async Task<List<StorageLocation>> GetStorageLocationsAsync()
        {
            var storageLocations = new List<StorageLocation>();

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = "SELECT id, erstellungsdatum, name, beschreibung, standort, kapazitaet, aktiver_status FROM lagerorte";
                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    storageLocations.Add(new StorageLocation
                    {
                        Id = reader.GetInt32("id"),
                        Erstellungsdatum = reader.GetDateTime("erstellungsdatum"),
                        Name = reader.GetString("name"),
                        Beschreibung = reader.GetString("beschreibung"),
                        Standort = reader.GetString("standort"),
                        Kapazitaet = reader.GetInt32("kapazitaet"),
                        AktiverStatus = reader.GetString("aktiver_status")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Abrufen der Lagerorte: {ex.Message}");
            }

            return storageLocations;
        }

    }
}