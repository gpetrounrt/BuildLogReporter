using System.Collections.ObjectModel;
using BuildLogReporter.Processors;
using BuildLogReporter.Reporters;
using FluentAssertions;
using HtmlAgilityPack;
using Xunit;

namespace BuildLogReporter.UnitTests.Reporters
{
    public sealed class HtmlReporterTests
    {
        [Fact]
        public void GetReportAsString_WhenHavingValidProcessedLogResult_ShouldCreateCorrectHTML()
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

            var jsonReporter = new HtmlReporter("LogPath");

            // Act
            var reportAsString = jsonReporter.GetReportAsString(processedLogResult);

            var htmlDocument = new HtmlDocument();
            var exception = Record.Exception(() => htmlDocument.LoadHtml(reportAsString));

            // Assert
            reportAsString.Should().Contain("<p>Build report for 'LogPath'.</br>");
            reportAsString.Should().Contain($@"Errors:&nbsp;{expectedErrorCount},&nbsp;");
            reportAsString.Should().Contain($@"Warnings:&nbsp;{expectedWarningCount}");

            exception.Should().BeNull();
            htmlDocument.ParseErrors.Should().HaveCount(0);
        }
    }
}
