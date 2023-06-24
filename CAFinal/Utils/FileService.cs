namespace CAFinal.Utils
{
    public static class FileService
    {
        public static void DeleteFile(params string[] path)
        {
            string resultPath = Path.Combine(path);
            if (System.IO.File.Exists(resultPath))
            {
                System.IO.File.Delete(resultPath);
            }
        }
    }
}
