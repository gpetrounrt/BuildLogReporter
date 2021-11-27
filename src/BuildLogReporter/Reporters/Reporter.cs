using BuildLogReporter.Processors;

namespace BuildLogReporter.Reporters
{
    public abstract class Reporter
    {
        public abstract string Extension { get; }

        public abstract string GetReportAsString(ProcessedLogResult processedLogResult);
    }
}
