using System.Collections.ObjectModel;
using BuildLogReporter.Processors;
using FluentAssertions;
using Xunit;

namespace BuildLogReporter.UnitTests.Processors
{
    public sealed class ProcessedLogResultTests
    {
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
