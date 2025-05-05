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

            string query = @"SELECT m.id, m.vorname, m.nachname, m.geburtsdatum, m.eintrittsdatum, m.position_id, p.titel, 
                                    m.telefon, m.email, m.aktiv, m.gehalt, m.ist_admin, m.bild_url, m.notizen, 
                                    a.strasse, a.stadt, a.plz, a.land
                             FROM mitarbeiter m
                             LEFT JOIN positionen p ON m.position_id = p.id
                             LEFT JOIN adressbuch a ON m.adressbuch_id = a.id";
            Console.WriteLine("Executing query: " + query);
            using var cmd = new MySqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                mitarbeiterList.Add(new Mitarbeiter
                {
                    Id = reader.GetInt32(0),
                    Vorname = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Nachname = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Geburtsdatum = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                    Eintrittsdatum = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                    PositionId = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                    PositionTitel = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Telefon = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Email = reader.IsDBNull(8) ? null : reader.GetString(8),
                    Aktiv = reader.GetBoolean(9),
                    Gehalt = reader.IsDBNull(10) ? (decimal?)null : reader.GetDecimal(10),
                    IstAdmin = reader.GetBoolean(11),
                    BildUrl = reader.IsDBNull(12) ? null : reader.GetString(12),
                    Notizen = reader.IsDBNull(13) ? null : reader.GetString(13),
                    Strasse = reader.IsDBNull(14) ? null : reader.GetString(14),
                    Stadt = reader.IsDBNull(15) ? null : reader.GetString(15),
                    PLZ = reader.IsDBNull(16) ? null : reader.GetString(16),
                    Land = reader.IsDBNull(17) ? null : reader.GetString(17)
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

    public async Task<int> SaveAdressbuchAsync(string? strasse, string? hausnummer, string? plz, string? stadt, string? land, string? adresszusatz, string? typ)
    {
        var query = "INSERT INTO adressbuch (strasse, hausnummer, plz, stadt, land, adresszusatz, typ) VALUES (@strasse, @hausnummer, @plz, @stadt, @land, @adresszusatz, @typ); SELECT LAST_INSERT_ID();";

        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@strasse", strasse);
            cmd.Parameters.AddWithValue("@hausnummer", hausnummer);
            cmd.Parameters.AddWithValue("@plz", plz);
            cmd.Parameters.AddWithValue("@stadt", stadt);
            cmd.Parameters.AddWithValue("@land", land);
            cmd.Parameters.AddWithValue("@adresszusatz", adresszusatz);
            cmd.Parameters.AddWithValue("@typ", typ);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Speichern des Adressbuch-Eintrags: {ex.Message}");
            throw;
        }
    }

    public async Task AddMitarbeiterAsync(Mitarbeiter mitarbeiter)
    {
        var query = "INSERT INTO mitarbeiter (vorname, nachname, geburtsdatum, eintrittsdatum, position_id, telefon, email, aktiv, gehalt, ist_admin, bild_url, notizen, adressbuch_id) " +
                    "VALUES (@vorname, @nachname, @geburtsdatum, @eintrittsdatum, @position_id, @telefon, @email, @aktiv, @gehalt, @ist_admin, @bild_url, @notizen, @adressbuch_id);";

        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@vorname", mitarbeiter.Vorname);
            cmd.Parameters.AddWithValue("@nachname", mitarbeiter.Nachname);
            cmd.Parameters.AddWithValue("@geburtsdatum", mitarbeiter.Geburtsdatum);
            cmd.Parameters.AddWithValue("@eintrittsdatum", mitarbeiter.Eintrittsdatum);
            cmd.Parameters.AddWithValue("@position_id", mitarbeiter.PositionId);
            cmd.Parameters.AddWithValue("@telefon", mitarbeiter.Telefon);
            cmd.Parameters.AddWithValue("@email", mitarbeiter.Email);
            cmd.Parameters.AddWithValue("@aktiv", mitarbeiter.Aktiv);
            cmd.Parameters.AddWithValue("@gehalt", mitarbeiter.Gehalt);
            cmd.Parameters.AddWithValue("@ist_admin", mitarbeiter.IstAdmin);
            cmd.Parameters.AddWithValue("@bild_url", mitarbeiter.BildUrl);
            cmd.Parameters.AddWithValue("@notizen", mitarbeiter.Notizen);
            cmd.Parameters.AddWithValue("@adressbuch_id", mitarbeiter.AdressbuchId);

            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Hinzufügen eines Mitarbeiters: {ex.Message}");
            throw;
        }
    }
}