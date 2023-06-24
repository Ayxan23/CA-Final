namespace CAFinal.Areas.Admin.ViewModels
{
    public class ContactViewModel
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Title { get; set; }
        [Required, MaxLength(100)]
        public string Description { get; set; }
        [Required, MaxLength(100)]
        public string Icon { get; set; }
    }
}
