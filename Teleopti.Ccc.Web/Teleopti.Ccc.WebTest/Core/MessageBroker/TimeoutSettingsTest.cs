using NUnit.Framework;
using Teleopti.Ccc.Web.Broker;

namespace Teleopti.Ccc.WebTest.Core.MessageBroker
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