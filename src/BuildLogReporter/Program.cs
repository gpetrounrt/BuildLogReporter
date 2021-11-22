namespace BuildLogReporter
{
    public static class Program
    {
        public static Task<int> Main(string[] args)
        {
            var executor = new ProgramExecutor();

            return executor.ProcessFileAsync(args);
        }
    }
}
