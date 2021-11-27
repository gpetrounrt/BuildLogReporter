using System.Reflection;

namespace BuildLogReporter.IntegrationTests.Fixtures
{
    public sealed class ProgramFixture
    {
        public string TestProjectsDirectory { get; }

        public string TestProjectsBuiltDirectory { get; }

        public ProgramFixture()
        {
            TestProjectsDirectory = Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    @"..\..\..\..\tests"));

#if DEBUG
            const string Configuration = "Debug";
#else
            const string Configuration = "Release";
#endif

            TestProjectsBuiltDirectory = Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    $@"..\..\..\..\artifacts\Built\{Configuration}"));
        }
    }
}
