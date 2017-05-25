using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	[MyTimeWebTest]
	[SetCulture("sv-SE")]
	public class ScheduleApiControllerFetchMonthDataTest
	{
		public ScheduleApiController Target;
		public ICurrentScenario Scenario;
		public ILoggedOnUser User;
		public FakeScheduleDataReadScheduleStorage ScheduleData;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public FakePersonRequestRepository PersonRequestRepository;
		
		[Test]
		public void ShouldHaveTheFirstDayOfTheFirstWeekInMonth()
		{
			Now.Is(new DateTime(2014, 11, 30, 20, 0, 0, DateTimeKind.Utc));
			TimeZone.IsSweden();

			var result = Target.FetchMonthData(null);
			result.CurrentDate.Date.Should().Be.EqualTo(new DateTime(2014, 11, 30));
			result.ScheduleDays.First().Date.Should().Be.EqualTo(new DateTime(2014, 10, 27));
			result.ScheduleDays.First().FixedDate.Should().Be.EqualTo(new DateOnly(2014, 10, 27).ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldHaveCorrectNumberOfDaysAccordingToMonth()
		{
			var result = Target.FetchMonthData(null);
			result.ScheduleDays.Count().Should().Be.EqualTo(35);
		}

		[Test]
		public void ShouldCreateModelForWeekScheduleWithSevenDays()
		{
			var result = Target.FetchWeekData(null);
			result.Days.Count().Should().Be.EqualTo(7);
		}

		[Test]
		public void ShouldMapCurrentDate()
		{
			var result = Target.FetchMonthData(null);
			var localDateOnly = Now.LocalDateOnly();
			result.CurrentDate.Should().Be.EqualTo(localDateOnly.Date);
			result.FixedDate.Should().Be.EqualTo(localDateOnly.ToFixedClientDateOnlyFormat());
		}

		[Test]
		[SetUICulture("de-DE")]
		[SetCulture("en-GB")]
		public void ShouldMapDayHeaderOfWeek()
		{
			var result = Target.FetchMonthData(null);

			result.DayHeaders.First().Name.Should().Be.EqualTo("Montag");
			result.DayHeaders.First().ShortName.Should().Be.EqualTo("Mo");
		}

		[Test]
		public void ShouldMapAbsenceName()
		{
			ScheduleData.Add(new PersonAbsence(User.CurrentUser(), Scenario.Current(),
				new AbsenceLayer(new Absence { Description = new Description("Illness", "IL") }, new DateTimePeriod(2014, 12, 1, 8, 2014, 12, 1, 17))));

			var result = Target.FetchMonthData(null);

			result.ScheduleDays.First().Absence.Name.Should().Be.EqualTo("Illness");
			result.ScheduleDays.First().Absence.ShortName.Should().Be.EqualTo("IL");
		}

		[Test]
		public void ShouldMapAbsenceRespectingPriority()
		{
			ScheduleData.Add(new PersonAbsence(User.CurrentUser(), Scenario.Current(),
							new AbsenceLayer(new Absence { Description = new Description("a", "IL"), Priority = 1 }, new DateTimePeriod(2014, 12, 1, 8, 2014, 12, 1, 17))));

			ScheduleData.Add(new PersonAbsence(User.CurrentUser(), Scenario.Current(),
							new AbsenceLayer(new Absence { Description = new Description("a", "HO"), Priority = 100 }, new DateTimePeriod(2014, 12, 1, 8, 2014, 12, 1, 17))));

			var result = Target.FetchMonthData(null);
			result.ScheduleDays.First().Absence.ShortName.Should().Be.EqualTo("IL");
		}

		[Test]
		public void ShouldMapIsFullDayAbsence()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 01));
			assignment.AddActivity(new Activity("Phone"), new DateTimePeriod(2014, 12, 1, 8, 2014, 12, 1, 17), true);
			ScheduleData.Add(assignment);
			ScheduleData.Add(new PersonAbsence(User.CurrentUser(), Scenario.Current(),
				new AbsenceLayer(new Absence { Description = new Description("Illness", "IL") }, new DateTimePeriod(2014, 12, 1, 8, 2014, 12, 1, 17))));

			var result = Target.FetchMonthData(null);
			result.ScheduleDays.First().Absence.IsFullDayAbsence.Should().Be.True();
		}

		[Test]
		public void ShouldMapIsDayOff()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 01));
			assignment.SetDayOff(new DayOffTemplate(new Description("Day off", "DO")), true);
			ScheduleData.Add(assignment);

			var result = Target.FetchMonthData(null);
			result.ScheduleDays.First().IsDayOff.Should().Be.True();
		}

		[Test]
		public void ShouldMapIsNotDayOffForWorkingDay()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 01));
			assignment.AddActivity(new Activity("Phone"), new DateTimePeriod(2014, 12, 1, 8, 2014, 12, 1, 17), true);
			ScheduleData.Add(assignment);

			var result = Target.FetchMonthData(null);
			result.ScheduleDays.First().IsDayOff.Should().Be.False();
		}

		[Test]
		public void ShouldMapIsDayOffForContractDayOff()
		{
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2001, 1, 1));
			var contractScheduleWeek = new ContractScheduleWeek();
			contractScheduleWeek.Add(DayOfWeek.Saturday, false);
			personPeriod.PersonContract.ContractSchedule.AddContractScheduleWeek(contractScheduleWeek);
			User.CurrentUser().AddPersonPeriod(personPeriod);

			ScheduleData.Add(new PersonAbsence(User.CurrentUser(), Scenario.Current(),
				new AbsenceLayer(new Absence { Description = new Description("Illness", "IL") }, new DateTimePeriod(2014, 12, 6, 8, 2014, 12, 6, 17))));

			var result = Target.FetchMonthData(null);
			result.ScheduleDays.ElementAt(5).IsDayOff.Should().Be.True();
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldMapTimeSpanForWorkingDay()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 01));
			assignment.AddActivity(new Activity("Phone") { InWorkTime = true }, new DateTimePeriod(2014, 12, 1, 7, 2014, 12, 1, 16), true);
			assignment.SetShiftCategory(new ShiftCategory("Late") { Description = new Description("Late", "PM"), DisplayColor = Color.Green });
			ScheduleData.Add(assignment);

			var result = Target.FetchMonthData(null).ScheduleDays.ElementAt(1);

			result.Shift.Should().Not.Be.Null();
			result.Shift.TimeSpan.Should().Be.EqualTo(assignment.Period.TimePeriod(TimeZone.TimeZone()).ToShortTimeString());
			result.Shift.Color.Should().Be.EqualTo("rgb(0,128,0)");
			result.Shift.Name.Should().Be.EqualTo("Late");
			result.Shift.ShortName.Should().Be.EqualTo("PM");
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldMapTimeSpanForWorkingDayExcludingPersonalActivity()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 01));
			var period = new DateTimePeriod(2014, 12, 1, 7, 2014, 12, 1, 16);
			assignment.AddActivity(new Activity("a") { InWorkTime = true }, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			assignment.AddPersonalActivity(new Activity("b") { InWorkTime = true }, period.MovePeriod(TimeSpan.FromHours(-2)));
			ScheduleData.Add(assignment);

			var result = Target.FetchMonthData(null).ScheduleDays.ElementAt(1);
			result.Shift.Should().Not.Be.Null();
			result.Shift.TimeSpan.Should().Be.EqualTo(period.TimePeriod(TimeZone.TimeZone()).ToShortTimeString());
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldMapWorkingHoursForWorkingDay()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 01));
			var period = new DateTimePeriod(2014, 12, 1, 7, 2014, 12, 1, 16);
			assignment.AddActivity(new Activity("a") { InWorkTime = true, InContractTime = true }, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var result = Target.FetchMonthData(null).ScheduleDays.ElementAt(1);
			result.Shift.WorkingHours.Should().Be(TimeHelper.GetLongHourMinuteTimeString(TimeSpan.FromHours(9), CultureInfo.CurrentUICulture));
		}
		
	}
}
