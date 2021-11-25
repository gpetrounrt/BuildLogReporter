using System.Collections.ObjectModel;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Reflection;
using BuildLogReporter.Diagnostics;
using BuildLogReporter.Entries;
using BuildLogReporter.Processors;

namespace BuildLogReporter.Execution
{
    public sealed class ProgramExecutor
    {
        private readonly IFileSystem _fileSystem;

        private readonly RootCommand _rootCommand;

        public int ProcessLogFile(
            string logPath,
            string reportPath,
            bool verbose)
        {
            LogProcessor logProcessor;
            if (logPath.EndsWith(".binlog", StringComparison.OrdinalIgnoreCase))
            {
                logProcessor = new BinaryLogProcessor(_fileSystem);
            }
            else if (logPath.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
            {
                logProcessor = new TextLogProcessor(_fileSystem);
            }
            else
            {
                Console.Error.WriteLine("Could not initialize log processor.");
                return 1;
            }

            (bool success, ushort errorCount, ushort warningCount, ReadOnlyCollection<LogEntry> logEntries) = logProcessor.CountAndGetErrorsAndWarnings(logPath, verbose);
            if (!success)
            {
                return 1;
            }

            if (verbose)
            {
                Console.WriteLine($"Found {errorCount} error(s) and {warningCount} warning(s).");
            }

            try
            {
                if (!_fileSystem.Directory.Exists(reportPath))
                {
                    if (verbose)
                    {
                        Console.WriteLine($"Creating '{reportPath}'...");
                    }

                    _fileSystem.Directory.CreateDirectory(reportPath);

                    if (verbose)
                    {
                        Console.WriteLine($"Created report directory.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Could not create {reportPath}.{Environment.NewLine}{ex}");
                return 1;
            }

            // TODO: Implement reporters.
            return 0;
        }

        public int ProcessFile(
            string logPath,
            string reportPath,
            bool verbose)
        {
            if (verbose)
            {
                Console.WriteLine($"Starting processing of '{logPath}'...");
                var executionTimer = new ExecutionTimer();
                int result = -1;
                executionTimer.Measure(() =>
                {
                    result = ProcessLogFile(logPath, reportPath, verbose);
                });
                Console.WriteLine($"Completed processing in {executionTimer.GetElapsedTimeAsString()}.");

                return result;
            }
            else
            {
                return ProcessLogFile(logPath, reportPath, verbose);
            }
        }

        public Task<int> ProcessFileAsync(string[] args) =>
            _rootCommand.InvokeAsync(args);

        public ProgramExecutor(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;

            var logPathArgument = new Argument<string>("logPath", "The path of the log file");
            logPathArgument.AddValidator(argumentResult =>
            {
                var logPath = argumentResult.GetValueOrDefault<string>();
                if (string.IsNullOrWhiteSpace(logPath))
                {
                    return $"'{argumentResult.Argument.Name}' cannot be null or empty.";
                }

                bool isBinaryLog = logPath.EndsWith(".binlog", StringComparison.OrdinalIgnoreCase);
                bool isTextLog = logPath.EndsWith(".log", StringComparison.OrdinalIgnoreCase);
                if (!isBinaryLog && !isTextLog)
                {
                    return $"'{argumentResult.Argument.Name}' has an unsupported file extension value of '{Path.GetExtension(logPath)}'.";
                }

                if (!_fileSystem.File.Exists(logPath))
                {
                    return $"'{logPath}' does not exist.";
                }

                return null;
            });

            var reportPathArgument = new Argument<string>("reportPath", "The path of the report files");
            reportPathArgument.AddValidator(argumentResult =>
            {
                var reportPath = argumentResult.GetValueOrDefault<string>();
                if (string.IsNullOrWhiteSpace(reportPath))
                {
                    return $"'{argumentResult.Argument.Name}' cannot be null or empty.";
                }

                if (reportPath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                {
                    return $"'{reportPath}' contains invalid characters.";
                }

                return null;
            });

            _rootCommand = new RootCommand
            {
                logPathArgument,
                reportPathArgument,
                new Option<bool>(
                    new[] { "--verbose", "-v" },
                    () => false,
                    "Whether to use verbose output")
            };

            var versionAsString = Assembly.GetExecutingAssembly()
                 ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                 ?.InformationalVersion;

            _rootCommand.Description = $"Build Log Reporter {versionAsString}";

            _rootCommand.Handler = CommandHandler.Create<string, string, bool>(ProcessFile);
        }

        public ProgramExecutor()
            : this(new FileSystem())
        {
        }
    }
}
