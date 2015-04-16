using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Messaging.Client.SignalR;

namespace Teleopti.MessagingTest.SignalR
{
	[TestFixture]
	public class DisabledSignalRClientTest
	{
		[Test]
		public void ShouldIndicateIsAlive()
		{
			var target = new DisabledSignalRClient();
			target.IsAlive.Should().Be.True();;
		}

		[Test]
		public void ShouldConfigureUrl()
		{
			var target = new DisabledSignalRClient();
			target.Configure("my url");
			target.Url.Should().Be("my url");
		}
	}
}