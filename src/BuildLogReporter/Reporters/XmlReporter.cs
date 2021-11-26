using System.Xml.Serialization;
using BuildLogReporter.Processors;

namespace BuildLogReporter.Reporters
{
    public sealed class XmlReporter : Reporter
    {
        public override string GetReportAsString(ProcessedLogResult processedLogResult)
        {
            var xmlSerializer = new XmlSerializer(typeof(ProcessedLogResult));
            using var stringWriter = new Utf8StringWriter();
            xmlSerializer.Serialize(stringWriter, processedLogResult);

            return stringWriter.ToString();
        }
    }
}
