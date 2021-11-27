using System.Text;
using BuildLogReporter.Processors;

namespace BuildLogReporter.Reporters
{
    public sealed class MarkdownReporter : Reporter
    {
        public override string GetReportAsString(ProcessedLogResult processedLogResult)
        {
            var reportStringBuilder = new StringBuilder();
            reportStringBuilder
                .AppendLine($"Errors: {processedLogResult.ErrorCount}, Warnings: {processedLogResult.WarningCount}")
                .AppendLine()
                .AppendLine($"| Type | Code | Message | File path | Line number |")
                .AppendLine($"|:---|:---|:---|:---|:---|");

            foreach (var logEntry in processedLogResult.LogEntries)
            {
                reportStringBuilder.AppendLine($"| {logEntry.Type} | {logEntry.Code} | {logEntry.Message} | {logEntry.FilePath} | {logEntry.LineNumber} |");
            }

            return reportStringBuilder.ToString();
        }
    }
}
