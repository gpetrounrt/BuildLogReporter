using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using BuildLogReporter.Processors;
using BuildLogReporter.Reporters;
using FluentAssertions;
using Xunit;

namespace BuildLogReporter.UnitTests.Reporters
{
    public sealed class BadgeReporterTests
    {
        [Fact]
        public void GetReportAsString_WhenHavingValidProcessedLogResult_ShouldCreateCorrectSVG()
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

            var badgeReporter = new BadgeReporter();

            // Act
            var reportAsString = badgeReporter.GetReportAsString(processedLogResult);
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(reportAsString));
            var xmlDocument = new XmlDocument();
            var exception = Record.Exception(() => xmlDocument.Load(memoryStream));

            // Assert
            reportAsString.Should().Contain($@"{expectedErrorCount} error, {expectedWarningCount} warnings");
            reportAsString.Should().Contain($@"<path fill=""#E05D44"" d=""M29.392578 0h107.063965v20H29.392578z"" />");
            exception.Should().BeNull();
        }
    }
}
