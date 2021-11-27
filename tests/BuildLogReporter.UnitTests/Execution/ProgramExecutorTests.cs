using System.IO.Abstractions.TestingHelpers;
using BuildLogReporter.Execution;
using BuildLogReporter.Tests.Common.Diagnostics;
using FluentAssertions;
using Xunit;

namespace BuildLogReporter.UnitTests.Execution
{
    public sealed class ProgramExecutorTests
    {
        [Fact]
        public async Task ProcessFileAsync_WhenHavingNoArguments_ShouldReturnOneAndDisplayExpectedOutput()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var programExecutor = new ProgramExecutor(mockFileSystem);
            using var consoleRecorder = new ConsoleRecorder();

            // Act
            var result = await programExecutor.ProcessFileAsync(Array.Empty<string>()).ConfigureAwait(false);

            // Assert
            result.Should().Be(1);
            consoleRecorder.GetOutput().Should().MatchRegex(@"Build Log Reporter \d+.\d+.\d+");
            consoleRecorder.GetError().Should().StartWith("Required argument missing for command");
        }

        [Fact]
        public async Task ProcessFileAsync_WhenHavingEmptyLogPath_ShouldReturnOneAndDisplayExpectedOutput()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var programExecutor = new ProgramExecutor(mockFileSystem);
            using var consoleRecorder = new ConsoleRecorder();

            // Act
            var result = await programExecutor.ProcessFileAsync(new string[] { string.Empty, @"C:\temp\out" }).ConfigureAwait(false);

            // Assert
            result.Should().Be(1);
            consoleRecorder.GetOutput().Should().Contain("<logPath>     The path of the log file");
            consoleRecorder.GetError().Should().StartWith("'logPath' cannot be null or empty.");
        }

        [Fact]
        public async Task ProcessFileAsync_WhenHavingInvalidFileExtension_ShouldReturnOneAndDisplayExpectedOutput()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var programExecutor = new ProgramExecutor(mockFileSystem);
            using var consoleRecorder = new ConsoleRecorder();

            // Act
            var result = await programExecutor.ProcessFileAsync(new string[] { @"C:\temp\build.invalid", @"C:\temp\out" }).ConfigureAwait(false);

            // Assert
            result.Should().Be(1);
            consoleRecorder.GetOutput().Should().Contain("<logPath>     The path of the log file");
            consoleRecorder.GetError().Should().StartWith("'logPath' has an unsupported file extension value of '.invalid'");
        }

        [Fact]
        public async Task ProcessFileAsync_WhenHavingInvalidPath_ShouldReturnOneAndDisplayExpectedOutput()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var programExecutor = new ProgramExecutor(mockFileSystem);
            using var consoleRecorder = new ConsoleRecorder();
            const string invalidPath = @"C:\temp\build.binlog";

            // Act
            var result = await programExecutor.ProcessFileAsync(new string[] { invalidPath, @"C:\temp\out" }).ConfigureAwait(false);

            // Assert
            result.Should().Be(1);
            consoleRecorder.GetOutput().Should().Contain("<logPath>     The path of the log file");
            consoleRecorder.GetError().Should().StartWith($"'{invalidPath}' does not exist.");
        }

        [Fact]
        public async Task ProcessFileAsync_WhenHavingEmptyExistingBinaryLogPath_ShouldReturnOneAndDisplayExpectedOutput()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var mockLogFile = new MockFileData(string.Empty);
            const string logPath = @"C:\temp\build.binlog";
            mockFileSystem.AddFile(logPath, mockLogFile);
            var programExecutor = new ProgramExecutor(mockFileSystem);
            using var consoleRecorder = new ConsoleRecorder();

            // Act
            var result = await programExecutor.ProcessFileAsync(new string[] { logPath, @"C:\temp\out" }).ConfigureAwait(false);

            // Assert
            result.Should().Be(1);
            consoleRecorder.GetOutput().Should().BeEmpty();
            consoleRecorder.GetError().Should().StartWith($"Could not extract errors and warnings.{Environment.NewLine}System.IO.EndOfStreamException");
        }

        [Fact]
        public async Task ProcessFileAsync_WhenHavingEmptyExistingTextLogPath_ShouldReturnZeroAndDisplayExpectedOutput()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var mockLogFile = new MockFileData(string.Empty);
            const string logPath = @"C:\temp\build.log";
            mockFileSystem.AddFile(logPath, mockLogFile);
            var programExecutor = new ProgramExecutor(mockFileSystem);
            using var consoleRecorder = new ConsoleRecorder();

            // Act
            var result = await programExecutor.ProcessFileAsync(new string[] { logPath, @"C:\temp\out" }).ConfigureAwait(false);

            // Assert
            result.Should().Be(0);
            consoleRecorder.GetOutput().Should().BeEmpty();
            consoleRecorder.GetError().Should().BeEmpty();
        }

        [Fact]
        public async Task ProcessFileAsync_WhenVerboseIsEnabledAndHavingEmptyExistingBinaryLogPath_ShouldReturnOneAndDisplayExpectedOutput()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var mockLogFile = new MockFileData(string.Empty);
            const string logPath = @"C:\temp\build.binlog";
            mockFileSystem.AddFile(logPath, mockLogFile);
            var programExecutor = new ProgramExecutor(mockFileSystem);
            using var consoleRecorder = new ConsoleRecorder();

            // Act
            var result = await programExecutor.ProcessFileAsync(new string[] { "--verbose", logPath, @"C:\temp\out" }).ConfigureAwait(false);

            // Assert
            result.Should().Be(1);
            consoleRecorder.GetOutput().Should().StartWith($"Starting processing of '{logPath}'...");
            consoleRecorder.GetOutput().Should().Contain($"Completed processing in");
            consoleRecorder.GetError().Should().StartWith($"Could not extract errors and warnings.{Environment.NewLine}System.IO.EndOfStreamException");
        }

        [Fact]
        public async Task ProcessFileAsync_WhenVerboseIsEnabledAndHavingEmptyExistingTextLogPath_ShouldReturnZeroAndDisplayExpectedOutput()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var mockLogFile = new MockFileData(string.Empty);
            const string logPath = @"C:\temp\build.log";
            mockFileSystem.AddFile(logPath, mockLogFile);
            var programExecutor = new ProgramExecutor(mockFileSystem);
            using var consoleRecorder = new ConsoleRecorder();

            // Act
            var result = await programExecutor.ProcessFileAsync(new string[] { "--verbose", logPath, @"C:\temp\out" }).ConfigureAwait(false);

            // Assert
            result.Should().Be(0);
            consoleRecorder.GetOutput().Should().StartWith($"Starting processing of '{logPath}'...");
            consoleRecorder.GetOutput().Should().Contain($"Completed processing in");
            consoleRecorder.GetError().Should().BeEmpty();
        }
    }
}
