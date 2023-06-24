namespace CAFinal.Utils
{
    public static class Extension
    {
        public static bool CheckFileSize(this IFormFile file, int size)
        => file.Length / 1024 < size;

        public static bool CheckFileType(this IFormFile file, string fileType)
        => file.ContentType.Contains($"{fileType}/");
    }
}
