using BuildLogReporter.Processors;
using BuildLogReporter.UnitTests.Diagnostics;
using BuildLogReporter.UnitTests.Fixtures;
using BuildLogReporter.UnitTests.Helpers;
using FluentAssertions;
using Xunit;

namespace BuildLogReporter.UnitTests.Processors
{
    [Collection(nameof(LogProcessorCollectionFixture))]
    public sealed class BinaryLogProcessorTests
    {
        private readonly LogProcessorFixture _logProcessorFixture;

        private static string GetBinlogPath(string testProjectsBuiltDirectory, string projectName) =>
            Path.GetFullPath(
                Path.Combine(
                    testProjectsBuiltDirectory,
                    $@"{projectName}\output.binlog"));

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CountAndGetErrorsAndWarnings_WhenParsingBinlogWithErrors_ShouldReturnCorrectTuple(bool verbose)
        {
            // Arrange
            var binaryLogProcessor = new BinaryLogProcessor();
            string projectWithErrorsPath = LogProcessorHelper.GetTestSupportProjectPath(_logProcessorFixture.TestProjectsDirectory, "ProjectWithErrors");
            string projectWithErrorsBinlogPath = GetBinlogPath(_logProcessorFixture.TestProjectsBuiltDirectory, "ProjectWithErrors");
            LogProcessorHelper.CreateLogFile(projectWithErrorsPath, projectWithErrorsBinlogPath);
            using var consoleRecorder = new ConsoleRecorder();

            // Act
            (bool actualSuccess, ProcessedLogResult actualProcessedLogResult) = binaryLogProcessor.CountAndGetErrorsAndWarnings(projectWithErrorsBinlogPath, verbose);

            // Assert
            actualSuccess.Should().BeTrue();
            actualProcessedLogResult.ErrorCount.Should().Be(2);
            actualProcessedLogResult.WarningCount.Should().Be(0);

            var logEntries = actualProcessedLogResult.LogEntries;
            logEntries.Should().HaveCount(2);

            logEntries[0].Type.Should().Be(LogEntryType.Error);
            logEntries[0].Code.Should().Be("CS0029");
            logEntries[0].Message.Should().Be("Cannot implicitly convert type 'string' to 'int'");
            logEntries[0].FilePath.Should().EndWith(@"tests\TestSupportProjects\ProjectWithErrors\Program.cs");
            logEntries[0].LineNumber.Should().Be(1);

            logEntries[1].Type.Should().Be(LogEntryType.Error);
            logEntries[1].Code.Should().Be("CS0029");
            logEntries[1].Message.Should().Be("Cannot implicitly convert type 'int' to 'string'");
            logEntries[1].FilePath.Should().EndWith(@"tests\TestSupportProjects\ProjectWithErrors\Program.cs");
            logEntries[1].LineNumber.Should().Be(2);

            if (verbose)
            {
                string output = consoleRecorder.GetOutput();
                output.Should().Contain("Processing records...");
                output.Should().Contain("Processed ");
            }
            else
            {
                consoleRecorder.GetOutput().Should().BeEmpty();
            }

            consoleRecorder.GetError().Should().BeEmpty();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CountAndGetErrorsAndWarnings_WhenParsingBinlogWithErrorsAndWarnings_ShouldReturnCorrectTuple(bool verbose)
        {
            // Arrange
            var binaryLogProcessor = new BinaryLogProcessor();
            string projectWithErrorsAndWarningsPath = LogProcessorHelper.GetTestSupportProjectPath(_logProcessorFixture.TestProjectsDirectory, "ProjectWithErrorsAndWarnings");
            string projectWithErrorsAndWarningsBinlogPath = GetBinlogPath(_logProcessorFixture.TestProjectsBuiltDirectory, "ProjectWithErrorsAndWarnings");
            LogProcessorHelper.CreateLogFile(projectWithErrorsAndWarningsPath, projectWithErrorsAndWarningsBinlogPath);
            using var consoleRecorder = new ConsoleRecorder();

            // Act
            (bool actualSuccess, ProcessedLogResult actualProcessedLogResult) = binaryLogProcessor.CountAndGetErrorsAndWarnings(projectWithErrorsAndWarningsBinlogPath, verbose);

            // Assert
            actualSuccess.Should().BeTrue();
            actualProcessedLogResult.ErrorCount.Should().Be(2);
            actualProcessedLogResult.WarningCount.Should().Be(2);

            var logEntries = actualProcessedLogResult.LogEntries;
            logEntries.Should().HaveCount(4);

            logEntries[0].Type.Should().Be(LogEntryType.Error);
            logEntries[0].Code.Should().Be("CS0029");
            logEntries[0].Message.Should().Be("Cannot implicitly convert type 'string' to 'int'");
            logEntries[0].FilePath.Should().EndWith(@"tests\TestSupportProjects\ProjectWithErrorsAndWarnings\Program.cs");
            logEntries[0].LineNumber.Should().Be(1);

            logEntries[1].Type.Should().Be(LogEntryType.Error);
            logEntries[1].Code.Should().Be("CS0029");
            logEntries[1].Message.Should().Be("Cannot implicitly convert type 'int' to 'string'");
            logEntries[1].FilePath.Should().EndWith(@"tests\TestSupportProjects\ProjectWithErrorsAndWarnings\Program.cs");
            logEntries[1].LineNumber.Should().Be(2);

            logEntries[2].Type.Should().Be(LogEntryType.Warning);
            logEntries[2].Code.Should().Be("CS8600");
            logEntries[2].Message.Should().Be("Converting null literal or possible null value to non-nullable type.");
            logEntries[2].FilePath.Should().EndWith(@"tests\TestSupportProjects\ProjectWithErrorsAndWarnings\Program.cs");
            logEntries[2].LineNumber.Should().Be(3);

            logEntries[3].Type.Should().Be(LogEntryType.Warning);
            logEntries[3].Code.Should().Be("CS0219");
            logEntries[3].Message.Should().Be("The variable 'firstAndSecondWarning' is assigned but its value is never used");
            logEntries[3].FilePath.Should().EndWith(@"tests\TestSupportProjects\ProjectWithErrorsAndWarnings\Program.cs");
            logEntries[3].LineNumber.Should().Be(3);

            if (verbose)
            {
                string output = consoleRecorder.GetOutput();
                output.Should().Contain("Processing records...");
                output.Should().Contain("Processed ");
            }
            else
            {
                consoleRecorder.GetOutput().Should().BeEmpty();
            }

            consoleRecorder.GetError().Should().BeEmpty();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CountAndGetErrorsAndWarnings_WhenParsingBinlogWithoutErrorsOrWarnings_ShouldReturnCorrectTuple(bool verbose)
        {
            // Arrange
            var binaryLogProcessor = new BinaryLogProcessor();
            string projectWithoutErrorsOrWarningsPath = LogProcessorHelper.GetTestSupportProjectPath(_logProcessorFixture.TestProjectsDirectory, "ProjectWithoutErrorsOrWarnings");
            string projectWithoutErrorsOrWarningsBinlogPath = GetBinlogPath(_logProcessorFixture.TestProjectsBuiltDirectory, "ProjectWithoutErrorsOrWarnings");
            LogProcessorHelper.CreateLogFile(projectWithoutErrorsOrWarningsPath, projectWithoutErrorsOrWarningsBinlogPath);
            using var consoleRecorder = new ConsoleRecorder();

            // Act
            (bool actualSuccess, ProcessedLogResult actualProcessedLogResult) = binaryLogProcessor.CountAndGetErrorsAndWarnings(projectWithoutErrorsOrWarningsBinlogPath, verbose);

            // Assert
            actualSuccess.Should().BeTrue();
            actualProcessedLogResult.ErrorCount.Should().Be(0);
            actualProcessedLogResult.WarningCount.Should().Be(0);

            actualProcessedLogResult.LogEntries.Should().HaveCount(0);

            if (verbose)
            {
                string output = consoleRecorder.GetOutput();
                output.Should().Contain("Processing records...");
                output.Should().Contain("Processed ");
            }
            else
            {
                consoleRecorder.GetOutput().Should().BeEmpty();
            }

            consoleRecorder.GetError().Should().BeEmpty();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CountAndGetErrorsAndWarnings_WhenParsingBinlogWithWarnings_ShouldReturnCorrectTuple(bool verbose)
        {
            // Arrange
            var binaryLogProcessor = new BinaryLogProcessor();
            string projectWithWarningsPath = LogProcessorHelper.GetTestSupportProjectPath(_logProcessorFixture.TestProjectsDirectory, "ProjectWithWarnings");
            string projectWithWarningsBinlogPath = GetBinlogPath(_logProcessorFixture.TestProjectsBuiltDirectory, "ProjectWithWarnings");
            LogProcessorHelper.CreateLogFile(projectWithWarningsPath, projectWithWarningsBinlogPath);
            using var consoleRecorder = new ConsoleRecorder();

            // Act
            (bool actualSuccess, ProcessedLogResult actualProcessedLogResult) = binaryLogProcessor.CountAndGetErrorsAndWarnings(projectWithWarningsBinlogPath, verbose);

            // Assert
            actualSuccess.Should().BeTrue();
            actualProcessedLogResult.ErrorCount.Should().Be(0);
            actualProcessedLogResult.WarningCount.Should().Be(2);

            var logEntries = actualProcessedLogResult.LogEntries;
            logEntries.Should().HaveCount(2);

            logEntries[0].Type.Should().Be(LogEntryType.Warning);
            logEntries[0].Code.Should().Be("CS8600");
            logEntries[0].Message.Should().Be("Converting null literal or possible null value to non-nullable type.");
            logEntries[0].FilePath.Should().EndWith(@"tests\TestSupportProjects\ProjectWithWarnings\Program.cs");
            logEntries[0].LineNumber.Should().Be(1);

            logEntries[1].Type.Should().Be(LogEntryType.Warning);
            logEntries[1].Code.Should().Be("CS0219");
            logEntries[1].Message.Should().Be("The variable 'firstAndSecondWarning' is assigned but its value is never used");
            logEntries[1].FilePath.Should().EndWith(@"tests\TestSupportProjects\ProjectWithWarnings\Program.cs");
            logEntries[1].LineNumber.Should().Be(1);

            if (verbose)
            {
                string output = consoleRecorder.GetOutput();
                output.Should().Contain("Processing records...");
                output.Should().Contain("Processed ");
            }
            else
            {
                consoleRecorder.GetOutput().Should().BeEmpty();
            }

            consoleRecorder.GetError().Should().BeEmpty();
        }

        public BinaryLogProcessorTests(LogProcessorFixture logProcessorFixture) =>
            _logProcessorFixture = logProcessorFixture;
    }
}
