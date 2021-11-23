﻿using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using Xunit;

namespace BuildLogReporter.UnitTests
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
            var result = await programExecutor.ProcessFileAsync(Array.Empty<string>());

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
            var result = await programExecutor.ProcessFileAsync(new string[] { string.Empty });

            // Assert
            result.Should().Be(1);
            consoleRecorder.GetOutput().Should().Contain("<logPath>  The path of the log file");
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
            var result = await programExecutor.ProcessFileAsync(new string[] { @"C:\temp\build.invalid" });

            // Assert
            result.Should().Be(1);
            consoleRecorder.GetOutput().Should().Contain("<logPath>  The path of the log file");
            consoleRecorder.GetError().Should().StartWith("'logPath' has an unsupported file extension value of '.invalid'");
        }

        [Fact]
        public async Task ProcessFileAsync_WhenHavingInvalidPath_ShouldReturnOneAndDisplayExpectedOutput()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            const string invalidPath = @"C:\temp\build.binlog";
            var programExecutor = new ProgramExecutor(mockFileSystem);
            using var consoleRecorder = new ConsoleRecorder();

            // Act
            var result = await programExecutor.ProcessFileAsync(new string[] { invalidPath });

            // Assert
            result.Should().Be(1);
            consoleRecorder.GetOutput().Should().Contain("<logPath>  The path of the log file");
            consoleRecorder.GetError().Should().StartWith($"'{invalidPath}' does not exist.");
        }

        [Theory]
        [InlineData(@"C:\temp\build.binlog")]
        [InlineData(@"C:\temp\build.log")]
        public async Task ProcessFileAsync_WhenHavingExistingInvalidPath_ShouldReturnZeroAndDisplayExpectedOutput(string logPath)
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var mockLogFile = new MockFileData(string.Empty);
            mockFileSystem.AddFile(logPath, mockLogFile);
            var programExecutor = new ProgramExecutor(mockFileSystem);
            using var consoleRecorder = new ConsoleRecorder();

            // Act
            var result = await programExecutor.ProcessFileAsync(new string[] { logPath });

            // Assert
            result.Should().Be(0);
            consoleRecorder.GetOutput().Should().BeEmpty();
            consoleRecorder.GetError().Should().BeEmpty();
        }

        [Theory]
        [InlineData(@"C:\temp\build.binlog")]
        [InlineData(@"C:\temp\build.log")]
        public async Task ProcessFileAsync_WhenVerboseIsEnabledAndHavingExistingInvalidPath_ShouldReturnZeroAndDisplayExpectedOutput(string logPath)
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var mockLogFile = new MockFileData(string.Empty);
            mockFileSystem.AddFile(logPath, mockLogFile);
            var programExecutor = new ProgramExecutor(mockFileSystem);
            using var consoleRecorder = new ConsoleRecorder();

            // Act
            var result = await programExecutor.ProcessFileAsync(new string[] { "--verbose", logPath });

            // Assert
            result.Should().Be(0);
            consoleRecorder.GetOutput().Should().StartWith($"Starting processing of '{logPath}'...");
            consoleRecorder.GetOutput().Should().Contain($"Completed processing in");
            consoleRecorder.GetError().Should().BeEmpty();
        }
    }
}
