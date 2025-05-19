using System;

namespace Percuro.Models.MitarbeiterModels
{
    public class ZeitkontoModel
    {
        public int Id { get; set; }
        public int MitarbeiterId { get; set; }
        public string CheckType { get; set; } = string.Empty; 
        public DateTime CheckDateTime { get; set; }
        public string? CheckLocation { get; set; }
        public string MitarbeiterVorname { get; set; } = string.Empty;
        public string MitarbeiterNachname { get; set; } = string.Empty;
    }
}
