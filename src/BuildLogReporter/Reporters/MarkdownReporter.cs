using System.Globalization;
using System.Text;
using BuildLogReporter.Processors;

namespace BuildLogReporter.Reporters
{
    public sealed class MarkdownReporter : Reporter
    {
        private const string ExtensionValue = "md";

        public override string Extension => ExtensionValue;

        public override string GetReportAsString(ProcessedLogResult processedLogResult)
        {
            var reportStringBuilder = new StringBuilder();
            reportStringBuilder
                .AppendLine(CultureInfo.InvariantCulture, $"Errors: {processedLogResult.ErrorCount}, Warnings: {processedLogResult.WarningCount}")
                .AppendLine()
                .AppendLine("| Type | Code | Message | File path | Line number |")
                .AppendLine("|:---|:---|:---|:---|:---|");

            foreach (var logEntry in processedLogResult.LogEntries)
            {
                reportStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"| {logEntry.Type} | {logEntry.Code} | {logEntry.Message} | {logEntry.FilePath} | {logEntry.LineNumber} |");
            }

            return reportStringBuilder.ToString();
        }
    }
}
