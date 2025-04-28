using System.Collections.ObjectModel;

namespace Percuro.Models.InventoryModels
{
    public abstract class BaseInventoryGroup
    {
        public string LagerName { get; set; } = string.Empty;
        public ObservableCollection<InventoryStock> Items { get; set; } = new();
    }

    public class InventoryStockGroup : BaseInventoryGroup
    {
        // Additional properties or methods specific to InventoryStockGroup can go here
    }

    public class TargetInventoryStock : BaseInventoryGroup
    {
        public long Id { get; set; }
        public string ArtikelBezeichnung { get; set; } = string.Empty;
        public int Bestand { get; set; }
    }
}