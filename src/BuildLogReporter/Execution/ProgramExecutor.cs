using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Reflection;
using BuildLogReporter.Diagnostics;
using BuildLogReporter.Processors;
using BuildLogReporter.Reporters;

namespace BuildLogReporter.Execution
{
    public sealed class ProgramExecutor
    {
        private readonly IFileSystem _fileSystem;

        private readonly RootCommand _rootCommand;

        private readonly HashSet<string> _availableReportTypes = new HashSet<string>()
        {
            nameof(ReportType.Badge),
            nameof(ReportType.Html),
            nameof(ReportType.Json),
            nameof(ReportType.Markdown),
            nameof(ReportType.Xml)
        };

        public int ExportReports(
            string logPath,
            string reportPath,
            string reportTypes,
            ProcessedLogResult processedLogResult,
            bool verbose)
        {
            ArgumentNullException.ThrowIfNull(reportTypes);

            var reportTypesAsArray = reportTypes.Split(';');
            foreach (var reportType in reportTypesAsArray)
            {
                Reporter reporter;
                switch (reportType)
                {
                    case nameof(ReportType.Badge):
                        reporter = new BadgeReporter();
                        break;
                    case nameof(ReportType.Html):
                        reporter = new HtmlReporter(logPath);
                        break;
                    case nameof(ReportType.Json):
                        reporter = new JsonReporter();
                        break;
                    case nameof(ReportType.Markdown):
                        reporter = new MarkdownReporter();
                        break;
                    case nameof(ReportType.Xml):
                        reporter = new XmlReporter();
                        break;
                    default:
                        Console.Error.WriteLine($"Could not find reporter for {reportType}.");
                        return 1;
                }

                string reportAsString = string.Empty;
                if (verbose)
                {
                    Console.WriteLine($"Generating {reportType} report...");
                    var executionTimer = new ExecutionTimer();
                    executionTimer.Measure(() =>
                    {
                        reportAsString = reporter.GetReportAsString(processedLogResult);
                    });
                    Console.WriteLine($"Generated report in {executionTimer.GetElapsedTimeAsString()}.");
                }
                else
                {
                    reportAsString = reporter.GetReportAsString(processedLogResult);
                }

                if (verbose)
                {
                    Console.WriteLine($"Saving report to {reportPath}.");
                }

                try
                {
                    _fileSystem.File.WriteAllText(Path.Combine(reportPath, $"BuildLogReport.{reporter.Extension}"), reportAsString);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Could not save report to file.{Environment.NewLine}{ex}");

                    return 1;
                }

                if (verbose)
                {
                    Console.WriteLine("Saved report.");
                }
            }

            return 0;
        }

        public int ProcessLogFileAndExportReports(
            string logPath,
            string reportPath,
            string reportTypes,
            bool verbose)
        {
            ArgumentNullException.ThrowIfNull(logPath);
            ArgumentNullException.ThrowIfNull(reportPath);
            ArgumentNullException.ThrowIfNull(reportTypes);

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

            (bool success, ProcessedLogResult processedLogResult) = logProcessor.CountAndGetErrorsAndWarnings(logPath, verbose);
            if (!success)
            {
                return 1;
            }

            if (verbose)
            {
                Console.WriteLine($"Found {processedLogResult.ErrorCount} error(s) and {processedLogResult.WarningCount} warning(s).");
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
                        Console.WriteLine("Created report directory.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Could not create {reportPath}.{Environment.NewLine}{ex}");
                return 1;
            }

            return ExportReports(
                logPath,
                reportPath,
                reportTypes,
                processedLogResult,
                verbose);
        }

        public int ProcessFile(
            string logPath,
            string reportPath,
            string reportTypes,
            bool verbose)
        {
            if (verbose)
            {
                Console.WriteLine($"Starting processing of '{logPath}'...");
                var executionTimer = new ExecutionTimer();
                int result = -1;
                executionTimer.Measure(() =>
                {
                    result = ProcessLogFileAndExportReports(logPath, reportPath, reportTypes, verbose);
                });
                Console.WriteLine($"Completed processing in {executionTimer.GetElapsedTimeAsString()}.");

                return result;
            }
            else
            {
                return ProcessLogFileAndExportReports(logPath, reportPath, reportTypes, verbose);
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

            var reportTypesOption = new Option<string>(
                new[] { "--report-types", "-rt" },
                () => "Html",
                "The type of reports to generate");
            reportTypesOption.AddValidator(optionResult =>
            {
                var reportTypes = optionResult.GetValueOrDefault<string>();
                if (string.IsNullOrWhiteSpace(reportTypes))
                {
                    return $"'{optionResult.Option.Name}' cannot be null or empty.";
                }

                var reportTypesAsArray = reportTypes.Split(';');
                foreach (var reportType in reportTypesAsArray)
                {
                    if (!_availableReportTypes.Contains(reportType))
                    {
                        return $"'{reportType}' is an invalid value.{Environment.NewLine}Possible values: {string.Join(", ", _availableReportTypes)}";
                    }
                }

                var uniqueReportTypes = new HashSet<string>();
                foreach (string reportType in reportTypesAsArray)
                {
                    if (uniqueReportTypes.Contains(reportType))
                    {
                        return $"'{reportType}' is already selected in '{optionResult.Option.Name}'.";
                    }

                    uniqueReportTypes.Add(reportType);
                }

                return null;
            });

            _rootCommand = new RootCommand
            {
                logPathArgument,
                reportPathArgument,
                reportTypesOption,
                new Option<bool>(
                    new[] { "--verbose", "-v" },
                    () => false,
                    "Whether to use verbose output")
            };

            var versionAsString = Assembly.GetExecutingAssembly()
                 ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                 ?.InformationalVersion;

            _rootCommand.Description = $"Build Log Reporter {versionAsString}";

            _rootCommand.Handler = CommandHandler.Create<string, string, string, bool>(ProcessFile);
        }

        public ProgramExecutor()
            : this(new FileSystem())
        {
        }
    }
}
