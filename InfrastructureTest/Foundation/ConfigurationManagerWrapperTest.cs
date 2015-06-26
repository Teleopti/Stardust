using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture, Category("LongRunning")]
	public class ConfigurationManagerWrapperTest
	{
		private IConfigurationWrapper target;

		[SetUp]
		public void Setup()
		{
			target = new ConfigurationManagerWrapper();
		}

		[Test]
		public void ShouldBeEmptyFromStart()
		{
			target.AppSettings.Count.Should().Be.GreaterThan(0);
		}
	}
}