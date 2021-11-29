namespace BuildLogReporter.IntegrationTests.Helpers
{
    public static class ReportHelper
    {
        public static string ReadBaselineFile(string path, string actualText)
        {
            var parentDirectory = Path.GetDirectoryName(path);
            if (parentDirectory is null)
            {
                throw new DirectoryNotFoundException(nameof(path));
            }

            if (!Directory.Exists(parentDirectory))
            {
                Directory.CreateDirectory(parentDirectory);
            }

            if (!File.Exists(path))
            {
                File.WriteAllText(path, actualText);

                throw new FileNotFoundException(path);
            }

            return File.ReadAllText(path);
        }
    }
}
