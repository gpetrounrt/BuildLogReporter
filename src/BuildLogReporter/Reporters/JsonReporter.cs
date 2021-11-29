using System.Text.Json;
using BuildLogReporter.Processors;

namespace BuildLogReporter.Reporters
{
    public sealed class JsonReporter : Reporter
    {
        private const string ExtensionValue = "json";

        public override string Extension => ExtensionValue;

        public override string GetReportAsString(ProcessedLogResult processedLogResult)
        {
            var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

            return JsonSerializer.Serialize(processedLogResult, jsonSerializerOptions);
        }
    }
}
