using System.Text.RegularExpressions;
using BuildLogReporter.IntegrationTests.Fixtures;
using BuildLogReporter.IntegrationTests.Helpers;
using BuildLogReporter.Tests.Common.Diagnostics;
using BuildLogReporter.Tests.Common.Helpers;
using FluentAssertions;
using Xunit;

namespace BuildLogReporter.IntegrationTests
{
    public sealed class ProgramTests : IClassFixture<ProgramFixture>
    {
        private readonly ProgramFixture _programFixture;

        private static string GetProjectBuiltPath(string testProjectsBuiltDirectory, string projectName) =>
           Path.GetFullPath(
               Path.Combine(
                   testProjectsBuiltDirectory,
                   $"{projectName}"));

        private static string GetBinlogPath(string projectBuiltPath) =>
               Path.Combine(
                   projectBuiltPath,
                   "output.binlog");

        private static string GetReportPath(string projectBuiltPath, string reportExtension) =>
             Path.Combine(
                 projectBuiltPath,
                 $"BuildLogReport.{reportExtension}");

        private static string GetBaselineDirectory(string testProjectsDirectory, string projectName) =>
              Path.Combine(
                  testProjectsDirectory,
                  @"BuildLogReporter.IntegrationTests\Baseline",
                  projectName);

        private static string GetBaselineReportPath(string baselineDirectory, string reportExtension) =>
              Path.Combine(
                  baselineDirectory,
                  $"BuildLogReport.{reportExtension}");

        [Theory]
        [InlineData("ProjectWithErrors")]
        [InlineData("ProjectWithErrorsAndWarnings")]
        [InlineData("ProjectWithoutErrorsOrWarnings")]
        [InlineData("ProjectWithWarnings")]
        public async Task Main_WhenHavingValidOptions_ShouldGenerateExpectedReports(string projectName)
        {
            // Arrange
            using var consoleRecorder = new ConsoleRecorder();
            string projectPath = LogProcessorHelper.GetTestSupportProjectPath(_programFixture.TestProjectsDirectory, projectName);
            string projectBuiltPath = GetProjectBuiltPath(_programFixture.TestProjectsBuiltDirectory, projectName);
            string projectBinlogPath = GetBinlogPath(projectBuiltPath);
            LogProcessorHelper.CreateLogFile(projectPath, projectBinlogPath);

            string badgeReportPath = GetReportPath(projectBuiltPath, "svg");
            string htmlReportPath = GetReportPath(projectBuiltPath, "htm");
            string jsonReportPath = GetReportPath(projectBuiltPath, "json");
            string markdownReportPath = GetReportPath(projectBuiltPath, "md");
            string xmlReportPath = GetReportPath(projectBuiltPath, "xml");

            string baselineDirectory = GetBaselineDirectory(_programFixture.TestProjectsDirectory, projectName);

            string consoleBaselineOutputPath = GetBaselineReportPath(baselineDirectory, "out");
            string badgeBaselineReportPath = GetBaselineReportPath(baselineDirectory, "svg");
            string htmlBaselineReportPath = GetBaselineReportPath(baselineDirectory, "htm");
            string jsonBaselineReportPath = GetBaselineReportPath(baselineDirectory, "json");
            string markdownBaselineReportPath = GetBaselineReportPath(baselineDirectory, "md");
            string xmlBaselineReportPath = GetBaselineReportPath(baselineDirectory, "xml");

            // Act
            var result = await Program.Main(new string[] { "--report-types", "Badge;Html;Json;Markdown;Xml", "--verbose", projectBinlogPath, projectBuiltPath }).ConfigureAwait(false);

            var consoleOutput = Regex.Replace(Regex.Replace(consoleRecorder.GetOutput(), @"\d+\.?\d* (milliseconds|seconds)", "elapsed time"), @"\d+ record", "N record")
                .Replace(_programFixture.TestProjectsBuiltDirectory, string.Empty, StringComparison.Ordinal)
                .Replace(_programFixture.TestProjectsDirectory, string.Empty, StringComparison.Ordinal);

            string consoleOutputBaselineText = ReportHelper.ReadBaselineFile(consoleBaselineOutputPath, consoleOutput);

            string badgeText = File.ReadAllText(badgeReportPath);
            string badgeBaselineReportText = ReportHelper.ReadBaselineFile(badgeBaselineReportPath, badgeText);

            string htmlText = File.ReadAllText(htmlReportPath)
                .Replace(_programFixture.TestProjectsBuiltDirectory, string.Empty, StringComparison.Ordinal)
                .Replace(_programFixture.TestProjectsDirectory, string.Empty, StringComparison.Ordinal);
            string htmlBaselineReportText = ReportHelper.ReadBaselineFile(htmlBaselineReportPath, htmlText);

            string jsonText = File.ReadAllText(jsonReportPath)
                .Replace(_programFixture.TestProjectsBuiltDirectory.Replace(@"\", @"\\", StringComparison.Ordinal), string.Empty, StringComparison.Ordinal)
                .Replace(_programFixture.TestProjectsDirectory.Replace(@"\", @"\\", StringComparison.Ordinal), string.Empty, StringComparison.Ordinal);
            string jsonBaselineReportText = ReportHelper.ReadBaselineFile(jsonBaselineReportPath, jsonText);

            string markdownText = File.ReadAllText(markdownReportPath)
                .Replace(_programFixture.TestProjectsBuiltDirectory, string.Empty, StringComparison.Ordinal)
                .Replace(_programFixture.TestProjectsDirectory, string.Empty, StringComparison.Ordinal);
            string markdownBaselineReportText = ReportHelper.ReadBaselineFile(markdownBaselineReportPath, markdownText);

            string xmlText = File.ReadAllText(xmlReportPath)
                .Replace(_programFixture.TestProjectsBuiltDirectory, string.Empty, StringComparison.Ordinal)
                .Replace(_programFixture.TestProjectsDirectory, string.Empty, StringComparison.Ordinal);
            string xmlBaselineReportText = ReportHelper.ReadBaselineFile(xmlBaselineReportPath, xmlText);

            // Assert
            result.Should().Be(0);
            consoleOutput.Should().Be(consoleOutputBaselineText);
            consoleRecorder.GetError().Should().BeEmpty();

            badgeText.Should().Be(badgeBaselineReportText);
            htmlText.Should().Be(htmlBaselineReportText);
            jsonText.Should().Be(jsonBaselineReportText);
            markdownText.Should().Be(markdownBaselineReportText);
            xmlText.Should().Be(xmlBaselineReportText);
        }

        public ProgramTests(ProgramFixture programFixture) =>
            _programFixture = programFixture;
    }
}
