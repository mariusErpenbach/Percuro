using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using Percuro.Models.MitarbeiterModels;
using DotNetEnv;

namespace Percuro.Services.MitarbeiterServices;

public class MitarbeiterDatabaseService
{
    private readonly string _connectionString;

    public MitarbeiterDatabaseService()
    {
        // Load environment variables
        Env.Load();
        _connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION")
                                ?? throw new Exception("MYSQL_CONNECTION is not set.");
    }

    public async Task<List<Mitarbeiter>> GetAllMitarbeiterAsync()
    {
        var mitarbeiterList = new List<Mitarbeiter>();

        try
        {
            Console.WriteLine("Connecting to the database...");
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            Console.WriteLine("Database connection established.");

            string query = "SELECT id, vorname, nachname, geburtsdatum, eintrittsdatum, position_id, telefon, email, aktiv, gehalt, ist_admin, bild_url, notizen, adresse FROM mitarbeiter";
            Console.WriteLine("Executing query: " + query);
            using var cmd = new MySqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Console.WriteLine("Reading data for Mitarbeiter ID: " + reader.GetInt32(0));
                mitarbeiterList.Add(new Mitarbeiter
                {
                    Id = reader.GetInt32(0),
                    Vorname = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Nachname = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Geburtsdatum = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                    Eintrittsdatum = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                    PositionId = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                    Telefon = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Email = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Aktiv = reader.GetBoolean(8),
                    Gehalt = reader.IsDBNull(9) ? (decimal?)null : reader.GetDecimal(9),
                    IstAdmin = reader.GetBoolean(10),
                    BildUrl = reader.IsDBNull(11) ? null : reader.GetString(11),
                    Notizen = reader.IsDBNull(12) ? null : reader.GetString(12),
                    Adresse = reader.IsDBNull(13) ? null : reader.GetString(13)
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Abrufen der Mitarbeiterdaten: {ex.Message}");
        }

        Console.WriteLine("Total Mitarbeiter fetched: " + mitarbeiterList.Count);
        return mitarbeiterList;
    }
}