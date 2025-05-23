using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Percuro.Models.MitarbeiterModels;

namespace Percuro.Services.MitarbeiterServices
{
    public class ZeiterfassungsService
    {
        private readonly ZeiterfassungsDatabaseService _databaseService;

        public event Action? ZeitkontoCacheUpdated;

        private List<ZeitkontoModel> _zeitkontoCache = new List<ZeitkontoModel>();

        public ZeiterfassungsService()
        {
            _databaseService = new ZeiterfassungsDatabaseService();
        }

        public async Task<List<ZeitkontoModel>> GetMitarbeiterZeitkontoImZeitraumAsync(DateTime? startdatum, DateTime? enddatum)
        {
            _zeitkontoCache = await _databaseService.FetchMitarbeiterZeitkontoImZeitraumAsync(startdatum, enddatum);
            ZeitkontoCacheUpdated?.Invoke(); // Notify subscribers about the update
            return _zeitkontoCache;
        }

        public List<string> GetMitarbeiterNamenImZeitraum()
        {
            return _zeitkontoCache
                .Select(z => $"{z.MitarbeiterId:D2} {z.MitarbeiterVorname[0]}. {z.MitarbeiterNachname}")
                .OrderBy(name => name)
                .ToList();
        }

        public void ClearCache()
        {
            _zeitkontoCache.Clear();
        }

        public List<ZeitkontoModel> GetZeitkontoEntries()
        {
            return _zeitkontoCache;
        }

        public List<ZeitkontoModel> GetAllEntriesForDataGrid(int mitarbeiterId)
        {
            Console.WriteLine($"[GetAllEntriesForDataGrid] Suche nach MitarbeiterId: {mitarbeiterId}");
            Console.WriteLine($"[GetAllEntriesForDataGrid] _zeitkontoCache.Count: {_zeitkontoCache.Count}");
            foreach (var entry in _zeitkontoCache)
            {
                Console.WriteLine($"  [CacheEntry] Id: {entry.Id}, MitarbeiterId: {entry.MitarbeiterId}, CheckType: {entry.CheckType}, CheckDateTime: {entry.CheckDateTime}, CheckLocation: {entry.CheckLocation}, Vorname: {entry.MitarbeiterVorname}, Nachname: {entry.MitarbeiterNachname}");
            }
            var allIds = _zeitkontoCache.Select(e => e.MitarbeiterId).Distinct().ToList();
            Console.WriteLine($"[GetAllEntriesForDataGrid] IDs im Cache: {string.Join(", ", allIds)}");
            var result = _zeitkontoCache.Where(e => e.MitarbeiterId == mitarbeiterId).ToList();
            Console.WriteLine($"[GetAllEntriesForDataGrid] Treffer: {result.Count}");
            return result;
        }
    }
}
