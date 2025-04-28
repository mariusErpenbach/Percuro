namespace Percuro.Models
{
    public class TargetInventoryStock : BaseInventoryGroup
    {
        public long Id { get; set; }
        public string ArtikelBezeichnung { get; set; } = string.Empty;
        public int Bestand { get; set; }
    }
}