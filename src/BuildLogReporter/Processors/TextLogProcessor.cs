using System.Collections.ObjectModel;
using System.IO.Abstractions;
using System.Text;
using System.Text.RegularExpressions;
using BuildLogReporter.Diagnostics;

namespace BuildLogReporter.Processors
{
    public sealed class TextLogProcessor : LogProcessor
    {
        private readonly Regex _codeRegex = new Regex(@": (error|warning) (.*?):", RegexOptions.Compiled);

        private readonly Regex _messageRegex = new Regex(@"\d: (.*) \[", RegexOptions.Compiled);

        private readonly Regex _filePathRegex = new Regex(@"\s*(.*)\(\d*,\d*\): (error|warning)", RegexOptions.Compiled);

        private readonly Regex _lineAndColumnRegex = new Regex(@"\((.*)\): ", RegexOptions.Compiled);

        private static string GetValueFromRegex(string line, Regex entryTypeRegex, int groupIndex)
        {
            Match match = entryTypeRegex.Match(line);
            if (match.Success)
            {
                return match.Groups[groupIndex].Value;
            }

            return string.Empty;
        }

        private int GetLineNumber(string line)
        {
            string lineAndColumn = GetValueFromRegex(line, _lineAndColumnRegex, 1);
            int indexOfComma = lineAndColumn.IndexOf(',', StringComparison.Ordinal);

            return int.Parse(lineAndColumn.AsSpan().Slice(0, indexOfComma));
        }

        private (bool Success, ProcessedLogResult ProcessedLogResult) ExtractErrorsAndWarnings(
            string logPath,
            bool verbose)
        {
            try
            {
                if (verbose)
                {
                    Console.WriteLine("Opening text log file...");
                }

                using var fileStream = FileSystem.File.OpenRead(logPath);
                using var streamReader = new StreamReader(fileStream, Encoding.UTF8);
                string? line;

                if (verbose)
                {
                    Console.WriteLine("Opened text log file.");
                }

                ushort errorCount = 0;
                ushort warningCount = 0;
                var logEntries = new List<LogEntry>();

                ulong lineCount = 0;
                if (verbose)
                {
                    Console.WriteLine("Processing lines...");
                }

                while ((line = streamReader.ReadLine()) != null)
                {
                    if (verbose)
                    {
                        lineCount++;
                    }

                    if (Regex.IsMatch(line, ": error", RegexOptions.IgnoreCase) && !Regex.IsMatch(line, @"^\s*\d"))
                    {
                        errorCount++;

                        LogEntry logEntry = new LogEntry(
                            LogEntryType.Error,
                            GetValueFromRegex(line, _codeRegex, 2),
                            GetValueFromRegex(line, _messageRegex, 1),
                            GetValueFromRegex(line, _filePathRegex, 1),
                            GetLineNumber(line));
                        logEntries.Add(logEntry);

                        if (verbose)
                        {
                            Console.WriteLine($"Found error line.{logEntry}");
                        }
                    }
                    else if (Regex.IsMatch(line, ": warning", RegexOptions.IgnoreCase) && !Regex.IsMatch(line, @"^\s*\d"))
                    {
                        warningCount++;

                        LogEntry logEntry = new LogEntry(
                             LogEntryType.Warning,
                             GetValueFromRegex(line, _codeRegex, 2),
                             GetValueFromRegex(line, _messageRegex, 1),
                             GetValueFromRegex(line, _filePathRegex, 1),
                             GetLineNumber(line));
                        logEntries.Add(logEntry);

                        if (verbose)
                        {
                            Console.WriteLine($"Found warning line.{logEntry}");
                        }
                    }
                }

                if (verbose)
                {
                    Console.WriteLine($"Processed {lineCount} line(s).");
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
                Console.WriteLine("Starting extraction of log entries...");
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

        public TextLogProcessor(IFileSystem fileSystem)
            : base(fileSystem)
        {
        }

        public TextLogProcessor()
           : this(new FileSystem())
        {
        }
    }
}
