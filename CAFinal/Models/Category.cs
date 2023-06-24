namespace CAFinal.Models
{
    public class Category : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }

        public bool IsDeleted { get; set; }

        public ICollection<ProductCategory> ProductCategories { get; set; }
    }
}
