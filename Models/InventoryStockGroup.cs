using System.Collections.ObjectModel;

namespace Percuro.Models
{
    public class InventoryStockGroup
    {
        public string LagerName { get; set; } = string.Empty;
        public ObservableCollection<InventoryStock> Items { get; set; } = new();
    }
}