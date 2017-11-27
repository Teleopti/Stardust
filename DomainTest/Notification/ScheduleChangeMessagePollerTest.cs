//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Teleopti.Ccc.TestCommon.IoC;
//using Teleopti.Ccc.IocCommon;
//using Teleopti.Ccc.Domain.Notification;
//using SharpTestsEx;
//using Teleopti.Ccc.TestCommon.FakeData;
//using Teleopti.Ccc.TestCommon.FakeRepositories;
//using Teleopti.Ccc.Domain.MessageBroker;
//using Teleopti.Ccc.Domain.MessageBroker.Server;
//using Teleopti.Ccc.Domain.Repositories;
//using Teleopti.Ccc.TestCommon;
//using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
//using Teleopti.Ccc.Domain.Common;
//using Teleopti.Ccc.Domain.Common.Time;

//namespace Teleopti.Ccc.DomainTest.Notification
//{
//	[TestFixture, DomainTest]
//	public class ScheduleChangeMessagePollerTest : ISetup
//	{
//		public ScheduleChangeMessagePoller Target;
//		public FakePersonScheduleChangeMessageRepository FakePersonScheduleChangeMessageRepository;
//		public FakeScenarioRepository ScenarioRepository;
//		public DefaultScenarioFromRepository DefaultScenarioFromRepository;
//		public FakeLoggedOnUser LoggedOnUser;
//		public MutableNow Now;

//		public void Setup(ISystem system, IIocConfiguration configuration)
//		{
//			system.AddService<ScheduleChangeMessagePoller>();
//			system.UseTestDouble<FakeScenarioRepository>().For<IScenarioRepository>();
//			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
//			system.UseTestDouble<DefaultScenarioFromRepository>().For<ICurrentScenario>();
//			system.UseTestDouble<MutableNow>().For<INow>();
//			system.UseTestDouble<FakePersonScheduleChangeMessageRepository>().For<IPersonScheduleChangeMessageRepository>();
//		}

//		[Test]
//		public void ShouldReturnUpdatedPeriodsWhenHasScheduleChangedMessageWithinPollInterval()
//		{
//			var scenario = ScenarioFactory.CreateScenario("test", false, false).WithId();
//			ScenarioRepository.Add(scenario);
//			var me = PersonFactory.CreatePerson().WithId();
//			LoggedOnUser.SetFakeLoggedOnUser(me);
//			me.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

//			var scheduleChangeMessage = new PersonScheduleChangeMessage
//			{
//				StartDate = new DateTime(2017, 11, 17, 8, 0, 0),
//				EndDate = new DateTime(2017, 11, 17, 10, 0, 0),
//				PersonId = me.Id.Value,
//				TimeStamp = Now.UtcDateTime()
//			};

//			var scheduleChangeMessage2 = new PersonScheduleChangeMessage
//			{
//				StartDate = new DateTime(2017, 11, 17, 10, 0, 0),
//				EndDate = new DateTime(2017, 11, 17, 11, 0, 0),
//				PersonId = me.Id.Value,
//				TimeStamp = Now.UtcDateTime()
//			};


//			FakePersonScheduleChangeMessageRepository.Add(scheduleChangeMessage);
//			FakePersonScheduleChangeMessageRepository.Add(scheduleChangeMessage2);

//			var period = new PollerInputPeriod(new DateTime(2017, 11, 17), new DateTime(2017, 11, 17));
//			Target.Check(period)[period].Count.Should().Be.EqualTo(2);
//		}

//		[Test]
//		public void ShouldNotReturnUpdatedPeriodWhenHasScheduleChangedMessageOutsideOfPollInterval()
//		{
//			var scenario = ScenarioFactory.CreateScenario("test", false, false).WithId();
//			ScenarioRepository.Add(scenario);
//			var me = PersonFactory.CreatePerson().WithId();
//			LoggedOnUser.SetFakeLoggedOnUser(me);

//			var scheduleChangeMessage = new PersonScheduleChangeMessage
//			{
//				StartDate = new DateTime(2017, 11, 15, 8, 0, 0),
//				EndDate = new DateTime(2017, 11, 15, 10, 0, 0),
//				PersonId = me.Id.Value,
//				TimeStamp = Now.UtcDateTime()
//			};

//			FakePersonScheduleChangeMessageRepository.Add(scheduleChangeMessage);


//			var period = new PollerInputPeriod(new DateTime(2017, 11, 17), new DateTime(2017, 11, 17));
//			Target.Check(period)[period].Should().Be.Empty();
//		}

//		[Test]
//		public void ShouldReturnUpdatedPeriodWhenHasScheduleChangedMessagePeriodIntersectWithPollInterval()
//		{
//			var scenario = ScenarioFactory.CreateScenario("test", false, false).WithId();
//			ScenarioRepository.Add(scenario);
//			var me = PersonFactory.CreatePerson().WithId();
//			LoggedOnUser.SetFakeLoggedOnUser(me);

//			var scheduleChangeMessage = new PersonScheduleChangeMessage
//			{
//				StartDate = new DateTime(2017, 11, 16, 22, 0, 0),
//				EndDate = new DateTime(2017, 11, 17, 5, 0, 0),
//				PersonId = me.Id.Value,
//				TimeStamp = Now.UtcDateTime()
//			};

//			FakePersonScheduleChangeMessageRepository.Add(scheduleChangeMessage);

//			var period = new PollerInputPeriod(new DateTime(2017, 11, 17), new DateTime(2017, 11, 17));
//			Target.Check(period)[period].Count.Should().Be.EqualTo(1);
//		}

//		[Test]
//		public void ShouldPollIntervalBeComparedBasedOnUtcTime()
//		{
//			var scenario = ScenarioFactory.CreateScenario("test", false, false).WithId();
//			ScenarioRepository.Add(scenario);
//			var me = PersonFactory.CreatePerson().WithId();
//			me.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo());
//			LoggedOnUser.SetFakeLoggedOnUser(me);

//			var scheduleChangeMessage = new PersonScheduleChangeMessage
//			{
//				StartDate = new DateTime(2017, 11, 21, 20, 0, 0),
//				EndDate = new DateTime(2017, 11, 21, 22, 0, 0),
//				PersonId = me.Id.Value,
//				TimeStamp = Now.UtcDateTime()
//			};

//			FakePersonScheduleChangeMessageRepository.Add(scheduleChangeMessage);
//			var period = new PollerInputPeriod(new DateTime(2017, 11, 21), new DateTime(2017, 11, 21));
//			Target.Check(period)[period].Should().Be.Empty();
//		}

//		[Test]
//		public void ShouldResetPolling()
//		{
//			var scenario = ScenarioFactory.CreateScenario("test", false, false).WithId();
//			ScenarioRepository.Add(scenario);
//			var me = PersonFactory.CreatePerson().WithId();
//			me.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo());
//			LoggedOnUser.SetFakeLoggedOnUser(me);

//			var scheduleChangeMessage = new PersonScheduleChangeMessage
//			{
//				StartDate = new DateTime(2017, 11, 21, 20, 0, 0),
//				EndDate = new DateTime(2017, 11, 21, 22, 0, 0),
//				PersonId = me.Id.Value,
//				TimeStamp = Now.UtcDateTime()
//			};

//			FakePersonScheduleChangeMessageRepository.Add(scheduleChangeMessage);

//			Target.ResetPolling();

//			FakePersonScheduleChangeMessageRepository.PopMessages(me.Id.Value).Should().Be.Empty();
//		}
//	}

//}
