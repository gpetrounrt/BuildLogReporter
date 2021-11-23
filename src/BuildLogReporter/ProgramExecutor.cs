using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Reflection;
using BuildLogReporter.Diagnostics;

namespace BuildLogReporter
{
    public sealed class ProgramExecutor
    {
        private readonly IFileSystem _fileSystem;

        private readonly RootCommand _rootCommand;

        public static void ProcessFile(string logPath, bool verbose)
        {
            if (verbose)
            {
                Console.WriteLine($"Starting processing of '{logPath}'...");
                var executionTimer = new ExecutionTimer();
                executionTimer.Measure(() =>
                {
                    // TODO: Process log file.
                });
                Console.WriteLine($"Completed processing in {executionTimer.GetElapsedTimeAsString()}.");
            }
            else
            {
                // TODO: Process log file.
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

            _rootCommand = new RootCommand
            {
                logPathArgument,
                new Option<bool>(
                    new[] { "--verbose", "-v" },
                    () => false,
                    "Whether to use verbose output")
            };

            var versionAsString = Assembly.GetExecutingAssembly()
                 ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                 ?.InformationalVersion;

            _rootCommand.Description = $"Build Log Reporter {versionAsString}";

            _rootCommand.Handler = CommandHandler.Create<string, bool>(ProcessFile);
        }

        public ProgramExecutor()
            : this(new FileSystem())
        {
        }
    }
}
