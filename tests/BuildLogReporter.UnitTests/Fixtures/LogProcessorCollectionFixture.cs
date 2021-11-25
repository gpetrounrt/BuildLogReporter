using Xunit;

namespace BuildLogReporter.UnitTests.Fixtures
{
    [CollectionDefinition(nameof(LogProcessorCollectionFixture))]
    public sealed class LogProcessorCollectionFixture : ICollectionFixture<LogProcessorFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
