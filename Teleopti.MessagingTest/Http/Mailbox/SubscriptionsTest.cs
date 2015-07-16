using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Client.Http;

namespace Teleopti.MessagingTest.Http.Mailbox
{
	[TestFixture]
	[IoCTest]
	[Toggle(Toggles.MessageBroker_SchedulingScreenMailbox_32733)]
	public class SubscriptionsTest : ISetup
	{
		public IMessageListener Target;
		public FakeHttpServer Server;
		public FakeTime Time;
		public ISystemCheck SystemCheck;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			var fakeSignalRClient = new FakeSignalRClient();
			fakeSignalRClient.Configure("http://someserver/");
			system.UseTestDouble(fakeSignalRClient).For<ISignalRClient>();
			system.UseTestDouble<FakeHttpServer>().For<IHttpServer>();
			system.UseTestDouble<FakeTime>().For<ITime>();
			var config = new FakeConfigReader();
			config.AppSettings_DontUse["MessageBrokerMailboxPollingIntervalInSeconds"] = "60";
			system.UseTestDouble(config).For<IConfigReader>();
		}

		[Test]
		public void ShouldInvokeSubscriptionCallback()
		{
			var wasEventHandlerCalled = false;
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true, typeof(ITestType), false, true);

			Server.Has(new testMessage
			{
				BusinessUnitId = Guid.Empty.ToString(),
				DataSource = string.Empty,
				DomainQualifiedType = "ITestType",
				DomainType = "ITestType"
			});
			Time.Passes("60".Seconds());

			wasEventHandlerCalled.Should().Be.True();
		}

		[Test]
		public void ShouldOnlyCallServerForSubscriptionOnce()
		{
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof (ITestType), false, true);
			Time.Passes("15".Minutes());
			Server.CallsToCreateMailbox.Should().Be(1);
		}

		private bool wasCalled_pleaseForgiveMeForSharingState;
		private void EventMessageHandler(object o, EventMessageArgs eventMessageArgs)
		{
			wasCalled_pleaseForgiveMeForSharingState = true;
		}

		[Test]
		public void ShouldUnsubscribe()
		{
			wasCalled_pleaseForgiveMeForSharingState = false;
			Target.RegisterSubscription(string.Empty, Guid.Empty, EventMessageHandler, typeof(ITestType), false, true);
			Target.UnregisterSubscription(EventMessageHandler);

			Server.Has(new testMessage
			{
				BusinessUnitId = Guid.Empty.ToString(),
				DataSource = string.Empty,
				DomainQualifiedType = "ITestType",
				DomainType = "ITestType",
				BinaryData = "false"
			});
			Time.Passes("60".Seconds());

			wasCalled_pleaseForgiveMeForSharingState.Should().Be.False();
			wasCalled_pleaseForgiveMeForSharingState = false;
		}

		[Test]
		public void ShouldUnsubscribeWithBase64Encoding()
		{
			wasCalled_pleaseForgiveMeForSharingState = false;
			Target.RegisterSubscription(string.Empty, Guid.Empty, EventMessageHandler, typeof(ITestType), true, true);
			Server.Has(new testMessage
			{
				BusinessUnitId = Guid.Empty.ToString(),
				DataSource = string.Empty,
				DomainQualifiedType = "ITestType",
				DomainType = "ITestType",
			});
			Time.Passes("60".Seconds());
			wasCalled_pleaseForgiveMeForSharingState.Should().Be.True();
			wasCalled_pleaseForgiveMeForSharingState = false;

			Target.UnregisterSubscription(EventMessageHandler);
			Server.Has(new testMessage
			{
				BusinessUnitId = Guid.Empty.ToString(),
				DataSource = string.Empty,
				DomainQualifiedType = "ITestType",
				DomainType = "ITestType",
			});
			Time.Passes("60".Seconds());

			wasCalled_pleaseForgiveMeForSharingState.Should().Be.False();
			wasCalled_pleaseForgiveMeForSharingState = false;
		}

		[Test]
		public void ShouldStopTimersWhenUnsubscribing()
		{
			Target.RegisterSubscription(string.Empty, Guid.Empty, EventMessageHandler, typeof(ITestType), false, true);
			
			Target.UnregisterSubscription(EventMessageHandler);

			Time.ActiveTimers().Should().Be(0);
		}
	}
}