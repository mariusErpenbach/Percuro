using System;

namespace Percuro.Models.InventoryModels
{
    public class StorageLocation
    {
        public int Id { get; set; }
        public DateTime? Erstellungsdatum { get; set; } = DateTime.Now;
        public string Name { get; set; } = string.Empty;
        public string? Beschreibung { get; set; }
        public string? Standort { get; set; }
        public int? Kapazitaet { get; set; }
        public string AktiverStatus { get; set; } = "aktiv";
    }
}