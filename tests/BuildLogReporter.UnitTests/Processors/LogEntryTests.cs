using System.Text;
using System.Xml;
using BuildLogReporter.Processors;
using FluentAssertions;
using Xunit;

namespace BuildLogReporter.UnitTests.Processors
{
    public sealed class LogEntryTests
    {
        [Theory]
        [InlineData(LogEntryType.Error, LogEntryType.Error, "C1", "C1", "Message1", "Message1", "Path1", "Path1", 1, 1, true)]
        [InlineData(LogEntryType.Error, LogEntryType.Warning, "C1", "C1", "Message1", "Message1", "Path1", "Path1", 1, 1, false)]
        [InlineData(LogEntryType.Error, LogEntryType.Error, "C1", "C2", "Message1", "Message1", "Path1", "Path1", 1, 1, false)]
        [InlineData(LogEntryType.Error, LogEntryType.Error, "C1", "C1", "Message1", "Message2", "Path1", "Path1", 1, 1, false)]
        [InlineData(LogEntryType.Error, LogEntryType.Error, "C1", "C1", "Message1", "Message1", "Path1", "Path2", 1, 1, false)]
        [InlineData(LogEntryType.Error, LogEntryType.Error, "C1", "C1", "Message1", "Message1", "Path1", "Path1", 1, 2, false)]
        public void EqualOperator_WhenComparingLogEntries_ShouldHaveExpectedResult(
            LogEntryType firstType,
            LogEntryType secondType,
            string firstCode,
            string secondCode,
            string firstMessage,
            string secondMessage,
            string firstFilePath,
            string secondFilePath,
            int firstLineNumber,
            int secondLineNumber,
            bool expectedResult)
        {
            // Arrange
            var first = new LogEntry(
                firstType,
                firstCode,
                firstMessage,
                firstFilePath,
                firstLineNumber);
            var second = new LogEntry(
                secondType,
                secondCode,
                secondMessage,
                secondFilePath,
                secondLineNumber);

            // Act
            var firstResult = first == second;
            var secondResult = second == first;

            // Assert
            firstResult.Should().Be(expectedResult);
            secondResult.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(LogEntryType.Error, LogEntryType.Error, "C1", "C1", "Message1", "Message1", "Path1", "Path1", 1, 1, false)]
        [InlineData(LogEntryType.Error, LogEntryType.Warning, "C1", "C1", "Message1", "Message1", "Path1", "Path1", 1, 1, true)]
        [InlineData(LogEntryType.Error, LogEntryType.Error, "C1", "C2", "Message1", "Message1", "Path1", "Path1", 1, 1, true)]
        [InlineData(LogEntryType.Error, LogEntryType.Error, "C1", "C1", "Message1", "Message2", "Path1", "Path1", 1, 1, true)]
        [InlineData(LogEntryType.Error, LogEntryType.Error, "C1", "C1", "Message1", "Message1", "Path1", "Path2", 1, 1, true)]
        [InlineData(LogEntryType.Error, LogEntryType.Error, "C1", "C1", "Message1", "Message1", "Path1", "Path1", 1, 2, true)]
        public void NotEqualOperator_WhenComparingLogEntries_ShouldHaveExpectedResult(
            LogEntryType firstType,
            LogEntryType secondType,
            string firstCode,
            string secondCode,
            string firstMessage,
            string secondMessage,
            string firstFilePath,
            string secondFilePath,
            int firstLineNumber,
            int secondLineNumber,
            bool expectedResult)
        {
            // Arrange
            var first = new LogEntry(
                firstType,
                firstCode,
                firstMessage,
                firstFilePath,
                firstLineNumber);
            var second = new LogEntry(
                secondType,
                secondCode,
                secondMessage,
                secondFilePath,
                secondLineNumber);

            // Act
            var firstResult = first != second;
            var secondResult = second != first;

            // Assert
            firstResult.Should().Be(expectedResult);
            secondResult.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(LogEntryType.Error, LogEntryType.Error, "C1", "C1", "Message1", "Message1", "Path1", "Path1", 1, 1, true)]
        [InlineData(LogEntryType.Error, LogEntryType.Warning, "C1", "C1", "Message1", "Message1", "Path1", "Path1", 1, 1, false)]
        [InlineData(LogEntryType.Error, LogEntryType.Error, "C1", "C2", "Message1", "Message1", "Path1", "Path1", 1, 1, false)]
        [InlineData(LogEntryType.Error, LogEntryType.Error, "C1", "C1", "Message1", "Message2", "Path1", "Path1", 1, 1, false)]
        [InlineData(LogEntryType.Error, LogEntryType.Error, "C1", "C1", "Message1", "Message1", "Path1", "Path2", 1, 1, false)]
        [InlineData(LogEntryType.Error, LogEntryType.Error, "C1", "C1", "Message1", "Message1", "Path1", "Path1", 1, 2, false)]
        public void GetHashCode_WhenInitialized_ShouldSetTheCorrectPropertyValues(
            LogEntryType firstType,
            LogEntryType secondType,
            string firstCode,
            string secondCode,
            string firstMessage,
            string secondMessage,
            string firstFilePath,
            string secondFilePath,
            int firstLineNumber,
            int secondLineNumber,
            bool expectedResult)
        {
            // Arrange
            var first = new LogEntry(
                firstType,
                firstCode,
                firstMessage,
                firstFilePath,
                firstLineNumber);
            var second = new LogEntry(
                secondType,
                secondCode,
                secondMessage,
                secondFilePath,
                secondLineNumber);

            // Act
            var firstHashCode = first.GetHashCode();
            var secondHashCode = second.GetHashCode();

            // Assert
            (firstHashCode == secondHashCode).Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(LogEntryType.Error, LogEntryType.Error, "C1", "C1", "Message1", "Message1", "Path1", "Path1", 1, 1, true)]
        [InlineData(LogEntryType.Error, LogEntryType.Warning, "C1", "C1", "Message1", "Message1", "Path1", "Path1", 1, 1, false)]
        [InlineData(LogEntryType.Error, LogEntryType.Error, "C1", "C2", "Message1", "Message1", "Path1", "Path1", 1, 1, false)]
        [InlineData(LogEntryType.Error, LogEntryType.Error, "C1", "C1", "Message1", "Message2", "Path1", "Path1", 1, 1, false)]
        [InlineData(LogEntryType.Error, LogEntryType.Error, "C1", "C1", "Message1", "Message1", "Path1", "Path2", 1, 1, false)]
        [InlineData(LogEntryType.Error, LogEntryType.Error, "C1", "C1", "Message1", "Message1", "Path1", "Path1", 1, 2, false)]
        public void Equals_WhenComparingLogEntryWithLogEntryObject_ShouldHaveExpectedResult(
            LogEntryType firstType,
            LogEntryType secondType,
            string firstCode,
            string secondCode,
            string firstMessage,
            string secondMessage,
            string firstFilePath,
            string secondFilePath,
            int firstLineNumber,
            int secondLineNumber,
            bool expectedResult)
        {
            // Arrange
            var first = new LogEntry(
                firstType,
                firstCode,
                firstMessage,
                firstFilePath,
                firstLineNumber);
            var second = new LogEntry(
                secondType,
                secondCode,
                secondMessage,
                secondFilePath,
                secondLineNumber);

            object secondAsObject = second;

            // Act
            var result = first.Equals(secondAsObject);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Fact]
        public void Equals_WhenComparingNull_ShouldReturnFalse()
        {
            // Arrange
            var logEntry = new LogEntry(
                LogEntryType.Error,
                "Code",
                "Message",
                "FilePath",
                1);

            // Act
            var result = logEntry.Equals(null);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WhenComparingDifferentObjectType_ShouldReturnFalse()
        {
            // Arrange
            var logEntry = new LogEntry(
                LogEntryType.Error,
                "Code",
                "Message",
                "FilePath",
                1);

            var obj = default(LogEntryType);

            // Act
            var result = logEntry.Equals(obj);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ToString_WhenHavingCorrectPropertyValues_ShouldReturnExpectedResult()
        {
            // Arrange
            var logEntry = new LogEntry(
                LogEntryType.Error,
                "Code",
                "Message",
                "FilePath",
                1);

            // Act
            var result = logEntry.ToString();

            // Assert
            result.Should().Be(@"
Type: Error
Code: Code
Message: Message
File path: FilePath
Line Number: 1");
        }

        [Fact]
        public void WriteXml_WhenHavingCorrectPropertyValues_ShouldReturnExpectedResult()
        {
            // Arrange
            var logEntry = new LogEntry(
                LogEntryType.Error,
                "Code",
                "Message",
                "FilePath",
                1);

            var stringBuilder = new StringBuilder();
            using var xmlWriter = XmlWriter.Create(stringBuilder);

            // Act
            logEntry.WriteXml(xmlWriter);
            xmlWriter.Flush();

            // Assert
            stringBuilder.ToString().Should().EndWith(@"<LogEntry Type=""Error"" Code=""Code"" Message=""Message"" FilePath=""FilePath"" LineNumber=""1"" />");
        }

        [Fact]
        public void Constructor_WhenInitialized_ShouldSetTheCorrectPropertyValues()
        {
            // Arrange
            LogEntryType expectedType = LogEntryType.Warning;
            string expectedCode = "Code";
            string expectedMessage = "Message";
            string expectedFilePath = "FilePath";
            int expectedLineNumber = 1;

            // Act
            var logEntry = new LogEntry(
                expectedType,
                expectedCode,
                expectedMessage,
                expectedFilePath,
                expectedLineNumber);

            // Assert
            logEntry.Type.Should().Be(expectedType);
            logEntry.Code.Should().Be(expectedCode);
            logEntry.Message.Should().Be(expectedMessage);
            logEntry.FilePath.Should().Be(expectedFilePath);
            logEntry.LineNumber.Should().Be(expectedLineNumber);
        }
    }
}
