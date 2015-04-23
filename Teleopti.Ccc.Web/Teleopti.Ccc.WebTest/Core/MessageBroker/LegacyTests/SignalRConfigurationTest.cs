using NUnit.Framework;
using Teleopti.Ccc.Web.Broker;

namespace Teleopti.Ccc.WebTest.Core.MessageBroker.LegacyTests
{
	[TestFixture]
	public class SignalRConfigurationTest
	{
		[Test]
		public void ShouldConfigure()
		{
			SignalRConfiguration.Configure(() => { });
		}
	}
}