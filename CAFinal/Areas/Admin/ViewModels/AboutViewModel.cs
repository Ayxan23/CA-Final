namespace CAFinal.Areas.Admin.ViewModels
{
    public class AboutViewModel
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Title { get; set; }
        [Required, MaxLength(1000)]
        public string Description { get; set; }
        public IFormFile? Image { get; set; }
    }
}
