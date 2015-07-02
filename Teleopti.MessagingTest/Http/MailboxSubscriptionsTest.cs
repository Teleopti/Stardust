using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Client.Http;

namespace Teleopti.MessagingTest.Http
{
	[TestFixture]
	[IoCTest]
	[Toggle(Toggles.MessageBroker_SchedulingScreenMailbox_32733)]
	public class MailboxSubscriptionsTest : ISetup
	{
		public IMessageListener Target;
		public FakeConfigurationWrapper ConfigReader;
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
			system.UseTestDouble(new FakeConfigurationWrapper
			{
				AppSettings = new Dictionary<string, string>
				{
					{"MessageBrokerMailboxPollingIntervalInSeconds", "60"}
				}
			}).For<IConfigurationWrapper>();
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

		private bool wasCalled_pleaseForgiveMeForSharingState;
		private void EventMessageHandler(object o, EventMessageArgs eventMessageArgs)
		{
			wasCalled_pleaseForgiveMeForSharingState = true;
		}


		[Test]
		public void ShouldTellIfPollingIsNotWorking()
		{
			Target.RegisterSubscription(string.Empty, Guid.Empty,(sender, args)=>{}, typeof(ITestType), false, true);
			Time.Passes("60".Seconds());

			Server.Fails(HttpStatusCode.ServiceUnavailable);
			Time.Passes("60".Seconds());

			SystemCheck.IsRunningOk().Should().Be.False();
		}

		[Test]
		public void ShouldTellIfPollingIsWorking()
		{
			Target.RegisterSubscription(string.Empty, Guid.Empty,(sender, args)=>{}, typeof(ITestType), false, true);
			Server.Has(new testMessage
			{
				BusinessUnitId = Guid.Empty.ToString(),
				DataSource = string.Empty,
				DomainQualifiedType = "ITestType",
				DomainType = "ITestType",
			});
			Time.Passes("60".Seconds());

			SystemCheck.IsRunningOk().Should().Be.True();
		}

		[Test]
		public void ShouldNotTellThatPollingIsNotWorkingBeforeTrying()
		{
			SystemCheck.IsRunningOk().Should().Be.True();
		}

		[Test]
		public void ShouldTellIfCreatingMailboxIsNotWorking()
		{
			Server.Fails(HttpStatusCode.ServiceUnavailable);
			
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);
			
			SystemCheck.IsRunningOk().Should().Be.False();
		}


		[Test]
		public void ShouldTellIfAnyCreationOfMailboxIsNotWorking()
		{
			Server.Fails(HttpStatusCode.ServiceUnavailable);

			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);
			Time.Passes("30".Seconds());
			Server.Succeds();
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);

			SystemCheck.IsRunningOk().Should().Be.False();

			Time.Passes("30".Seconds());
			SystemCheck.IsRunningOk().Should().Be.True();
		}

		
		[Test]
		public void ShouldRetryToAddMailboxIfItFails()
		{
			var wasEventHandlerCalled = false;
			Server.Fails(HttpStatusCode.ServiceUnavailable);
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true, typeof(ITestType), false, true);
			Time.Passes("60".Seconds());
			Server.Succeds();

			Server.Has(new testMessage
			{
				BusinessUnitId = Guid.Empty.ToString(),
				DataSource = string.Empty,
				DomainQualifiedType = "ITestType",
				DomainType = "ITestType",
			});
			Time.Passes("120".Seconds());

			wasEventHandlerCalled.Should().Be.True();
		}

		[Test]
		public void ShouldRetryAddingMailboxBasedOnTimer()
		{
			Server.Fails(HttpStatusCode.ServiceUnavailable);
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);
			Time.Passes("60".Seconds());

			Server.CallsToCreateMailbox.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldRetryAddingMailboxBasedOnTimerForTwoSubscriptions()
		{
			Server.Fails(HttpStatusCode.ServiceUnavailable);
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);
			Time.Passes("30".Seconds());
			Server.Succeds();
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);
			Time.Passes("30".Seconds());

			Server.CallsToCreateMailbox.Should().Be.EqualTo(3);
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