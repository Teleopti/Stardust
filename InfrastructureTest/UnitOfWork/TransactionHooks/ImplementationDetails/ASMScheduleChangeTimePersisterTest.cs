using NUnit.Framework;
using System;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

using Teleopti.Ccc.IocCommon;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using System.Collections.Generic;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.TransactionHooks.ImplementationDetails
{
	[TestFixture, DatabaseTest]
	public class ASMScheduleChangeTimePersisterTest : IIsolateSystem
	{
		public ASMScheduleChangeTimePersister Target;
		public FakeASMScheduleChangeTimeRepository Repo;
		public MutableNow Now;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ASMScheduleChangeTimePersister>().For<ITransactionHook>();
			isolate.UseTestDouble<FakeASMScheduleChangeTimeRepository>().For<IASMScheduleChangeTimeRepository>();
			isolate.UseTestDouble<MutableNow>().For<INow>();
		}

		[Test]
		public void ShouldAddScheduleChangeTimeForPersonAssignment()
		{
			var person = PersonFactory.CreatePerson().WithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				ScenarioFactory.CreateScenarioWithId("default", true),
				new DateTimePeriod(new DateTime(2017, 11, 24, 6, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 11, 24, 15, 0, 0, DateTimeKind.Utc)));
			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(pa, DomainUpdateType.Update)
			};
			var now = new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc);
			Now.Is(now);
			Target.AfterCompletion(roots);

			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).TimeStamp.Should().Be(now);
		}

		[Test]
		public void ShouldNotAddScheduleChangeTimeForPersonAssignmentIfNotInDefaultScenario()
		{
			var person = PersonFactory.CreatePerson().WithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				ScenarioFactory.CreateScenarioWithId("not default", false),
				new DateTimePeriod(new DateTime(2017, 11, 24, 6, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 11, 24, 15, 0, 0, DateTimeKind.Utc)));
			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(pa, DomainUpdateType.Update)
			};
			Now.Is(new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc));
			Target.AfterCompletion(roots);
			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).Should().Be.Null();
		}

		[Test]
		public void ShouldAddScheduleChangeTimeIfNextDayInNotifyPeriod()
		{
			var person = PersonFactory.CreatePerson().WithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				ScenarioFactory.CreateScenarioWithId("default", true),
				new DateTimePeriod(new DateTime(2017, 11, 25, 6, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 11, 25, 15, 0, 0, DateTimeKind.Utc)));
			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(pa, DomainUpdateType.Update)
			};
			var now = new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc);
			Now.Is(now);
			Target.AfterCompletion(roots);

			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).TimeStamp.Should().Be(now);
		}

		[Test]
		public void ShouldAddScheduleChangeTimeIfTheDayAfterNextDayInNotifyPeriod()
		{
			var person = PersonFactory.CreatePerson().WithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				ScenarioFactory.CreateScenarioWithId("default", true),
				new DateTimePeriod(new DateTime(2017, 12, 8, 13, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 12, 8, 15, 0, 0, DateTimeKind.Utc)));

			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(pa, DomainUpdateType.Update)
			};
			var now = new DateTime(2017, 12, 6, 13, 0, 0, DateTimeKind.Utc);
			Now.Is(now);
			Target.AfterCompletion(roots);

			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).TimeStamp.Should().Be(now);
		}

		[Test]
		public void ShouldNotAddScheduleChangeTimeIfTheDayAfterNextDayNotInNotifyPeriod()
		{
			var person = PersonFactory.CreatePerson().WithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				ScenarioFactory.CreateScenarioWithId("default", true),
				new DateTimePeriod(new DateTime(2017, 12, 8, 8, 10, 0, DateTimeKind.Utc),
				new DateTime(2017, 12, 8, 20, 0, 0, DateTimeKind.Utc)));

			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(pa, DomainUpdateType.Update)
			};
			var now = new DateTime(2017, 12, 6, 0, 0, 0, DateTimeKind.Utc);
			Now.Is(now);
			Target.AfterCompletion(roots);

			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).Should().Be.Null();
		}

		[Test]
		public void ShouldNotAddScheduleChangeTimeIfYesterdaysScheduleChangedAndNowIsAfterOneOclockUnderAgentsTimezone()
		{
			var person = PersonFactory.CreatePerson().WithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				ScenarioFactory.CreateScenarioWithId("default", true),
				new DateTimePeriod(new DateTime(2017, 12, 6, 23, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 12, 7, 23, 0, 0, DateTimeKind.Utc)));

			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(pa, DomainUpdateType.Insert)
			};
			var now = new DateTime(2017, 12, 8, 2, 0, 0, DateTimeKind.Utc);
			Now.Is(now);
			Target.AfterCompletion(roots);

			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).Should().Be.Null();
		}

		[Test]
		public void ShouldNotAddScheduleChangeTimeIfThePreviousDayNotInNotifyPeriod()
		{
			var person = PersonFactory.CreatePerson().WithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				ScenarioFactory.CreateScenarioWithId("default", true),
				new DateTimePeriod(new DateTime(2017, 12, 5, 23, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 12, 5, 23, 30, 0, DateTimeKind.Utc)));

			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(pa, DomainUpdateType.Update)
			};
			var now = new DateTime(2017, 12, 6, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(now);
			Target.AfterCompletion(roots);

			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).Should().Be.Null();
		}

		[Test]
		public void ShouldAddScheduleChangeTimeIfThePreviousDayInNotifyPeriod()
		{
			var person = PersonFactory.CreatePerson().WithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				ScenarioFactory.CreateScenarioWithId("default", true),
				new DateTimePeriod(new DateTime(2017, 12, 5, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 12, 5, 1, 0, 0, DateTimeKind.Utc)));

			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(pa, DomainUpdateType.Update)
			};
			var now = new DateTime(2017, 12, 6, 0, 0, 0, DateTimeKind.Utc);
			Now.Is(now);
			Target.AfterCompletion(roots);

			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).TimeStamp.Should().Be(now);
		}

		[Test]
		public void ShouldNotAddScheduleChangeTimeForPersonAssignmentIfNotInASMNotifyPeriod()
		{
			var person = PersonFactory.CreatePerson().WithId();

			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo());
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				ScenarioFactory.CreateScenarioWithId("default", true),
				new DateTimePeriod(new DateTime(2017, 11, 25, 10, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 11, 25, 15, 0, 0, DateTimeKind.Utc)));

			var roots = new IRootChangeInfo[]
			{
								new RootChangeInfo(pa, DomainUpdateType.Update)
			};
			Now.Is(new DateTime(2017, 11, 26, 20, 0, 0, DateTimeKind.Utc));
			Target.AfterCompletion(roots);
			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).Should().Be.Null();
		}

		[Test]
		public void ShouldUpdateScheduleChangeTimeForPersonAssignment()
		{
			var person = PersonFactory.CreatePerson().WithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				ScenarioFactory.CreateScenarioWithId("default", true),
				new DateTimePeriod(new DateTime(2017, 11, 24, 6, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 11, 24, 15, 0, 0, DateTimeKind.Utc)));
			var roots = new IRootChangeInfo[]
			{
								new RootChangeInfo(pa, DomainUpdateType.Update)
			};
			Now.Is(new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc));

			Target.AfterCompletion(roots);

			var newTime = new DateTime(2017, 11, 24, 15, 0, 0, DateTimeKind.Utc);
			Now.Is(newTime);

			Target.AfterCompletion(roots);

			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).TimeStamp.Should().Be(newTime);
		}

		[Test]
		public void ShouldAddScheduleChangeTimeForPersonAssignmentInUserTimeZone()
		{
			var person = PersonFactory.CreatePerson().WithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo());

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				ScenarioFactory.CreateScenarioWithId("default", true),
				new DateTimePeriod(new DateTime(2017, 12, 5, 23, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 12, 5, 23, 30, 0, DateTimeKind.Utc)));

			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(pa, DomainUpdateType.Update)
			};
			var now = new DateTime(2017, 12, 6, 0, 0, 0, DateTimeKind.Utc);
			Now.Is(now);
			Target.AfterCompletion(roots);

			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).TimeStamp.Should().Be(now);
		}

		[Test]
		public void ShouldAddScheduleChangeTimeForPersonAbsence()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var period = new DateTimePeriod(new DateTime(2017, 11, 24, 6, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 11, 24, 15, 0, 0, DateTimeKind.Utc));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, period);

			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(personAbsence, DomainUpdateType.Update)
			};
			var now = new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc);
			Now.Is(now);

			Target.AfterCompletion(roots);
			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).TimeStamp.Should().Be(now);
		}

		[Test]
		public void ShouldNotAddScheduleChangeTimeForPersonAbsenceIfNotInDefaultScenario()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var scenario = ScenarioFactory.CreateScenarioWithId("default", false);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var period = new DateTimePeriod(new DateTime(2017, 11, 24, 6, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 11, 24, 15, 0, 0, DateTimeKind.Utc));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, period);

			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(personAbsence, DomainUpdateType.Update)
			};
			var now = new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc);
			Now.Is(now);

			Target.AfterCompletion(roots);
			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).Should().Be.Null();
		}

		[Test]
		public void ShouldAddScheduleChangeTimeForMultipleDaysPersonAbsenceIfInNotifyPeriod()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var period = new DateTimePeriod(new DateTime(2017, 12, 8, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 12, 9, 18, 0, 0, DateTimeKind.Utc));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, period);

			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(personAbsence, DomainUpdateType.Update)
			};
			var time = new DateTime(2017, 12, 10, 0, 0, 0, DateTimeKind.Utc);
			Now.Is(time);

			Target.AfterCompletion(roots);

			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).TimeStamp.Should().Be(time);
		}

		[Test]
		public void ShouldNotAddScheduleChangeTimeForMultipleDaysPersonAbsenceIfNotInNotifyPeriod()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var period = new DateTimePeriod(new DateTime(2017, 12, 8, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 12, 9, 18, 0, 0, DateTimeKind.Utc));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, period);

			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(personAbsence, DomainUpdateType.Update)
			};
			var time = new DateTime(2017, 12, 10, 13, 0, 0, DateTimeKind.Utc);
			Now.Is(time);

			Target.AfterCompletion(roots);

			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).Should().Be.Null();
		}

		[Test]
		public void ShouldNotAddScheduleChangeTimeIfTheTypeIsNotAbsenceOrAssignmentOrMeeting()
		{
			var person = PersonFactory.CreatePerson().WithId();

			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var roots = new IRootChangeInfo[]
			{
						new RootChangeInfo(person, DomainUpdateType.Update)
			};
			Now.Is(new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc));
			Target.AfterCompletion(roots);
			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).Should().Be.Null();
		}

		[Test]
		public void ShouldUpdateScheduleChangeTimeForPersonAbsence()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var period = new DateTimePeriod(new DateTime(2017, 11, 24, 6, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 11, 24, 15, 0, 0, DateTimeKind.Utc));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, period);

			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(personAbsence, DomainUpdateType.Update)
			};
			Now.Is(new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc));

			Target.AfterCompletion(roots);

			var newTime = new DateTime(2017, 11, 24, 15, 0, 0, DateTimeKind.Utc);
			Now.Is(newTime);

			Target.AfterCompletion(roots);

			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).TimeStamp.Should().Be(newTime);
		}

		[Test]
		public void ShouldAddScheduleChangeTimeForMeetingInDefaultScenario()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var requiredPerson = PersonFactory.CreatePerson().WithId();
			requiredPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			var activity = ActivityFactory.CreateActivity("Meeting");
			Meeting meeting = new Meeting(person,
								  new List<IMeetingPerson>
									  {
										   new MeetingPerson(requiredPerson, false),
									  }, "my subject", "my location",
								  "my description", activity, scenario);

			meeting.StartDate = new DateOnly(2017, 12, 1);
			meeting.EndDate = new DateOnly(2017, 12, 1);
			meeting.StartTime = TimeSpan.FromHours(8);
			meeting.EndTime = TimeSpan.FromHours(10);
			meeting.TimeZone = TimeZoneInfo.Utc;

			var now = new DateTime(2017, 12, 1, 13, 0, 0, DateTimeKind.Utc);
			Now.Is(now);
			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(meeting, DomainUpdateType.Update)
			};
			Target.AfterCompletion(roots);
			Repo.GetScheduleChangeTime(requiredPerson.Id.GetValueOrDefault()).TimeStamp.Should().Be.EqualTo(now);
		}

		[Test]
		public void ShouldNotAddScheduleChangeTimeForPersonMeetingIfNotInDefaultScenario()
		{
			var meetingCreator = PersonFactory.CreatePerson().WithId();
			var attendee = PersonFactory.CreatePerson().WithId();
			attendee.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", false);
			var activity = ActivityFactory.CreateActivity("Meeting");
			Meeting meeting = new Meeting(meetingCreator,
								  new List<IMeetingPerson>
									  {
										   new MeetingPerson(attendee, false),
									  }, "my subject", "my location",
								  "my description", activity, scenario);

			meeting.StartDate = new DateOnly(2017, 12, 1);
			meeting.EndDate = new DateOnly(2017, 12, 1);
			meeting.StartTime = TimeSpan.FromHours(8);
			meeting.EndTime = TimeSpan.FromHours(10);
			meeting.TimeZone = TimeZoneInfo.Utc;

			var now = new DateTime(2017, 12, 1, 13, 0, 0, DateTimeKind.Utc);
			Now.Is(now);
			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(meeting, DomainUpdateType.Update)
			};
			Target.AfterCompletion(roots);
			Repo.GetScheduleChangeTime(attendee.Id.GetValueOrDefault()).Should().Be.Null();
		}

		[Test]
		public void ShouldAddScheduleChangeTimeForRecurrentMeetingIfAnyMeetingDaysInNotifyPeriod()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var requiredPerson = PersonFactory.CreatePerson().WithId();
			requiredPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			var activity = ActivityFactory.CreateActivity("Meeting");
			Meeting meeting = new Meeting(person,
								  new List<IMeetingPerson>
									  {
										   new MeetingPerson(requiredPerson, false),
									  }, "my subject", "my location",
								  "my description", activity, scenario);
			meeting.SetRecurrentOption(new RecurrentDailyMeeting { IncrementCount = 1 });
			meeting.StartDate = new DateOnly(2017, 12, 1);
			meeting.EndDate = new DateOnly(2017, 12, 6);
			meeting.StartTime = TimeSpan.FromHours(8);
			meeting.EndTime = TimeSpan.FromHours(10);
			meeting.TimeZone = TimeZoneInfo.Utc;

			var now = new DateTime(2017, 12, 1, 13, 0, 0, DateTimeKind.Utc);
			Now.Is(now);
			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(meeting, DomainUpdateType.Update)
			};
			Target.AfterCompletion(roots);
			Repo.GetScheduleChangeTime(requiredPerson.Id.GetValueOrDefault()).TimeStamp.Should().Be.EqualTo(now);
		}

		[Test]
		public void ShouldNotAddScheduleChangeTimeForRecurrentMeetingIfNoMeetingDaysInNotifyPeriod()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var requiredPerson = PersonFactory.CreatePerson().WithId();
			requiredPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			var activity = ActivityFactory.CreateActivity("Meeting");
			Meeting meeting = new Meeting(person,
								  new List<IMeetingPerson>
									  {
										   new MeetingPerson(requiredPerson, false),
									  }, "my subject", "my location",
								  "my description", activity, scenario);
			meeting.SetRecurrentOption(new RecurrentDailyMeeting { IncrementCount = 1 });
			meeting.StartDate = new DateOnly(2017, 11, 29);
			meeting.EndDate = new DateOnly(2017, 11, 30);
			meeting.StartTime = TimeSpan.FromHours(6);
			meeting.EndTime = TimeSpan.FromHours(7);
			meeting.TimeZone = TimeZoneInfoFactory.ChinaTimeZoneInfo();

			var now = new DateTime(2017, 12, 1, 13, 0, 0, DateTimeKind.Utc);
			Now.Is(now);
			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(meeting, DomainUpdateType.Update)
			};
			Target.AfterCompletion(roots);
			Repo.GetScheduleChangeTime(requiredPerson.Id.GetValueOrDefault()).Should().Be.Null();
		}

		[Test]
		public void ShouldNotUpdateScheduleChangeTimeIfTheTypeIsNotAbsenceOrAssignmentOrPersonMeeting()
		{
			var person = PersonFactory.CreatePerson().WithId();

			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(person, DomainUpdateType.Update)
			};
			Now.Is(new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc));
			Target.AfterCompletion(roots);

			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).Should().Be.Null();
		}

	}

}