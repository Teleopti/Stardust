using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.MessagingTest.SignalR
{
	[TestFixture]
	[IoCTest]
	public class SignalRSubscriptionsTest : ISetup
	{
		public IMessageListener Target;
		public FakeSignalRClient SignalRClient;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeSignalRClient>().For<ISignalRClient>();
		}

		[Test]
		public void ShouldInvokeSubscriptionCallback()
		{
			var wasEventHandlerCalled = false;
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true, typeof(ITestType));

			SignalRClient.RegisteredCallback.Invoke(new testMessage
			{
				BusinessUnitId = Guid.Empty.ToString(),
				DataSource = string.Empty,
				DomainQualifiedType = "ITestType",
				DomainType = "ITestType",
			});

			wasEventHandlerCalled.Should().Be(true);
		}

		private class testMessage : Message
		{
			public testMessage()
			{
				StartDate = Subscription.DateToString(DateTime.UtcNow);
				EndDate = Subscription.DateToString(DateTime.UtcNow);
			}
		}

		private interface ITestType
		{

		}
	}
}