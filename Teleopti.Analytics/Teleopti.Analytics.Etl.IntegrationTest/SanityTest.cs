using System.Configuration;
using NUnit.Framework;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
    [TestFixture]
    public class SanityTest
    {
        [Test]
        public void ShouldBeSane()
        {
            Assert.That(true, Is.True);
        }

        [Test]
        public void ShouldLoadConfig()
        {
            Assert.That(ConfigurationManager.AppSettings["configSanityCheck"],
                Is.EqualTo("Yes, configuration is loaded!"),
                "No! Configuration is not loaded!!!");
        }

    }
}