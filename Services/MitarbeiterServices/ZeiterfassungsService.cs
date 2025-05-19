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
        private List<ZeitkontoModel> _zeitkontoCache = new List<ZeitkontoModel>();

        public ZeiterfassungsService()
        {
            _databaseService = new ZeiterfassungsDatabaseService();
        }

        public async Task<List<ZeitkontoModel>> GetMitarbeiterZeitkontoImZeitraumAsync(DateTime? startdatum, DateTime? enddatum)
        {
            _zeitkontoCache = await _databaseService.FetchMitarbeiterZeitkontoImZeitraumAsync(startdatum, enddatum);
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
    }
}
