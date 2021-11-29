using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BuildLogReporter.Processors
{
    public sealed class ProcessedLogResult : IXmlSerializable
    {
        [JsonPropertyName("error_count")]
        public ushort ErrorCount { get; }

        [JsonPropertyName("warning_count")]
        public ushort WarningCount { get; }

        [JsonPropertyName("log_entries")]
        public ReadOnlyCollection<LogEntry> LogEntries { get; }

        public XmlSchema? GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
        }

        public void WriteXml(XmlWriter writer)
        {
            ArgumentNullException.ThrowIfNull(writer);

            writer.WriteAttributeString(nameof(ErrorCount), ErrorCount.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(nameof(WarningCount), WarningCount.ToString(CultureInfo.InvariantCulture));
            writer.WriteStartElement(nameof(LogEntries));
            foreach (var logEntry in LogEntries)
            {
                logEntry.WriteXml(writer);
            }

            writer.WriteEndElement();
        }

        public ProcessedLogResult(
            ushort errorCount,
            ushort warningCount,
            ReadOnlyCollection<LogEntry> logEntries)
        {
            ErrorCount = errorCount;
            WarningCount = warningCount;
            LogEntries = logEntries;
        }

        public ProcessedLogResult()
            : this(0, 0, new List<LogEntry>().AsReadOnly())
        {
        }
    }
}
