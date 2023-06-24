namespace CAFinal.Models
{
    public class Service : BaseEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }

        public bool IsDeleted { get; set; }
    }
}
