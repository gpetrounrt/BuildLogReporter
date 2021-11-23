namespace BuildLogReporter.Entries
{
    public readonly struct LogEntry : IEquatable<LogEntry>
    {
        public LogEntryType Type { get; }

        public string Code { get; }

        public string Message { get; }

        public string FilePath { get; }

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
