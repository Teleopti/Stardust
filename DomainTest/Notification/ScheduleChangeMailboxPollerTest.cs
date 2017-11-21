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
		public INow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<ScheduleChangeMailboxPoller>();
			system.UseTestDouble<FakeMailboxRepository>().For<IMailboxRepository>();
			system.UseTestDouble<FakeScenarioRepository>().For<IScenarioRepository>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<DefaultScenarioFromRepository>().For<ICurrentScenario>();
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
		public void ShouldReturnTrueWhenHasScheduleChangedMessageWithinPollInterval()
		{
			var scenario = ScenarioFactory.CreateScenario("test", false, false).WithId();
			ScenarioRepository.Add(scenario);
			var me = PersonFactory.CreatePerson().WithId();
			LoggedOnUser.SetFakeLoggedOnUser(me);
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

			FakeMailboxRepository.Add(new Mailbox
			{
				Id = mailboxId,
				Route = scheduleChangeMessage.Routes().First(),
				ExpiresAt = Now.UtcDateTime().Add(TimeSpan.FromSeconds(15 * 60))
			});

			FakeMailboxRepository.AddMessage(scheduleChangeMessage);

			var period = new DateOnlyPeriod(new DateOnly(2017, 11, 17), new DateOnly(2017, 11, 17));
			Target.Check(mailboxId, period.StartDate.Date, period.EndDate.Date).Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldReturnFalseWhenHasScheduleChangedMessageOutsideOfPollInterval()
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

			var period = new DateOnlyPeriod(new DateOnly(2017, 11, 17), new DateOnly(2017, 11, 17));
			Target.Check(mailboxId, period.StartDate.Date, period.EndDate.Date).Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldReturnTrueWhenHasScheduleChangedMessagePeriodIntersectWithPollInterval()
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

			var period = new DateOnlyPeriod(new DateOnly(2017, 11, 17), new DateOnly(2017, 11, 17));
			Target.Check(mailboxId, period.StartDate.Date, period.EndDate.Date).Should().Be.EqualTo(true);
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

			Target.Check(mailboxId, new DateTime(2017, 11, 21), new DateTime(2017, 11, 21)).Should().Be.EqualTo(false);
		}


	}
}
