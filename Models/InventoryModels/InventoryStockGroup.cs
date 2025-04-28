namespace Percuro.Models
{
    public class InventoryStockGroup : BaseInventoryGroup
    {
        // Additional properties or methods specific to InventoryStockGroup can go here
    }

    // Spezielle Klasse für "Kein Lager" Option
    public class NoneInventoryStockGroup : InventoryStockGroup
    {
        public NoneInventoryStockGroup()
        {
            LagerName = "Kein Lager";
        }
    }
}