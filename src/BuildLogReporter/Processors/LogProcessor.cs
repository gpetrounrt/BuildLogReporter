using System.IO.Abstractions;

namespace BuildLogReporter.Processors
{
    public abstract class LogProcessor
    {
        protected IFileSystem FileSystem { get; }

        public abstract (bool Success, ProcessedLogResult ProcessedLogResult) CountAndGetErrorsAndWarnings(
            string logPath,
            bool verbose);

        protected LogProcessor(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }
    }
}
