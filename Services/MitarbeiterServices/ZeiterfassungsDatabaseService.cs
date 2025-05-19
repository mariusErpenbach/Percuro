using Percuro.Models.MitarbeiterModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using Percuro.Services;
using System.Data;
using System.Linq;

namespace Percuro.Services.MitarbeiterServices
{
    public class ZeiterfassungsDatabaseService
    {
        private readonly string _connectionString;

        public ZeiterfassungsDatabaseService()
        {
            var dbService = new DatabaseService();
            _connectionString = dbService.GetType()
                .GetField("_connectionString", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(dbService)?.ToString()
                ?? throw new Exception("ConnectionString konnte nicht geladen werden.");
        }

        public async Task<List<ZeitkontoModel>> LadeAlleEintraegeAsync()
        {
            var result = new List<ZeitkontoModel>();
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                string query = "SELECT id, mitarbeiter_id, check_type, check_datetime, check_location FROM zeitkonto";
                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new ZeitkontoModel
                    {
                        Id = reader.GetInt32("id"),
                        MitarbeiterId = reader.GetInt32("mitarbeiter_id"),
                        CheckType = reader.GetString("check_type"),
                        CheckDateTime = reader.GetDateTime("check_datetime"),
                        CheckLocation = reader.IsDBNull("check_location") ? null : reader.GetString("check_location")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Laden der Zeitkonto-Einträge: {ex.Message}");
            }
            return result;
        }

        public async Task<List<ZeitkontoModel>> LadeEintraegeImZeitraumAsync(DateTime? startdatum = null, DateTime? enddatum = null)
        {
            var result = new List<ZeitkontoModel>();
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                string query = "SELECT id, mitarbeiter_id, check_type, check_datetime, check_location FROM zeitkonto WHERE 1=1";
                if (startdatum.HasValue)
                    query += " AND check_datetime >= @startdatum";
                if (enddatum.HasValue)
                    query += " AND check_datetime <= @enddatum";
                using var cmd = new MySqlCommand(query, connection);
                if (startdatum.HasValue)
                    cmd.Parameters.AddWithValue("@startdatum", startdatum.Value);
                if (enddatum.HasValue)
                    cmd.Parameters.AddWithValue("@enddatum", enddatum.Value);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new ZeitkontoModel
                    {
                        Id = reader.GetInt32("id"),
                        MitarbeiterId = reader.GetInt32("mitarbeiter_id"),
                        CheckType = reader.GetString("check_type"),
                        CheckDateTime = reader.GetDateTime("check_datetime"),
                        CheckLocation = reader.IsDBNull("check_location") ? null : reader.GetString("check_location")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Laden der Zeitkonto-Einträge: {ex.Message}");
            }
            return result;
        }

        public async Task<List<Mitarbeiter>> LadeMitarbeiterMitZeitkontoAsync()
        {
            var mitarbeiterList = new List<Mitarbeiter>();
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                // Hole alle eindeutigen Mitarbeiter-IDs aus zeitkonto
                string idQuery = "SELECT DISTINCT mitarbeiter_id FROM zeitkonto";
                var ids = new List<int>();
                using (var idCmd = new MySqlCommand(idQuery, connection))
                using (var reader = await idCmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        ids.Add(reader.GetInt32("mitarbeiter_id"));
                    }
                }
                if (ids.Count == 0)
                    return mitarbeiterList;
                // Hole alle Mitarbeiter mit diesen IDs
                string mitarbeiterQuery = $"SELECT id, vorname, nachname FROM mitarbeiter WHERE id IN ({string.Join(",", ids)})";
                using (var mitarbeiterCmd = new MySqlCommand(mitarbeiterQuery, connection))
                using (var mitarbeiterReader = await mitarbeiterCmd.ExecuteReaderAsync())
                {
                    while (await mitarbeiterReader.ReadAsync())
                    {
                        mitarbeiterList.Add(new Mitarbeiter
                        {
                            Id = mitarbeiterReader.GetInt32("id"),
                            Vorname = mitarbeiterReader.GetString("vorname"),
                            Nachname = mitarbeiterReader.GetString("nachname")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Laden der Mitarbeiter mit Zeitkonto: {ex.Message}");
            }
            return mitarbeiterList;
        }

        public async Task<List<ZeitkontoModel>> LadeZeitKontoDaten(
            DateTime? startdatum = null,
            DateTime? enddatum = null,
            int? mitarbeiterId = null,
            string? checkType = null,
            string? location = null)
        {
            var result = new List<ZeitkontoModel>();
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                string query = "SELECT id, mitarbeiter_id, check_type, check_datetime, check_location FROM zeitkonto WHERE 1=1";
                if (startdatum.HasValue)
                    query += " AND check_datetime >= @startdatum";
                if (enddatum.HasValue)
                    query += " AND check_datetime <= @enddatum";
                if (mitarbeiterId.HasValue)
                    query += " AND mitarbeiter_id = @mitarbeiterId";
                if (!string.IsNullOrWhiteSpace(checkType))
                    query += " AND check_type = @checkType";
                if (!string.IsNullOrWhiteSpace(location))
                    query += " AND check_location = @location";
                using var cmd = new MySqlCommand(query, connection);
                if (startdatum.HasValue)
                    cmd.Parameters.AddWithValue("@startdatum", startdatum.Value);
                if (enddatum.HasValue)
                    cmd.Parameters.AddWithValue("@enddatum", enddatum.Value);
                if (mitarbeiterId.HasValue)
                    cmd.Parameters.AddWithValue("@mitarbeiterId", mitarbeiterId.Value);
                if (!string.IsNullOrWhiteSpace(checkType))
                    cmd.Parameters.AddWithValue("@checkType", checkType);
                if (!string.IsNullOrWhiteSpace(location))
                    cmd.Parameters.AddWithValue("@location", location);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new ZeitkontoModel
                    {
                        Id = reader.GetInt32("id"),
                        MitarbeiterId = reader.GetInt32("mitarbeiter_id"),
                        CheckType = reader.GetString("check_type"),
                        CheckDateTime = reader.GetDateTime("check_datetime"),
                        CheckLocation = reader.IsDBNull("check_location") ? null : reader.GetString("check_location")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Laden der Zeitkonto-Daten: {ex.Message}");
            }
            return result;
        }

        public async Task<List<Mitarbeiter>> GetMitarbeiterImZeitraumAsync(DateTime? startdatum = null, DateTime? enddatum = null)
        {
            var mitarbeiterList = new List<Mitarbeiter>();
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                string query = @"SELECT DISTINCT m.id, m.vorname, m.nachname
                                 FROM zeitkonto z
                                 JOIN mitarbeiter m ON z.mitarbeiter_id = m.id
                                 WHERE 1=1";
                if (startdatum.HasValue)
                    query += " AND z.check_datetime >= @startdatum";
                if (enddatum.HasValue)
                    query += " AND z.check_datetime <= @enddatum";
                using var cmd = new MySqlCommand(query, connection);
                if (startdatum.HasValue)
                    cmd.Parameters.AddWithValue("@startdatum", startdatum.Value);
                if (enddatum.HasValue)
                    cmd.Parameters.AddWithValue("@enddatum", enddatum.Value);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    mitarbeiterList.Add(new Mitarbeiter
                    {
                        Id = reader.GetInt32("id"),
                        Vorname = reader.GetString("vorname"),
                        Nachname = reader.GetString("nachname")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Laden der Mitarbeiter im Zeitraum: {ex.Message}");
            }
            return mitarbeiterList;
        }

        public async Task<List<ZeitkontoModel>> FetchMitarbeiterZeitkontoImZeitraumAsync(DateTime? startdatum = null, DateTime? enddatum = null)
        {
            var result = new List<ZeitkontoModel>();
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                string query = @"SELECT z.id, z.mitarbeiter_id, z.check_type, z.check_datetime, z.check_location, m.vorname, m.nachname
                                 FROM zeitkonto z
                                 JOIN mitarbeiter m ON z.mitarbeiter_id = m.id
                                 WHERE 1=1";
                if (startdatum.HasValue)
                    query += " AND z.check_datetime >= @startdatum";
                if (enddatum.HasValue)
                    query += " AND z.check_datetime <= @enddatum";
                using var cmd = new MySqlCommand(query, connection);
                if (startdatum.HasValue)
                    cmd.Parameters.AddWithValue("@startdatum", startdatum.Value);
                if (enddatum.HasValue)
                    cmd.Parameters.AddWithValue("@enddatum", enddatum.Value);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new ZeitkontoModel
                    {
                        Id = reader.GetInt32("id"),
                        MitarbeiterId = reader.GetInt32("mitarbeiter_id"),
                        CheckType = reader.GetString("check_type"),
                        CheckDateTime = reader.GetDateTime("check_datetime"),
                        CheckLocation = reader.IsDBNull("check_location") ? null : reader.GetString("check_location"),
                        MitarbeiterVorname = reader.GetString("vorname"),
                        MitarbeiterNachname = reader.GetString("nachname")
                    });
                }
                Console.WriteLine($"Query Parameters: Startdatum = {startdatum}, Enddatum = {enddatum}");
                Console.WriteLine($"Fetched {result.Count} records from the database.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Laden der Zeitkonto-Daten: {ex.Message}");
            }
            return result;
        }
    }
}
