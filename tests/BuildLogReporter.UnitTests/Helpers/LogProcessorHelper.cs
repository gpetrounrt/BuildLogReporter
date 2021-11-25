using System.Diagnostics;

namespace BuildLogReporter.UnitTests.Helpers
{
    public static class LogProcessorHelper
    {
        public static string GetTestSupportProjectPath(string testProjectsDirectory, string projectName) =>
                  Path.GetFullPath(
                      Path.Combine(
                          testProjectsDirectory,
                          $@"TestSupportProjects\{projectName}\{projectName}.csproj"));

        public static void CreateLogFile(
            string projectPath,
            string logPath)
        {
            ArgumentNullException.ThrowIfNull(logPath);

            string logCommand = logPath.EndsWith(".binlog", System.StringComparison.OrdinalIgnoreCase) ? "-bl:" : "-fl -flp:logfile=";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                Arguments = $"build --no-incremental --configuration Release {logCommand}\"{logPath}\" \"{projectPath}\""
            };

            using var process = new Process
            {
                StartInfo = processStartInfo
            };

            process.Start();

            string outputText = process.StandardOutput.ReadToEnd();
            Debug.Assert(!string.IsNullOrWhiteSpace(outputText), "Process standard output text should not be null.");

            string errorText = process.StandardError.ReadToEnd();
            Debug.Assert(string.IsNullOrWhiteSpace(errorText), "Process standard error text should not be null.");

            process.WaitForExit();
        }
    }
}
