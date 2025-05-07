using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using Percuro.Models.MitarbeiterModels;
using Percuro.Models;
using DotNetEnv;
using Percuro.Models.HRModels;

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

    public async Task<int> SaveAdressbuchAsync(Adressbuch adressbuch)
    {
        // Validate input fields
        if (string.IsNullOrWhiteSpace(adressbuch.Strasse) || string.IsNullOrWhiteSpace(adressbuch.Plz) || string.IsNullOrWhiteSpace(adressbuch.Stadt) || string.IsNullOrWhiteSpace(adressbuch.Land))
        {
            throw new ArgumentException("Die Felder Straße, PLZ, Stadt und Land dürfen nicht leer sein.");
        }

        var query = "INSERT INTO adressbuch (strasse, hausnummer, plz, stadt, land, adresszusatz, typ) VALUES (@strasse, @hausnummer, @plz, @stadt, @land, @adresszusatz, @typ); SELECT LAST_INSERT_ID();";

        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@strasse", adressbuch.Strasse);
            cmd.Parameters.AddWithValue("@hausnummer", adressbuch.Hausnummer);
            cmd.Parameters.AddWithValue("@plz", adressbuch.Plz);
            cmd.Parameters.AddWithValue("@stadt", adressbuch.Stadt);
            cmd.Parameters.AddWithValue("@land", adressbuch.Land);
            cmd.Parameters.AddWithValue("@adresszusatz", adressbuch.Adresszusatz);
            cmd.Parameters.AddWithValue("@typ", adressbuch.Typ);

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

    public async Task<List<string>> FetchPositionTitlesAsync()
    {
        var titles = new List<string>();

        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Titel FROM positionen";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                titles.Add(reader.GetString(0));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Abrufen der Positionstitel: {ex.Message}");
        }

        return titles;
    }

    public async Task<int?> GetPositionIdByTitleAsync(string title)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id FROM positionen WHERE Titel = @title";
        command.Parameters.AddWithValue("@title", title);

        var result = await command.ExecuteScalarAsync();
        return result != null ? Convert.ToInt32(result) : (int?)null;
    }

    public async Task<Adressbuch?> GetAdressbuchByMitarbeiterIdAsync(int mitarbeiterId)
    {
        Adressbuch? adressbuch = null;

        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT a.strasse, a.hausnummer, a.plz, a.stadt, a.land, a.adresszusatz, a.typ
                          FROM adressbuch a
                          INNER JOIN mitarbeiter m ON m.adressbuch_id = a.id
                          WHERE m.id = @mitarbeiterId";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@mitarbeiterId", mitarbeiterId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                adressbuch = new Adressbuch
                {
                    Strasse = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                    Hausnummer = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Plz = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    Stadt = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Land = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Adresszusatz = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Typ = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Abrufen des Adressbuchs: {ex.Message}");
        }

        return adressbuch;
    }

    public async Task UpdateMitarbeiterAsync(Mitarbeiter mitarbeiter)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"UPDATE mitarbeiter
                          SET vorname = @vorname, nachname = @nachname, geburtsdatum = @geburtsdatum, eintrittsdatum = @eintrittsdatum,
                              telefon = @telefon, email = @email, position_id = @position_id
                          WHERE id = @id";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@vorname", mitarbeiter.Vorname);
            cmd.Parameters.AddWithValue("@nachname", mitarbeiter.Nachname);
            cmd.Parameters.AddWithValue("@geburtsdatum", mitarbeiter.Geburtsdatum);
            cmd.Parameters.AddWithValue("@eintrittsdatum", mitarbeiter.Eintrittsdatum);
            cmd.Parameters.AddWithValue("@telefon", mitarbeiter.Telefon);
            cmd.Parameters.AddWithValue("@email", mitarbeiter.Email);
            cmd.Parameters.AddWithValue("@position_id", mitarbeiter.PositionId);
            cmd.Parameters.AddWithValue("@id", mitarbeiter.Id);

            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Aktualisieren des Mitarbeiters: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateAdressbuchAsync(Adressbuch adressbuch)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"UPDATE adressbuch
                          SET strasse = @strasse, hausnummer = @hausnummer, plz = @plz, stadt = @stadt, land = @land
                          WHERE id = @id";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@strasse", adressbuch.Strasse);
            cmd.Parameters.AddWithValue("@hausnummer", adressbuch.Hausnummer);
            cmd.Parameters.AddWithValue("@plz", adressbuch.Plz);
            cmd.Parameters.AddWithValue("@stadt", adressbuch.Stadt);
            cmd.Parameters.AddWithValue("@land", adressbuch.Land);
            cmd.Parameters.AddWithValue("@id", adressbuch.Id);

            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Aktualisieren des Adressbuchs: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteMitarbeiterAsync(int mitarbeiterId)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "DELETE FROM mitarbeiter WHERE id = @id";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", mitarbeiterId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine(rowsAffected > 0
                ? $"Mitarbeiter with ID {mitarbeiterId} has been successfully deleted."
                : $"No Mitarbeiter found with ID {mitarbeiterId}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting Mitarbeiter with ID {mitarbeiterId}: {ex.Message}");
            throw;
        }
    }
}