using System.Collections.ObjectModel;

namespace Percuro.Models
{
    public class TargetInventoryStock
    {
        public long Id { get; set; }
        public string ArtikelBezeichnung { get; set; } = string.Empty;
        public string? LagerName { get; set; }
        public int Bestand { get; set; }

        // Liste der zugehörigen Artikel
        public ObservableCollection<InventoryStock> Items { get; set; } = new();
    }
}