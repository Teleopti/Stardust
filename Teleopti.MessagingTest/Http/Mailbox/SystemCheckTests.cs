using System;
using System.Linq;
using System.Net;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.SystemCheck;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Messaging.Client;
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
		public SystemCheckerValidator SystemCheck;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUrl("http://someserver/")).For<IMessageBrokerUrl>();
			system.UseTestDouble<FakeHttpServer>().For<IHttpServer>();
			system.UseTestDouble<FakeTime>().For<ITime>();
			system.UseTestDouble(new FakeConfigReader("MessageBrokerMailboxPollingIntervalInSeconds", "60")).For<IConfigReader>();
		}

		[Test]
		public void ShouldNotBeOkWhenSubscribingWithServerError()
		{
			Server.GivesError(HttpStatusCode.ServiceUnavailable);

			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);

			SystemCheck.IsOk().Should().Be.False();
		}

		[Test]
		public void ShouldNotBeOkWhenPollingGivesServerError()
		{
			Target.RegisterSubscription(string.Empty, Guid.Empty,(sender, args)=>{}, typeof(ITestType), false, true);
			Time.Passes("60".Seconds());

			Server.GivesError(HttpStatusCode.ServiceUnavailable);
			Time.Passes("60".Seconds());

			SystemCheck.IsOk().Should().Be.False();
		}

		[Test]
		public void ShouldNotBeOkWhenPollingWhenServerIsDown()
		{
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);
			Time.Passes("60".Seconds());

			Server.Down();
			Time.Passes("60".Seconds());

			SystemCheck.IsOk().Should().Be.False();
		}

		[Test]
		public void ShouldBeOk()
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

			SystemCheck.IsOk().Should().Be.True();
		}

		[Test]
		public void ShouldDefaultToOk()
		{
			SystemCheck.IsOk().Should().Be.True();
		}

		[Test]
		public void ShouldGiveWarningMessage()
		{
			Server.GivesError(HttpStatusCode.ServiceUnavailable);
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);

			SystemCheck.IsOk();
			
			SystemCheck.Result.Single().Should().Be("Could not get messages from message broker");
		}

		[Test]
		public void ShouldNotBeOkIfAnyCreationOfMailboxIsNotWorking()
		{
			Server.GivesError(HttpStatusCode.ServiceUnavailable);

			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);
			Time.Passes("30".Seconds());
			Server.Succeeds();
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);

			SystemCheck.IsOk().Should().Be.False();

			Time.Passes("30".Seconds());
			SystemCheck.IsOk().Should().Be.True();
		}
	}
}