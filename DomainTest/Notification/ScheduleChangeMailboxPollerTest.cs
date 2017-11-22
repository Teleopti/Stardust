using NUnit.Framework;
using System;
using System.Linq;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Interfaces.Domain;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;

namespace Teleopti.Ccc.DomainTest.Notification
{
	[TestFixture, DomainTest]
	public class ScheduleChangeMailboxPollerTest : ISetup
	{
		public ScheduleChangeMailboxPoller Target;
		public FakeMailboxRepository FakeMailboxRepository;
		public FakeScenarioRepository ScenarioRepository;
		public DefaultScenarioFromRepository DefaultScenarioFromRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public MutableNow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<ScheduleChangeMailboxPoller>();
			system.UseTestDouble<FakeMailboxRepository>().For<IMailboxRepository>();
			system.UseTestDouble<FakeScenarioRepository>().For<IScenarioRepository>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<DefaultScenarioFromRepository>().For<ICurrentScenario>();
			system.UseTestDouble<MutableNow>().For<INow>();
		}

		[Test]
		public void ShouldInitMailboxWhenStartPolling()
		{
			var scenario = ScenarioFactory.CreateScenario("test", false, false).WithId();
			scenario.DefaultScenario = true;
			ScenarioRepository.Add(scenario);
			var me = PersonFactory.CreatePerson().WithId();
			LoggedOnUser.SetFakeLoggedOnUser(me);

			var mailboxId = Target.StartPolling();
			
			mailboxId.Should().Be.EqualTo(FakeMailboxRepository.Data.Single().Id);
		}

