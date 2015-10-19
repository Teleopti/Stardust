using System;
using System.Linq;
using System.Net;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.MessagingTest.Http.Mailbox
{
	[TestFixture]
	[MessagingTest]
	[Toggle(Toggles.MessageBroker_SchedulingScreenMailbox_32733)]
	public class RetryTest
	{
		public IMessageListener Target;
		public FakeHttpServer Server;
		public FakeTime Time;
		public ISystemCheck SystemCheck;

		[Test]
		public void ShouldRetryToAddMailboxIfServerFails()
		{
			var wasEventHandlerCalled = false;
			Server.GivesError(HttpStatusCode.ServiceUnavailable);
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true, typeof(ITestType), false, true);
			Time.Passes("60".Seconds());
			Server.Succeeds();

			Server.Has(new TestMessage
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
		public void ShouldRetryAddingMailboxBasedOnTimerIfServerFails()
		{
			Server.GivesError(HttpStatusCode.ServiceUnavailable);
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);
			Time.Passes("60".Seconds());

			Server.Requests.Where(x => x.Uri.Contains("AddMailbox")).Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldRetryAddingMailboxBasedOnTimerForTwoSubscriptionsIfServerFails()
		{
			Server.GivesError(HttpStatusCode.ServiceUnavailable);
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);
			Time.Passes("30".Seconds());
			Server.Succeeds();
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);
			Time.Passes("30".Seconds());

			Server.Requests.Where(x => x.Uri.Contains("AddMailbox")).Should().Have.Count.EqualTo(3);
		}

		[Test]
		public void ShouldRetryToAddMailboxIfNoConnection()
		{
			var wasEventHandlerCalled = false;
			Server.Down();
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true, typeof(ITestType), false, true);
			Time.Passes("60".Seconds());
			Server.Succeeds();

			Server.Has(new TestMessage
			{
				BusinessUnitId = Guid.Empty.ToString(),
				DataSource = string.Empty,
				DomainQualifiedType = "ITestType",
				DomainType = "ITestType",
			});
			Time.Passes("120".Seconds());

			wasEventHandlerCalled.Should().Be.True();
		}

	}
}