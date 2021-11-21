namespace BuildLogReporter.Wrappers
{
    public interface IStopwatch
    {
        TimeSpan Elapsed { get; }

        void Start();

        void Stop();
    }
}
