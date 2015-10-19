using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.MessagingTest.Http.Mailbox
{
	[TestFixture]
	[MessagingTest]
	[Toggle(Toggles.MessageBroker_SchedulingScreenMailbox_32733)]
	public class SubscriptionsTest
	{
		public IMessageListener Client;
		public FakeHttpServer Server;
		public FakeTime Time;
		public ISystemCheck SystemCheck;
		
		[Test]
		public void ShouldInvokeSubscriptionCallbackWhenPollGotAMessage()
		{
			var wasEventHandlerCalled = false;
			Client.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true, typeof(ITestType), false, true);

			Server.Has(new TestMessage
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
			Client.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof (ITestType), false, true);
			Time.Passes("15".Minutes());
			Server.Requests.Where(x => x.Uri.Contains("AddMailbox")).Should().Have.Count.EqualTo(1);
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
			Client.RegisterSubscription(string.Empty, Guid.Empty, EventMessageHandler, typeof(ITestType), false, true);
			Client.UnregisterSubscription(EventMessageHandler);

			Server.Has(new TestMessage
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
			Client.RegisterSubscription(string.Empty, Guid.Empty, EventMessageHandler, typeof(ITestType), true, true);
			Server.Has(new TestMessage
			{
				BusinessUnitId = Guid.Empty.ToString(),
				DataSource = string.Empty,
				DomainQualifiedType = "ITestType",
				DomainType = "ITestType",
			});
			Time.Passes("60".Seconds());
			wasCalled_pleaseForgiveMeForSharingState.Should().Be.True();
			wasCalled_pleaseForgiveMeForSharingState = false;

			Client.UnregisterSubscription(EventMessageHandler);
			Server.Has(new TestMessage
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
		public void ShouldThrowOnMostExceptions()
		{
			Server.Throws(new AggregateException(new NullReferenceException()));

			Assert.Throws<AggregateException>(() => Client.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true));
		}
	}
}