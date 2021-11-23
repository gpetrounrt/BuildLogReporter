namespace BuildLogReporter.UnitTests.Diagnostics
{
    public sealed class ConsoleRecorder : IDisposable
    {
        private readonly StringWriter _outputStringWriter;

        private readonly StringWriter _errorStringWriter;

        private readonly TextWriter _originalOutputTextWriter;

        private readonly TextWriter _originalErrorTextWriter;

        public string GetOutput() =>
            _outputStringWriter.ToString();

        public string GetError() =>
            _errorStringWriter.ToString();

        public void Dispose()
        {
            Console.SetOut(_originalOutputTextWriter);
            _outputStringWriter.Dispose();

            Console.SetError(_originalErrorTextWriter);
            _errorStringWriter.Dispose();
        }

        public ConsoleRecorder()
        {
            _outputStringWriter = new StringWriter();
            _originalOutputTextWriter = Console.Out;
            Console.SetOut(_outputStringWriter);

            _errorStringWriter = new StringWriter();
            _originalErrorTextWriter = Console.Error;
            Console.SetError(_errorStringWriter);
        }
    }
}
