namespace CAFinal.ViewModels
{
    public class LoginViewModel
    {
        [Required, MaxLength(30)]
        public string? Username { get; set; }
        [Required, DataType(DataType.Password)]
        public string? Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
