using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using BuildLogReporter.Processors;
using FluentAssertions;
using Xunit;

namespace BuildLogReporter.UnitTests.Processors
{
    public sealed class ProcessedLogResultTests
    {
        [Fact]
        public void WriteXml_WhenHavingCorrectPropertyValues_ShouldReturnExpectedResult()
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
                    1)
            }
            .AsReadOnly();

            var processedLogResult = new ProcessedLogResult(
                expectedErrorCount,
                expectedWarningCount,
                expectedLogEntries);

            var stringBuilder = new StringBuilder();
            using var xmlWriter = XmlWriter.Create(stringBuilder);
            xmlWriter.WriteStartElement(nameof(ProcessedLogResult));

            // Act
            processedLogResult.WriteXml(xmlWriter);

            xmlWriter.WriteEndElement();
            xmlWriter.Flush();

            // Assert
            stringBuilder.ToString().Should().Contain(@"<ProcessedLogResult ErrorCount=""1"" WarningCount=""2"">");
            stringBuilder.ToString().Should().Contain(@"<LogEntry Type=""Error"" Code=""Code"" Message=""Message"" FilePath=""FilePath"" LineNumber=""1"" />");
        }

        [Fact]
        public void Constructor_WhenInitialized_ShouldSetTheCorrectPropertyValues()
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

            // Act
            var processedLogResult = new ProcessedLogResult(
                expectedErrorCount,
                expectedWarningCount,
                expectedLogEntries);

            // Assert
            processedLogResult.ErrorCount.Should().Be(expectedErrorCount);
            processedLogResult.WarningCount.Should().Be(expectedWarningCount);
            processedLogResult.LogEntries.Should().Equal(expectedLogEntries);
        }
    }
}
