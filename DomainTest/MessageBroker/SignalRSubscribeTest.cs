using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Messaging;

namespace Teleopti.Ccc.DomainTest.MessageBroker
{
	[TestFixture]
	[MessagingTest]
	public class SignalRSubscribeTest : IIsolateSystem
	{
		public IMessageBrokerServer Server;
		public FakeSignalR SignalR;
		public FakeCurrentDatasource Datasource;
		public FakeCurrentBusinessUnit BusinessUnit;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeCurrentDatasource(new DataSourceState())).For<ICurrentDataSource>();
			isolate.UseTestDouble(new FakeCurrentBusinessUnit()).For<ICurrentBusinessUnit>();
		}
		
		[Test]
		public void ShouldAddSubscriptionToSignalR()
		{
			var subscription = new Subscription();

			Server.AddSubscription(subscription, "connectionId");

			SignalR.AddedConnection.Should().Be("connectionId");
			SignalR.AddedConnectionToGroup.Should().Be(RouteToGroupName.Convert(subscription.Route()));
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
			SignalR.RemovedConnectionFromGroup.Should().Be(RouteToGroupName.Convert(route));
		}

	}
}