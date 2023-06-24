namespace CAFinal.Areas.Admin.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [Required, Range(0.01, 1000.00)]
        public double Price { get; set; }
        public IFormFile? Image { get; set; }
        [Required, MaxLength(500)]
        public string Description { get; set; }

        public int[]? CategoryIds { get; set; }
    }
}
