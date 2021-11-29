using System.Globalization;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BuildLogReporter.Processors
{
    public readonly struct LogEntry : IEquatable<LogEntry>, IXmlSerializable
    {
        [JsonPropertyName("type")]
        public LogEntryType Type { get; }

        [JsonPropertyName("code")]
        public string Code { get; }

        [JsonPropertyName("message")]
        public string Message { get; }

        [JsonPropertyName("file_path")]
        public string FilePath { get; }

        [JsonPropertyName("line_number")]
        public int LineNumber { get; }

        public static bool operator ==(LogEntry first, LogEntry second) =>
            first.Equals(second);

        public static bool operator !=(LogEntry first, LogEntry second) =>
            !first.Equals(second);

        public override int GetHashCode()
        {
            unchecked
            {
                var result = 0;
                result = (result * 397) ^ (int)Type;
                result = (result * 397) ^ Code.GetHashCode(StringComparison.Ordinal);
                result = (result * 397) ^ Message.GetHashCode(StringComparison.Ordinal);
                result = (result * 397) ^ FilePath.GetHashCode(StringComparison.Ordinal);
                result = (result * 397) ^ LineNumber;

                return result;
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is not LogEntry)
            {
                return false;
            }

            return Equals((LogEntry)obj);
        }

        public bool Equals(LogEntry other) =>
            Type == other.Type
            && Code == other.Code
            && Message == other.Message
            && FilePath == other.FilePath
            && LineNumber == other.LineNumber;

        public override string ToString() => $@"
Type: {Type}
Code: {Code}
Message: {Message}
File path: {FilePath}
Line Number: {LineNumber}";

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

            writer.WriteStartElement(nameof(LogEntry));
            writer.WriteAttributeString(nameof(Type), Type.ToString());
            writer.WriteAttributeString(nameof(Code), Code);
            writer.WriteAttributeString(nameof(Message), Message);
            writer.WriteAttributeString(nameof(FilePath), FilePath);
            writer.WriteAttributeString(nameof(LineNumber), LineNumber.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }

        public LogEntry(
            LogEntryType type,
            string code,
            string message,
            string filePath,
            int lineNumber)
        {
            Type = type;
            Code = code;
            Message = message;
            FilePath = filePath;
            LineNumber = lineNumber;
        }
    }
}
