using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
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

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeSignalRClient>().For<ISignalRClient>();
			system.UseTestDouble<FakeHttpServer>().For<IHttpServer>();
			system.UseTestDouble(new FakeConfigurationWrapper
			{
				AppSettings = new Dictionary<string, string>
				{
					{"MessageBrokerMailboxPollingIntervalInSeconds", "0.01"}
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

			Assert.That(() => wasEventHandlerCalled, Is.True.After(500, 10));
		}

		[Test]
		public void ShouldUseAppsettingsForPollingInterval()
		{
			var wasEventHandlerCalled = false;
			ConfigReader.AppSettings = new Dictionary<string, string>
			{
				{"MessageBrokerMailboxPollingInterval", "1000"}
			};
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true, typeof(ITestType), false, true);

			Server.Has(new testMessage
			{
				BusinessUnitId = Guid.Empty.ToString(),
				DataSource = string.Empty,
				DomainQualifiedType = "ITestType",
				DomainType = "ITestType"
			});

			Assert.That(() => wasEventHandlerCalled, Is.False.After(500));
		}

		[Test]
		public void ShouldUnsubscribe()
		{
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

			Assert.That(() => wasCalled_pleaseForgiveMeForSharingState, Is.False.After(500));
			wasCalled_pleaseForgiveMeForSharingState = false;
		}

		[Test]
		public void ShouldUnsubscribeWithBase64Encoding()
		{
			Target.RegisterSubscription(string.Empty, Guid.Empty, EventMessageHandler, typeof(ITestType), true, true);
			Server.Has(new testMessage
			{
				BusinessUnitId = Guid.Empty.ToString(),
				DataSource = string.Empty,
				DomainQualifiedType = "ITestType",
				DomainType = "ITestType",
			});
			Assert.That(() => wasCalled_pleaseForgiveMeForSharingState, Is.True.After(500, 10));
			wasCalled_pleaseForgiveMeForSharingState = false;

			Target.UnregisterSubscription(EventMessageHandler);
			Server.Has(new testMessage
			{
				BusinessUnitId = Guid.Empty.ToString(),
				DataSource = string.Empty,
				DomainQualifiedType = "ITestType",
				DomainType = "ITestType",
			});

			Assert.That(() => wasCalled_pleaseForgiveMeForSharingState, Is.False.After(500));
			wasCalled_pleaseForgiveMeForSharingState = false;
		}

		private bool wasCalled_pleaseForgiveMeForSharingState;
		private void EventMessageHandler(object o, EventMessageArgs eventMessageArgs)
		{
			wasCalled_pleaseForgiveMeForSharingState = true;
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