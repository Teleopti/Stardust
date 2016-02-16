using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.MessageBroker
{
	[TestFixture]
	[MessageBrokerServerTest]
	public class SignalRSubscribeTest
	{
		public IMessageBrokerServer Server;
		public FakeSignalR SignalR;
		public FakeCurrentDatasource Datasource;
		public FakeCurrentBusinessUnit BusinessUnit;

		[Test]
		public void ShouldAddSubscriptionToSignalR()
		{
			var subscription = new Subscription();

			Server.AddSubscription(subscription, "connectionId");

			SignalR.AddedConnection.Should().Be("connectionId");
			SignalR.AddedConnectionToGroup.Should().Be(MessageBrokerServer.RouteToGroupName(subscription.Route()));
		}

		[Test]
		public void ShouldReturnRouteWhenSubscribing()
		{
			var subscription = new Subscription();

			var result = Server.AddSubscription(subscription, "connectionId");

			result.Should().Be(subscription.Route());
		}

		[Test]
		public void ShouldFillInMissingDataSource()
		{
			Datasource.FakeName("datasource");
			var subscription = new Subscription();

			Server.AddSubscription(subscription, "connectionId");

			subscription.DataSource.Should().Be("datasource");
		}

		[Test]
		public void ShouldFillInMissingBusinessUnitId()
		{
			var businessUnit = BusinessUnitFactory.CreateWithId(".");
			BusinessUnit.FakeBusinessUnit(businessUnit);
			var subscription = new Subscription();

			Server.AddSubscription(subscription, "connectionId");

			subscription.BusinessUnitId.Should().Be(businessUnit.Id.Value.ToString());
		}

		[Test]
		public void ShouldRemoveSubscriptionFromSignalR()
		{
			var subscription = new Subscription();
			var route = Server.AddSubscription(subscription, "connectionId");

			Server.RemoveSubscription(route, "connectionId");

			SignalR.RemovedConnection.Should().Be("connectionId");
			SignalR.RemovedConnectionFromGroup.Should().Be(MessageBrokerServer.RouteToGroupName(route));
		}

	}
}