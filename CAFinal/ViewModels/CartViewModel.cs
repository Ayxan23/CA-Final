namespace CAFinal.ViewModels
{
    public class CartViewModel
    {
        [Required]
        public long CardNumber { get; set; }
        [Required]
        public int Year { get; set; }
        [Required]
        public int Month { get; set; }
        [Required]
        public int Cvv { get; set; }
    }
}
