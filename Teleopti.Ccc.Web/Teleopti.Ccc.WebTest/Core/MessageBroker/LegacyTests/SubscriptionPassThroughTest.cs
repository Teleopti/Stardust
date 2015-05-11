using NUnit.Framework;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.WebTest.Core.MessageBroker.LegacyTests
{
	[TestFixture]
	public class SubscriptionPassThroughTest
	{
		[Test]
		public void ShouldBeReallyLazy()
		{
			var subscription = new Subscription();
			var target = new SubscriptionPassThrough();

			target.Invoke(subscription);
		}
	}
}