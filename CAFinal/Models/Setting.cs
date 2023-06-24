namespace CAFinal.Models
{
    public class Setting
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Key { get; set; }
        [Required, MaxLength(300)]
        public string Value { get; set; }
    }
}
