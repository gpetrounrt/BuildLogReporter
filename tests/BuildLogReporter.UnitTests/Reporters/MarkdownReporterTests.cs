using System.Collections.ObjectModel;
using BuildLogReporter.Processors;
using BuildLogReporter.Reporters;
using FluentAssertions;
using Xunit;

namespace BuildLogReporter.UnitTests.Reporters
{
    public sealed class MarkdownReporterTests
    {
        [Fact]
        public void GetReportAsString_WhenHavingValidProcessedLogResult_ShouldCreateCorrectMarkdown()
        {
            // Arrange
            ushort expectedErrorCount = 1;
            ushort expectedWarningCount = 2;

            ReadOnlyCollection<LogEntry> expectedLogEntries = new List<LogEntry>()
            {
                new LogEntry(
                    LogEntryType.Error,
                    "Code",
                    "Message",
                    "FilePath",
                    1),
                new LogEntry(
                    LogEntryType.Warning,
                    "Code",
                    "Message",
                    "FilePath",
                    2),
                new LogEntry(
                    LogEntryType.Warning,
                    "Code",
                    "Message",
                    "FilePath",
                    3),
            }
            .AsReadOnly();

            var processedLogResult = new ProcessedLogResult(
                expectedErrorCount,
                expectedWarningCount,
                expectedLogEntries);

            var jsonReporter = new MarkdownReporter();

            // Act
            var reportAsString = jsonReporter.GetReportAsString(processedLogResult);

            // Assert
            reportAsString.Should().Contain($"Errors: {expectedErrorCount}, Warnings: {expectedWarningCount}");
            reportAsString.Should().Contain("| Error | Code | Message | FilePath | 1 |");
            reportAsString.Should().Contain("| Warning | Code | Message | FilePath | 2 |");
        }
    }
}
