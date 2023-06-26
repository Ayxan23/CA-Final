namespace CAFinal.Models
{
    public class AppUser : IdentityUser
    {
        public string Fullname { get; set; }
        public bool IsActive { get; set; }
        public string? Address { get; set; }

        public ICollection<Basket> Baskets { get; set; }

    }
}
