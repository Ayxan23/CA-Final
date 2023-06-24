namespace CAFinal.Models
{
    public class Product :BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }

        public bool IsDeleted { get; set; }

        public ICollection<Basket> Baskets { get; set; }
        public ICollection<ProductCategory> ProductCategories { get; set; }
    }
}
