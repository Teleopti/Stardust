using Microsoft.AspNet.SignalR;
using NUnit.Framework;
using Teleopti.Ccc.Web.Broker;

namespace Teleopti.Ccc.WebTest.Core.MessageBroker
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