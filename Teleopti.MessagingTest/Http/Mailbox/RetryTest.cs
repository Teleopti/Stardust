using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Messaging.Client.Http;

namespace Teleopti.MessagingTest.Http.Mailbox
{
	[TestFixture]
	[IoCTest]
	[Toggle(Toggles.MessageBroker_SchedulingScreenMailbox_32733)]
	public class RetryTest : ISetup
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
	}
}