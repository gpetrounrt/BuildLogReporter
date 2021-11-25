using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace BuildLogReporter.Processors
{
    public sealed class ProcessedLogResult
    {
        [JsonPropertyName("error_count")]
        public ushort ErrorCount { get; }

        [JsonPropertyName("warning_count")]
        public ushort WarningCount { get; }

        [JsonPropertyName("log_entries")]
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
