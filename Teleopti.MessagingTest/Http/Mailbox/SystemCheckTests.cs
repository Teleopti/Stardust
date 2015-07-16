using System;
using System.Net;
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
	public class SystemCheckTests : ISetup
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
		public void ShouldGiveWarningMessage()
		{
			SystemCheck.WarningText.Should().Be("Could not get messages from message broker");
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
	}
}