using System.Xml;
using System.Xml.Serialization;
using BuildLogReporter.Processors;

namespace BuildLogReporter.Reporters
{
    public sealed class XmlReporter : Reporter
    {
        private const string ExtensionValue = "xml";

        public override string Extension => ExtensionValue;

        public override string GetReportAsString(ProcessedLogResult processedLogResult)
        {
            var xmlWriterSettings = new XmlWriterSettings() { Indent = true };
            using var stringWriter = new Utf8StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings);
            var xmlSerializer = new XmlSerializer(typeof(ProcessedLogResult));
            xmlSerializer.Serialize(xmlWriter, processedLogResult);

            return stringWriter.ToString();
        }
    }
}
