using System;
using System.Linq;
using System.Net;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.SystemCheck;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Messaging;

namespace Teleopti.MessagingTest.Http.Mailbox
{
	[TestFixture]
	[MessagingTest]
	public class SystemCheckTest
	{
		public IMessageListener Target;
		public MessageBrokerServerBridge Server;
		public FakeTime Time;
		public SystemCheckerValidator SystemCheck;
		
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
		public void ShouldNotBeOkWhenPollingWhenServerIsDownWithFriendlyResponse()
		{
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);
			Time.Passes("60".Seconds());

			Server.DownFriendly();
			Time.Passes("60".Seconds());

			SystemCheck.IsOk().Should().Be.False();
		}

		[Test]
		public void ShouldBeOk()
		{
			Target.RegisterSubscription(string.Empty, Guid.Empty,(sender, args)=>{}, typeof(ITestType), false, true);
			Server.Receives(new TestMessage
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
			Server.IsHappy();
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);

			SystemCheck.IsOk().Should().Be.False();

			Time.Passes("30".Seconds());
			SystemCheck.IsOk().Should().Be.True();
		}
		
	}
}