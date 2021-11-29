using BuildLogReporter.Wrappers;

namespace BuildLogReporter.Diagnostics
{
    public sealed class ExecutionTimer
    {
        private const int MillisecondsPerSecond = 1000;

        private readonly IStopwatch _stopwatch;

        public double TotalMilliseconds { get; private set; }

        public void Measure(Action action)
        {
            ArgumentNullException.ThrowIfNull(action);

            _stopwatch.Start();
            action();
            _stopwatch.Stop();

            TotalMilliseconds = _stopwatch.Elapsed.TotalMilliseconds;
        }

        public string GetElapsedTimeAsString()
        {
            if (TotalMilliseconds > MillisecondsPerSecond)
            {
                return $"{TotalMilliseconds / MillisecondsPerSecond} seconds";
            }

            return $"{TotalMilliseconds} milliseconds";
        }

        public ExecutionTimer(IStopwatch stopwatch) =>
            _stopwatch = stopwatch;

        public ExecutionTimer()
            : this(new StopwatchWrapper())
        {
        }
    }
}
