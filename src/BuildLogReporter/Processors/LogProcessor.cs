using System.Collections.ObjectModel;
using System.IO.Abstractions;
using BuildLogReporter.Entries;

namespace BuildLogReporter.Processors
{
    public abstract class LogProcessor
    {
        protected IFileSystem FileSystem { get; }

        public abstract (bool Success, ushort ErrorCount, ushort WarningCount, ReadOnlyCollection<LogEntry> LogEntries) CountAndGetErrorsAndWarnings(
            string logPath,
            bool verbose);

        protected LogProcessor(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }
    }
}
