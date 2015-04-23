using NUnit.Framework;
using Teleopti.Ccc.Web.Broker;

namespace Teleopti.Ccc.WebTest.Core.MessageBroker
{
	[TestFixture]
	public class SubscriptionPassThroughTest
	{
		[Test]
		public void ShouldBeReallyLazy()
		{
			var subscription = new Interfaces.MessageBroker.Subscription();
			var target = new SubscriptionPassThrough();

			target.Invoke(subscription);
		}
	}
}