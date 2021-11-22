using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Reflection;

namespace BuildLogReporter
{
    public sealed class ProgramExecutor
    {
        private readonly IFileSystem _fileSystem;

        public Task<int> ProcessFileAsync(string[] args)
        {
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

            var rootCommand = new RootCommand
            {
                logPathArgument
            };

            var versionAsString = Assembly.GetExecutingAssembly()
                 ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                 ?.InformationalVersion
                 .ToString();

            rootCommand.Description = $"Build Log Reporter {versionAsString}";

            rootCommand.Handler = CommandHandler.Create<string>((logPath) =>
            {
                // TODO: Process log file.
            });

            return rootCommand.InvokeAsync(args);
        }

        public ProgramExecutor(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public ProgramExecutor()
            : this(new FileSystem())
        {
        }
    }
}
