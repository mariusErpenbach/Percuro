namespace Percuro.Models.MitarbeiterModels
{
    public class MitarbeiterNameWithId
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public MitarbeiterNameWithId(string name, int id)
        {
            Name = name;
            Id = id;
        }
    }
}
