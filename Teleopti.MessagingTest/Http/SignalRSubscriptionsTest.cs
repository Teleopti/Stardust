using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.MessagingTest.Http
{
	[TestFixture]
	[IoCTest]
	[Toggle(Toggles.MessageBroker_SchedulingScreenMailbox_32733)]
	public class MailboxSubscriptionsTest : ISetup
	{
		public IMessageListener Target;
		public FakeHttpServer Server;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeSignalRClient>().For<ISignalRClient>();
			system.UseTestDouble<FakeHttpServer>().For<IHttpServer>();
			system.UseTestDouble(new FakeConfigReader
			{
				AppSettings = new NameValueCollection
				{
					{"MessageBrokerMailboxPollingInterval", "0.01"}
				}
			}).For<IConfigReader>();
		}

		[Test, Ignore]
		public void ShouldInvokeSubscriptionCallback()
		{
			var wasEventHandlerCalled = false;
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true, typeof(ITestType), false, true);

			Server.Has(new testMessage
			{
				BusinessUnitId = Guid.Empty.ToString(),
				DataSource = string.Empty,
				DomainQualifiedType = "ITestType",
				DomainType = "ITestType",
			});

			Assert.That(wasEventHandlerCalled, Is.True.After(500, 10));
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

	public class FakeHttpServer : IHttpServer
	{
		private readonly IJsonSerializer _serializer;
		private readonly IList<Message> _messages = new List<Message>();

		public FakeHttpServer(IJsonSerializer serializer)
		{
			_serializer = serializer;
		}

		public void Has(Message message)
		{
			_messages.Add(message);
		}

		public void PostAsync(HttpClient client, string uri, HttpContent httpContent)
		{
		}

		public string Get(HttpClient client, string uri)
		{
			var result = _serializer.SerializeObject(_messages);
			_messages.Clear();
			return result;
		}
	}

}