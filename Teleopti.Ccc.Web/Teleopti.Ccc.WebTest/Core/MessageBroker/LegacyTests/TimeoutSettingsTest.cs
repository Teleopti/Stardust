using NUnit.Framework;
using Teleopti.Ccc.Web.Broker;

namespace Teleopti.Ccc.WebTest.Core.MessageBroker.LegacyTests
{
	[TestFixture]
	public class TimeoutSettingsTest
	{
		[Test]
		public void ShouldLoad()
		{
			TimeoutSettings.Load();
			// assert coverage
		}
	}
}