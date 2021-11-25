using System.Collections.ObjectModel;
using BuildLogReporter.Processors;
using BuildLogReporter.Reporters;
using FluentAssertions;
using Xunit;

namespace BuildLogReporter.UnitTests.Reporters
{
    public sealed class JsonReporterTests
    {
        [Fact]
        public void GetReportAsString_WhenHavingValidProcessedLogResult_ShouldCreateCorrectJSON()
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
                    1),
                new LogEntry(
                    LogEntryType.Warning,
                    "Code",
                    "Message",
                    "FilePath",
                    1),
            }
            .AsReadOnly();

            var processedLogResult = new ProcessedLogResult(
                expectedErrorCount,
                expectedWarningCount,
                expectedLogEntries);

            var jsonReporter = new JsonReporter();

            // Act
            var reportAsString = jsonReporter.GetReportAsString(processedLogResult);

            // Assert
            reportAsString.Should().Contain($@"""error_count"": {expectedErrorCount}");
            reportAsString.Should().Contain($@"""warning_count"": {expectedWarningCount}");
            reportAsString.Should().Contain($@"""log_entries"": [");
        }
    }
}
