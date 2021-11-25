using System.Collections.ObjectModel;

namespace BuildLogReporter.Processors
{
    public sealed class ProcessedLogResult
    {
        public ushort ErrorCount { get; }

        public ushort WarningCount { get; }

        public ReadOnlyCollection<LogEntry> LogEntries { get; }

        public ProcessedLogResult(
            ushort errorCount,
            ushort warningCount,
            ReadOnlyCollection<LogEntry> logEntries)
        {
            ErrorCount = errorCount;
            WarningCount = warningCount;
            LogEntries = logEntries;
        }
    }
}