		[Test]
		public void ShouldReturnUpdatedPeriodsWhenHasScheduleChangedMessageWithinPollInterval()
		{
			var scenario = ScenarioFactory.CreateScenario("test", false, false).WithId();
			ScenarioRepository.Add(scenario);
			var me = PersonFactory.CreatePerson().WithId();
			LoggedOnUser.SetFakeLoggedOnUser(me);
			me.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var mailboxId = Guid.NewGuid();

			var scheduleChangeMessage = new Message
			{
				StartDate = Subscription.DateToString(new DateTime(2017, 11, 17, 8, 0, 0)),
				EndDate = Subscription.DateToString(new DateTime(2017, 11, 17, 10, 0, 0)),
				DomainReferenceId = scenario.Id.Value.ToString(),
				DomainUpdateType = (int)DomainUpdateType.NotApplicable,
				DomainQualifiedType = typeof(IScheduleChangedMessage).AssemblyQualifiedName,
				DomainType = typeof(IScheduleChangedMessage).Name,
				BinaryData = string.Join(",", Enumerable.Range(0, 1000)
					.Select(_ => me.Id.ToString()))
			};

			var scheduleChangeMessage2 = new Message
			{
				StartDate = Subscription.DateToString(new DateTime(2017, 11, 17, 10, 0, 0)),
				EndDate = Subscription.DateToString(new DateTime(2017, 11, 17, 11, 0, 0)),
				DomainReferenceId = scenario.Id.Value.ToString(),
				DomainUpdateType = (int)DomainUpdateType.NotApplicable,
				DomainQualifiedType = typeof(IScheduleChangedMessage).AssemblyQualifiedName,
				DomainType = typeof(IScheduleChangedMessage).Name,
				BinaryData = string.Join(",", Enumerable.Range(0, 1000)
					.Select(_ => me.Id.ToString()))
			};

			FakeMailboxRepository.Add(new Mailbox
			{
				Id = mailboxId,
				Route = scheduleChangeMessage.Routes().First(),
				ExpiresAt = Now.UtcDateTime().Add(TimeSpan.FromSeconds(15 * 60))
			});

			FakeMailboxRepository.AddMessage(scheduleChangeMessage);
			FakeMailboxRepository.AddMessage(scheduleChangeMessage2);

			var period = new PollerInputPeriod(new DateTime(2017, 11, 17), new DateTime(2017, 11, 17));
			Target.Check(mailboxId, period)[period].Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotReturnUpdatedPeriodWhenHasScheduleChangedMessageOutsideOfPollInterval()
		{
			var scenario = ScenarioFactory.CreateScenario("test", false, false).WithId();
			ScenarioRepository.Add(scenario);
			var me = PersonFactory.CreatePerson().WithId();
			LoggedOnUser.SetFakeLoggedOnUser(me);
			var mailboxId = Guid.NewGuid();

			var scheduleChangeMessage = new Message
			{
				StartDate = Subscription.DateToString(new DateTime(2017, 11, 15, 8, 0, 0)),
				EndDate = Subscription.DateToString(new DateTime(2017, 11, 15, 10, 0, 0)),
				DomainReferenceId = scenario.Id.Value.ToString(),
				DomainUpdateType = (int)DomainUpdateType.NotApplicable,
				DomainQualifiedType = typeof(IScheduleChangedMessage).AssemblyQualifiedName,
				DomainType = typeof(IScheduleChangedMessage).Name,
				BinaryData = string.Join(",", Enumerable.Range(0, 1000)
					.Select(_ => me.Id.ToString()))
			};

			FakeMailboxRepository.Add(new Mailbox
			{
				Id = mailboxId,
				Route = scheduleChangeMessage.Routes().First(),
				ExpiresAt = Now.UtcDateTime().Add(TimeSpan.FromSeconds(15 * 60))
			});

			FakeMailboxRepository.AddMessage(scheduleChangeMessage);

			var period = new PollerInputPeriod(new DateTime(2017, 11, 17), new DateTime(2017, 11, 17));
			Target.Check(mailboxId, period)[period].Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnUpdatedPeriodWhenHasScheduleChangedMessagePeriodIntersectWithPollInterval()
		{
			var scenario = ScenarioFactory.CreateScenario("test", false, false).WithId();
			ScenarioRepository.Add(scenario);
			var me = PersonFactory.CreatePerson().WithId();
			LoggedOnUser.SetFakeLoggedOnUser(me);
			var mailboxId = Guid.NewGuid();


			var scheduleChangeMessage = new Message
			{
				StartDate = Subscription.DateToString(new DateTime(2017, 11, 16, 22, 0, 0)),
				EndDate = Subscription.DateToString(new DateTime(2017, 11, 17, 5, 0, 0)),
				DomainReferenceId = scenario.Id.Value.ToString(),
				DomainUpdateType = (int)DomainUpdateType.NotApplicable,
				DomainQualifiedType = typeof(IScheduleChangedMessage).AssemblyQualifiedName,
				DomainType = typeof(IScheduleChangedMessage).Name,
				BinaryData = string.Join(",", Enumerable.Range(0, 1000)
					.Select(_ => me.Id.ToString()))
			};

			FakeMailboxRepository.Add(new Mailbox
			{
				Id = mailboxId,
				Route = scheduleChangeMessage.Routes().First(),
				ExpiresAt = Now.UtcDateTime().Add(TimeSpan.FromSeconds(15 * 60))
			});

			FakeMailboxRepository.AddMessage(scheduleChangeMessage);

			var period = new PollerInputPeriod(new DateTime(2017, 11, 17), new DateTime(2017, 11, 17));
			Target.Check(mailboxId, period)[period].Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPollIntervalBeComparedBasedOnUtcTime()
		{
			var scenario = ScenarioFactory.CreateScenario("test", false, false).WithId();
			ScenarioRepository.Add(scenario);
			var me = PersonFactory.CreatePerson().WithId();
			me.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo());
			LoggedOnUser.SetFakeLoggedOnUser(me);
			var mailboxId = Guid.NewGuid();


			var scheduleChangeMessage = new Message
			{
				StartDate = Subscription.DateToString(new DateTime(2017, 11, 21, 20, 0, 0)),
				EndDate = Subscription.DateToString(new DateTime(2017, 11, 21, 22, 0, 0)),
				DomainReferenceId = scenario.Id.Value.ToString(),
				DomainUpdateType = (int)DomainUpdateType.NotApplicable,
				DomainQualifiedType = typeof(IScheduleChangedMessage).AssemblyQualifiedName,
				DomainType = typeof(IScheduleChangedMessage).Name,
				BinaryData = string.Join(",", Enumerable.Range(0, 1000)
					.Select(_ => me.Id.ToString()))
			};

			FakeMailboxRepository.Add(new Mailbox
			{
				Id = mailboxId,
				Route = scheduleChangeMessage.Routes().First(),
				ExpiresAt = Now.UtcDateTime().Add(TimeSpan.FromSeconds(15 * 60))
			});

			FakeMailboxRepository.AddMessage(scheduleChangeMessage);
			var period = new PollerInputPeriod(new DateTime(2017, 11, 21), new DateTime(2017, 11, 21));
			Target.Check(mailboxId, period)[period].Should().Be.Empty();
		}

		[Test]
		public void ShouldResetPolling()
		{
			var scenario = ScenarioFactory.CreateScenario("test", false, false).WithId();
			ScenarioRepository.Add(scenario);
			var me = PersonFactory.CreatePerson().WithId();
			me.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo());
			LoggedOnUser.SetFakeLoggedOnUser(me);
			var mailboxId = Guid.NewGuid();


			var scheduleChangeMessage = new Message
			{
				StartDate = Subscription.DateToString(new DateTime(2017, 11, 21, 20, 0, 0)),
				EndDate = Subscription.DateToString(new DateTime(2017, 11, 21, 22, 0, 0)),
				DomainReferenceId = scenario.Id.Value.ToString(),
				DomainUpdateType = (int)DomainUpdateType.NotApplicable,
				DomainQualifiedType = typeof(IScheduleChangedMessage).AssemblyQualifiedName,
				DomainType = typeof(IScheduleChangedMessage).Name,
				BinaryData = string.Join(",", Enumerable.Range(0, 1000)
					.Select(_ => me.Id.ToString()))
			};

			var expiresAt = Now.UtcDateTime().Add(TimeSpan.FromSeconds(15 * 60));
			Now.Is(expiresAt.Subtract(new TimeSpan(TimeSpan.FromSeconds(15 * 60).Ticks/2)).AddMinutes(1));
			FakeMailboxRepository.Add(new Mailbox
			{
				Id = mailboxId,
				Route = scheduleChangeMessage.Routes().First(),
				ExpiresAt = expiresAt
			});

			FakeMailboxRepository.AddMessage(scheduleChangeMessage);

			Target.ResetPolling(mailboxId);

			FakeMailboxRepository.Load(mailboxId).ExpiresAt.Should().Be.GreaterThan(expiresAt);
			FakeMailboxRepository.PopMessages(mailboxId, null).Should().Be.Empty();
		}

		[Test]
		public void ShouldRemoveCreatedMailbox()
		{
			var scenario = ScenarioFactory.CreateScenario("test", false, false).WithId();
			ScenarioRepository.Add(scenario);
			var me = PersonFactory.CreatePerson().WithId();
			me.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo());
			LoggedOnUser.SetFakeLoggedOnUser(me);
			var mailboxId = Guid.NewGuid();


			var scheduleChangeMessage = new Message
			{
				StartDate = Subscription.DateToString(new DateTime(2017, 11, 21, 20, 0, 0)),
				EndDate = Subscription.DateToString(new DateTime(2017, 11, 21, 22, 0, 0)),
				DomainReferenceId = scenario.Id.Value.ToString(),
				DomainUpdateType = (int)DomainUpdateType.NotApplicable,
				DomainQualifiedType = typeof(IScheduleChangedMessage).AssemblyQualifiedName,
				DomainType = typeof(IScheduleChangedMessage).Name,
				BinaryData = string.Join(",", Enumerable.Range(0, 1000)
					.Select(_ => me.Id.ToString()))
			};

			var expiresAt = Now.UtcDateTime().Add(TimeSpan.FromSeconds(15 * 60));
			Now.Is(expiresAt.Subtract(new TimeSpan(TimeSpan.FromSeconds(15 * 60).Ticks / 2)).AddMinutes(1));
			FakeMailboxRepository.Add(new Mailbox
			{
				Id = mailboxId,
				Route = scheduleChangeMessage.Routes().First(),
				ExpiresAt = expiresAt
			});

			FakeMailboxRepository.AddMessage(scheduleChangeMessage);

			Target.RemoveMailbox(mailboxId);

			FakeMailboxRepository.Load(mailboxId).Should().Be.Null();
			FakeMailboxRepository.PopMessages(mailboxId, null).Should().Be.Empty();
		}
	}
}
