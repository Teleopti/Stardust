using System.Configuration;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture]
	public class ConfigReaderTest
	{
		[Test]
		public void ShouldReturnAppSettingsFromConfigurationManager()
		{
			//stupid test, but cost to much to test "seriously"'
			var real = ConfigurationManager.AppSettings;
			var target = new ConfigReader();
			target.AppSettings.Should().Be.EqualTo(real);
		}
	}
}