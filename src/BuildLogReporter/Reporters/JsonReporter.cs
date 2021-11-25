using System.Text.Json;
using BuildLogReporter.Processors;

namespace BuildLogReporter.Reporters
{
    public sealed class JsonReporter : Reporter
    {
        public override string GetReportAsString(ProcessedLogResult processedLogResult)
        {
            var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

            return JsonSerializer.Serialize(processedLogResult, jsonSerializerOptions);
        }
    }
}
