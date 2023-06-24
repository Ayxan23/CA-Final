namespace CAFinal.Models
{
    public class About : BaseEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }

        public bool IsDeleted { get; set; }
    }
}
