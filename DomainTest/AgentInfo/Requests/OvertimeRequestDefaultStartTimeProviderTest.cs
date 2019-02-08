using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
	[DomainTest]
	public class OvertimeRequestDefaultStartTimeProviderTest : IIsolateSystem
	{
		public IOvertimeRequestDefaultStartTimeProvider OvertimeRequestDefaultStartTimeProvider;
		public ICurrentScenario CurrentScenario;
		public FakePersonAssignmentRepository FakeAssignmentRepository;
		public FakeActivityRepository FakeActivityRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakeLoggedOnUser FakeLoggedOnUser;
		public MutableNow Now;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble(new MutableNow(new DateTime(2018, 01, 08, 10, 00, 00, DateTimeKind.Utc))).For<INow>();
		}

		[Test]
		public void ShouldGetDefaultStartTime()
		{
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.DefaultStartTime.Date.Should().Be(date.Date);
			result.DefaultStartTime.Hour.Should().Be(17);
			result.DefaultStartTime.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldSetIsShiftEndTimeTrueWhenGettingTodayNormalShiftEndTimeAsDefaultStartTime()
		{
			Now.Is(new DateTime(2018, 1, 8, 08, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.DefaultStartTime.Date.Should().Be(date.Date);
			result.DefaultStartTime.Hour.Should().Be(17);
			result.DefaultStartTime.Minute.Should().Be(0);
			result.IsShiftStartTime.Should().Be(false);
			result.IsShiftEndTime.Should().Be(true);
		}

		[Test]
		public void ShouldSetIsShiftStartTimeTrueWhenGettingTodayOvernightShiftStartTimeAsDefaultStartTime()
		{
			Now.Is(new DateTime(2018, 1, 8, 08, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 17, 2018, 1, 9, 03));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.DefaultStartTime.Date.Should().Be(date.Date);
			result.DefaultStartTime.Hour.Should().Be(17);
			result.DefaultStartTime.Minute.Should().Be(0);
			result.IsShiftStartTime.Should().Be(true);
			result.IsShiftEndTime.Should().Be(false);
		}

		[Test]
		public void ShouldGetRoundUpHourAsDefaultStartTimeWhenCurrentTimeIsLaterThanShiftEndTime()
		{
			Now.Is(new DateTime(2018, 1, 8, 18, 09, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.DefaultStartTime.Date.Should().Be(date.Date);
			result.DefaultStartTime.Hour.Should().Be(18);
			result.DefaultStartTime.Minute.Should().Be(30);
		}

		[Test]
		public void ShouldGetRoundUpHourOnTomorrowAsDefaultStartTimeWhenCurrentTimeIsLaterThanShiftEndTime()
		{
			Now.Is(new DateTime(2018, 1, 8, 23, 49, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.DefaultStartTime.Date.Should().Be(date.Date.AddDays(1));
			result.DefaultStartTime.Hour.Should().Be(0);
			result.DefaultStartTime.Minute.Should().Be(30);
		}

		[Test]
		public void ShouldGetYesterdayOvernightShiftEndTimeAsDefaultStartTimeWhenCurrentTimeIsEarlierThanThatTime()
		{
			Now.Is(new DateTime(2018, 1, 8, 02, 09, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date.AddDays(-1));
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 7, 18, 2018, 1, 8, 03));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.DefaultStartTime.Date.Should().Be(date.Date);
			result.DefaultStartTime.Hour.Should().Be(3);
			result.DefaultStartTime.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetTodayNormalShiftEndTimeAsDefaultStartTimeWhenYesterdayShiftEndsAt12AmToday()
		{
			Now.Is(new DateTime(2018, 1, 8, 02, 09, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 9);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 9, 18, 2018, 1, 10, 00));
			FakeAssignmentRepository.Has(personAssignment);

			var personAssignment2 = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date.AddDays(1));
			personAssignment2.AddActivity(phone, new DateTimePeriod(2018, 1, 10, 11, 2018, 1, 10, 20));
			FakeAssignmentRepository.Has(personAssignment2);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date.AddDays(1));

			result.DefaultStartTime.Date.Should().Be(date.AddDays(1).Date);
			result.DefaultStartTime.Hour.Should().Be(20);
			result.DefaultStartTime.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetYesterdayOvernightShiftEndTimeWhenCurrentTimeIsEarlierThanThatTimeThoughThereIsOvernightShfitEndsAtTomorrow()
		{
			Now.Is(new DateTime(2018, 1, 8, 02, 09, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date.AddDays(-1));
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 7, 18, 2018, 1, 8, 03));

			var personAssignment2 = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment2.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 18, 2018, 1, 9, 03));
			FakeAssignmentRepository.Has(personAssignment);
			FakeAssignmentRepository.Has(personAssignment2);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.DefaultStartTime.Date.Should().Be(date.Date);
			result.DefaultStartTime.Hour.Should().Be(3);
			result.DefaultStartTime.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetTodayOvernightShiftEndTimeAsDefaultStartTimeWhenCurrentTimeIsWithinTheShift()
		{
			Now.Is(new DateTime(2018, 1, 8, 18, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 17, 2018, 1, 9, 03));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.DefaultStartTime.Date.Should().Be(date.AddDays(1).Date);
			result.DefaultStartTime.Hour.Should().Be(3);
			result.DefaultStartTime.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetTodayOvernightShiftStartTimeAsDefaultStartTimeWhenCurrentTimeIsBeforeTheShiftStarts()
		{
			Now.Is(new DateTime(2018, 1, 8, 8, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 17, 2018, 1, 9, 03));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.DefaultStartTime.Date.Should().Be(date.Date);
			result.DefaultStartTime.Hour.Should().Be(17);
			result.DefaultStartTime.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetTodayOvernightShiftStartTimeAsDefaultStartTimeWhenShiftEndsAt12AmTomorrowConsideringTimezone()
		{
			Now.Is(new DateTime(2018, 1, 8, 02, 09, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 9);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));

			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 9, 18, 2018, 1, 9, 23));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.DefaultStartTime.Date.Should().Be(date.Date);
			result.DefaultStartTime.Hour.Should().Be(19);
			result.DefaultStartTime.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetShiftEndTimeAsDefaultStartTimeWhenTheGapBetweenCurrentTimeAndShiftStartTimeIsLessThan20Min()
		{
			Now.Is(new DateTime(2018, 1, 8, 07, 45, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.DefaultStartTime.Date.Should().Be(date.Date);
			result.DefaultStartTime.Hour.Should().Be(17);
			result.DefaultStartTime.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldStillGetShiftEndTimeAsDefaultStartTimeWhenShiftEndsTodayThoughTheGapBetweenCurrentTimeAndShiftStartTimeIsMoreThan20Min()
		{
			Now.Is(new DateTime(2018, 1, 8, 07, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.DefaultStartTime.Date.Should().Be(date.Date);
			result.DefaultStartTime.Hour.Should().Be(17);
			result.DefaultStartTime.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetSelectedDayOvernightShiftStartTimeAsDefaultStartTime()
		{
			Now.Is(new DateTime(2018, 1, 8, 18, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 9);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 9, 17, 2018, 1, 10, 03));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.DefaultStartTime.Date.Should().Be(date.Date);
			result.DefaultStartTime.Hour.Should().Be(17);
			result.DefaultStartTime.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetRoundUpHalfHourAsDefaultStartTimeWhenCurrentTimeIsEarlierThanShiftEndTimePlus20Min()
		{
			Now.Is(new DateTime(2018, 1, 8, 17, 49, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.DefaultStartTime.Date.Should().Be(date.Date);
			result.DefaultStartTime.Hour.Should().Be(18);
			result.DefaultStartTime.Minute.Should().Be(30);
		}

		[Test]
		public void ShouldGetCorrectDefaultStartTimeConsideringShiftEndTimeAndCurrentTimeAndThe20MinGap()
		{
			Now.Is(new DateTime(2018, 1, 8, 16, 49, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);

			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.DefaultStartTime.Date.Should().Be(date.Date);
			result.DefaultStartTime.Hour.Should().Be(17);
			result.DefaultStartTime.Minute.Should().Be(30);
		}

		[Test]
		public void ShouldGetCorrectDefaultStartTimeConsideringOvernightShiftEndTimeAndCurrentTimeAndThe20MinGap()
		{
			Now.Is(new DateTime(2018, 1, 8, 6, 49, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);

			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date.AddDays(-1));
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 7, 18, 2018, 1, 8, 07));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.DefaultStartTime.Date.Should().Be(date.Date);
			result.DefaultStartTime.Hour.Should().Be(7);
			result.DefaultStartTime.Minute.Should().Be(30);
		}

		[Test]
		public void ShouldGetCorrectDefaultStartTimeConsideringWorkDefaultStartTimeAndCurrentTimeAndThe20MinGap()
		{
			Now.Is(new DateTime(2018, 1, 8, 07, 49, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.DefaultStartTime.Date.Should().Be(date.Date);
			result.DefaultStartTime.Hour.Should().Be(8);
			result.DefaultStartTime.Minute.Should().Be(30);
		}

		[Test]
		public void ShouldGetWorkDefaultStartTimeAsDefaultStartTimeWhenThereIsNoOvernightShiftOrNormalShiftOnSelectedDay()
		{
			Now.Is(new DateTime(2018, 1, 8, 10, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 8);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);

			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 8, 08, 2018, 1, 8, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date.AddDays(1));

			result.DefaultStartTime.Date.Should().Be(date.AddDays(1).Date);
			result.DefaultStartTime.Hour.Should().Be(8);
			result.DefaultStartTime.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetWorkDefaultStartTimeAsDefaultStartTimeWhenThereIsNoShiftOnSelectedDayAndTheDayBefore()
		{
			Now.Is(new DateTime(2018, 1, 8, 10, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 9);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date.AddDays(1));

			result.DefaultStartTime.Date.Should().Be(date.AddDays(1).Date);
			result.DefaultStartTime.Hour.Should().Be(8);
			result.DefaultStartTime.Minute.Should().Be(0);
		}

		[Test]
		public void ShouldGetDefaultStartTimeInUserTimezone()
		{
			Now.Is(new DateTime(2018, 1, 8, 10, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 10);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));

			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 10, 08, 2018, 1, 10, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.DefaultStartTime.Date.Should().Be(date.Date);
			result.DefaultStartTime.Hour.Should().Be(18);
			result.DefaultStartTime.Minute.Should().Be(0);
		}
	}
}
