using System.Collections.ObjectModel;
using System.IO.Abstractions;
using BuildLogReporter.Diagnostics;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging.StructuredLogger;

namespace BuildLogReporter.Processors
{
    public sealed class BinaryLogProcessor : LogProcessor
    {
        private (bool Success, ProcessedLogResult ProcessedLogResult) ExtractErrorsAndWarnings(
            string logPath,
            bool verbose)
        {
            try
            {
                if (verbose)
                {
                    Console.WriteLine("Opening binary log file...");
                }

                using var fileStream = FileSystem.File.OpenRead(logPath);
                var binLogReader = new BinLogReader();
                var records = binLogReader.ReadRecords(fileStream);

                if (verbose)
                {
                    Console.WriteLine("Opened binary log file.");
                }

                ushort errorCount = 0;
                ushort warningCount = 0;
                var logEntries = new List<LogEntry>();

                ulong recordCount = 0;
                if (verbose)
                {
                    Console.WriteLine("Processing records...");
                }

                foreach (var record in records)
                {
                    if (verbose)
                    {
                        recordCount++;
                    }

                    var buildEventArgs = record.Args;
                    if (buildEventArgs is BuildErrorEventArgs buildErrorEventArgs)
                    {
                        errorCount++;

                        LogEntry logEntry = new LogEntry(
                            LogEntryType.Error,
                            buildErrorEventArgs.Code,
                            buildErrorEventArgs.Message,
                            buildErrorEventArgs.File,
                            buildErrorEventArgs.LineNumber);
                        logEntries.Add(logEntry);

                        if (verbose)
                        {
                            Console.WriteLine($"Found error record.{logEntry}");
                        }
                    }
                    else if (buildEventArgs is BuildWarningEventArgs buildWarningEventArgs)
                    {
                        warningCount++;

                        LogEntry logEntry = new LogEntry(
                            LogEntryType.Warning,
                            buildWarningEventArgs.Code,
                            buildWarningEventArgs.Message,
                            buildWarningEventArgs.File,
                            buildWarningEventArgs.LineNumber);
                        logEntries.Add(logEntry);

                        if (verbose)
                        {
                            Console.WriteLine($"Found warning record.{logEntry}");
                        }
                    }
                }

                if (verbose)
                {
                    Console.WriteLine($"Processed {recordCount} record(s).");
                }

                return (true, new ProcessedLogResult(errorCount, warningCount, logEntries.AsReadOnly()));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Could not extract errors and warnings.{Environment.NewLine}{ex}");

                return (false, new ProcessedLogResult(0, 0, new List<LogEntry>().AsReadOnly()));
            }
        }

        public override (bool Success, ProcessedLogResult ProcessedLogResult) CountAndGetErrorsAndWarnings(
            string logPath,
            bool verbose)
        {
            if (verbose)
            {
                Console.WriteLine($"Starting extraction of log entries...");
                var executionTimer = new ExecutionTimer();
                (bool Success, ProcessedLogResult ProcessedLogResult) result = (false, new ProcessedLogResult(0, 0, new List<LogEntry>().AsReadOnly()));
                executionTimer.Measure(() =>
                {
                    result = ExtractErrorsAndWarnings(logPath, verbose);
                });
                Console.WriteLine($"Completed extraction in {executionTimer.GetElapsedTimeAsString()}.");

                return result;
            }
            else
            {
                return ExtractErrorsAndWarnings(logPath, verbose);
            }
        }

        public BinaryLogProcessor(IFileSystem fileSystem)
            : base(fileSystem)
        {
        }

        public BinaryLogProcessor()
           : this(new FileSystem())
        {
        }
    }
}
