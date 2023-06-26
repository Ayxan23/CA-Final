namespace CAFinal.ViewModels
{
    public class AddressViewModel
    {
        [Required, MaxLength(100)]
        public string City { get; set; }
        [Required, MaxLength(300)]
        public string Address { get; set; }
        [Required, MaxLength(20)]
        public string Postcode { get; set; }
        [Required, MaxLength(40)]
        public string Home { get; set; }
        [Required, DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
    }
}
