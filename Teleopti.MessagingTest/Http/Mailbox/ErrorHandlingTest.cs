using System;
using System.Net.Http;
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
using Teleopti.Messaging.Client.Http;

namespace Teleopti.MessagingTest.Http.Mailbox
{
	[TestFixture]
	[IoCTest]
	[Toggle(Toggles.MessageBroker_SchedulingScreenMailbox_32733)]
	public class ErrorHandlingTest : ISetup
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
		public void ShouldHandleHttpRequestExceptionWhenRegistratingSubscription()
		{
			Server.Throws(new AggregateException(new HttpRequestException()));
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);

			Time.Passes("60".Seconds());

			SystemCheck.IsRunningOk().Should().Be.False();
		}

		[Test]
		public void ShouldThrowIfSomeOtherExecptionWhenRegistratingSubscription()
		{
			Server.Throws(new AggregateException(new NullReferenceException()));

			Assert.Throws<AggregateException>(
				() => Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof (ITestType), false, true));
		}


		[Test]
		public void ShouldHandleHttpRequestExceptionWhenPolling()
		{
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);

			Server.Throws(new AggregateException(new HttpRequestException()));
			Time.Passes("60".Seconds());

			SystemCheck.IsRunningOk().Should().Be.False();
		}


		[Test]
		public void ShouldThrowIfSomeOtherExecptionWhenPolling()
		{
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);
			Server.Throws(new AggregateException(new NullReferenceException()));

			Assert.Throws<AggregateException>(() => Time.Passes("60".Seconds()));
		}
	}
}