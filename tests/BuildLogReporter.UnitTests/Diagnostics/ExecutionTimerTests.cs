using BuildLogReporter.Diagnostics;
using BuildLogReporter.Wrappers;
using FluentAssertions;
using Xunit;

namespace BuildLogReporter.UnitTests.Diagnostics
{
    public sealed class ExecutionTimerTests
    {
        [Fact]
        public void TotalMilliseconds_WhenMeasureIsCalled_ShouldBeGreaterThanZero()
        {
            // Arrange
            var executionTimer = new ExecutionTimer();

            // Act
            executionTimer.Measure(() => { });

            // Assert
            executionTimer.TotalMilliseconds.Should().BePositive();
        }

        [Theory]
        [InlineData(100, "100 milliseconds")]
        [InlineData(2000, "2 seconds")]
        public void GetElapsedTimeAsString_WhenMeasureIsCalled_ShouldHaveCorrectResult(double milliseconds, string expectedResult)
        {
            // Arrange
            var stopWatch = new Moq.Mock<IStopwatch>();
            stopWatch.SetupGet(s => s.Elapsed).Returns(TimeSpan.FromMilliseconds(milliseconds));
            var executionTimer = new ExecutionTimer(stopWatch.Object);

            // Act
            executionTimer.Measure(() => { });

            // Assert
            executionTimer.GetElapsedTimeAsString().Should().Be(expectedResult);
        }
    }
}
