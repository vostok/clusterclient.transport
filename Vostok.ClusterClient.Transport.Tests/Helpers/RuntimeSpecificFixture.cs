using NUnit.Framework;
using Vostok.Commons.Environment;

namespace Vostok.Clusterclient.Transport.Tests.Helpers
{
    internal abstract class RuntimeSpecificFixture
    {
        [SetUp]
        public void CheckRuntime()
        {
            if (!RunOnCore31 && RuntimeDetector.IsDotNetCore30AndNewer)
                Assert.Pass();

            if (!RunOnCore21 && RuntimeDetector.IsDotNetCore21AndNewer)
                Assert.Pass();

            if (!RunOnCore20 && RuntimeDetector.IsDotNetCore20)
                Assert.Pass();

            if (!RunOnMono && RuntimeDetector.IsMono)
                Assert.Pass();

            if (!RunOnFramework && RuntimeDetector.IsDotNetFramework)
                Assert.Pass();
        }

        protected abstract Runtime SupportedRuntimes { get; }

        private bool RunOnMono => SupportedRuntimes.HasFlag(Runtime.Mono);

        private bool RunOnFramework => SupportedRuntimes.HasFlag(Runtime.Framework);

        private bool RunOnCore20 => SupportedRuntimes.HasFlag(Runtime.Core20);

        private bool RunOnCore21 => SupportedRuntimes.HasFlag(Runtime.Core21);

        private bool RunOnCore31 => SupportedRuntimes.HasFlag(Runtime.Core31);
    }
}
