namespace CAFinal.Models
{
    public class Basket : BaseEntity
    {
       
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        
        public int Count { get; set; }
        public bool IsPay { get; set; }
    }
}
