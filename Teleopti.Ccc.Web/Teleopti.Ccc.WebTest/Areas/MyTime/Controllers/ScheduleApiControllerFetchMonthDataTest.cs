using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.WebTest.Core.IoC;



namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	[MyTimeWebTest]
	[SetCulture("sv-SE")]
	public class ScheduleApiControllerFetchMonthDataTest : IIsolateSystem
	{
		public ScheduleApiController Target;
		public ICurrentScenario Scenario;
		public ILoggedOnUser User;
		public IScheduleStorage ScheduleData;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakeSeatBookingRepository SeatBookingRepository;
		public FakeSeatMapRepository SeatMapRepository;
		public FakePushMessageDialogueRepository PushMessageDialogueRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeSeatMapRepository>().For<ISeatMapLocationRepository>();
			isolate.UseTestDouble<FakePushMessageDialogueRepository>().For<IPushMessageDialogueRepository>();
		}

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
			var localDateOnly = Now.ServerDate_DontUse();
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
				new AbsenceLayer(new Absence {Description = new Description("Illness", "IL")},
					new DateTimePeriod(2014, 12, 1, 8, 2014, 12, 1, 17))));

			var result = Target.FetchMonthData(null);

			result.ScheduleDays.First().Absences[0].Name.Should().Be.EqualTo("Illness");
			result.ScheduleDays.First().Absences[0].ShortName.Should().Be.EqualTo("IL");
		}


		[Test]
		public void ShouldMapAbsenceColor()
		{
			ScheduleData.Add(new PersonAbsence(User.CurrentUser(), Scenario.Current(),
				new AbsenceLayer(new Absence {DisplayColor = Color.Red, Description = new Description("Illness", "IL")},
					new DateTimePeriod(2014, 12, 1, 8, 2014, 12, 1, 17))));

			var result = Target.FetchMonthData(null);

			result.ScheduleDays.First().Absences[0].Color.Should().Be.EqualTo($"rgb({Color.Red.R},{Color.Red.G},{Color.Red.B})");
		}

		[Test]
		public void ShouldMapAbsencesRespectingPriority()
		{
			ScheduleData.Add(new PersonAbsence(User.CurrentUser(), Scenario.Current(),
				new AbsenceLayer(new Absence {Description = new Description("a", "IL"), Priority = 1},
					new DateTimePeriod(2014, 12, 1, 8, 2014, 12, 1, 17))));

			ScheduleData.Add(new PersonAbsence(User.CurrentUser(), Scenario.Current(),
				new AbsenceLayer(new Absence {Description = new Description("a", "HO"), Priority = 100},
					new DateTimePeriod(2014, 12, 1, 8, 2014, 12, 1, 17))));

			var result = Target.FetchMonthData(null);
			result.ScheduleDays.First().Absences[0].ShortName.Should().Be.EqualTo("IL");
			result.ScheduleDays.First().Absences[1].ShortName.Should().Be.EqualTo("HO");
		}

		[Test]
		public void ShouldMapIsFullDayAbsence()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 01));
			assignment.AddActivity(new Activity("Phone"), new DateTimePeriod(2014, 12, 1, 8, 2014, 12, 1, 17), true);
			ScheduleData.Add(assignment);
			ScheduleData.Add(new PersonAbsence(User.CurrentUser(), Scenario.Current(),
				new AbsenceLayer(new Absence {Description = new Description("Illness", "IL")},
					new DateTimePeriod(2014, 12, 1, 8, 2014, 12, 1, 17))));

			var result = Target.FetchMonthData(null);
			result.ScheduleDays.First().Absences[0].IsFullDayAbsence.Should().Be.True();
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
				new AbsenceLayer(new Absence {Description = new Description("Illness", "IL")},
					new DateTimePeriod(2014, 12, 6, 8, 2014, 12, 6, 17))));

			var result = Target.FetchMonthData(null);
			result.ScheduleDays.ElementAt(5).IsDayOff.Should().Be.True();
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldMapTimeSpanForWorkingDay()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 01));
			assignment.AddActivity(new Activity("Phone") {InWorkTime = true},
				new DateTimePeriod(2014, 12, 1, 7, 2014, 12, 1, 16), true);
			assignment.SetShiftCategory(new ShiftCategory("Late")
			{
				Description = new Description("Late", "PM"),
				DisplayColor = Color.Green
			});
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
			assignment.AddActivity(new Activity("a") {InWorkTime = true}, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			assignment.AddPersonalActivity(new Activity("b") {InWorkTime = true}, period.MovePeriod(TimeSpan.FromHours(-2)));
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
			assignment.AddActivity(new Activity("a") {InWorkTime = true, InContractTime = true}, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var result = Target.FetchMonthData(null).ScheduleDays.ElementAt(1);
			result.Shift.WorkingHours.Should()
				.Be(TimeHelper.GetLongHourMinuteTimeString(TimeSpan.FromHours(9), CultureInfo.CurrentUICulture));
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldNotLoadSeatBookingForMobileMonthView()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 1));
			var period = new DateTimePeriod(2014, 12, 1, 7, 2014, 12, 1, 16);
			assignment.AddActivity(new Activity("a") {InWorkTime = true, InContractTime = true}, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var seatMapLocation = new SeatMapLocation().WithId();
			SeatMapRepository.Add(seatMapLocation);
			var seat = seatMapLocation.AddSeat("s1", 1);
			SeatBookingRepository.Add(new SeatBooking(User.CurrentUser(), new DateOnly(2014, 12, 1),
				new DateTime(2014, 12, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2014, 12, 1, 9, 0, 0, DateTimeKind.Utc))
			{
				Seat = seat
			});

			var result = Target.FetchMobileMonthData(null).ScheduleDays.ElementAt(1);
			result.SeatBookings.Should().Be.Null();
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldMapOvertimesForMobileMonthView()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 1));
			var period = new DateTimePeriod(2014, 12, 1, 7, 2014, 12, 1, 16);
			assignment.AddActivity(new Activity("a") {InWorkTime = true, InContractTime = true}, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			assignment.AddOvertimeActivity(new Activity("ot") {DisplayColor = Color.Purple}
				, new DateTimePeriod(2014, 12, 1, 16, 2014, 12, 1, 18),
				new MultiplicatorDefinitionSet("overtime time", MultiplicatorType.Overtime));
			ScheduleData.Add(assignment);

			var result = Target.FetchMobileMonthData(null).ScheduleDays.ElementAt(1);
			result.Overtimes.Length.Should().Be(1);
			result.Overtimes.FirstOrDefault().Name.Should().Be("ot");
			result.Overtimes.FirstOrDefault().Color.Should().Be($"rgb({Color.Purple.R},{Color.Purple.G},{Color.Purple.B})");
		}

		[Test]
		public void ShouldMapDayOffNameAndShortNameForMobileMonthView()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 01));
			assignment.SetDayOff(new DayOffTemplate(new Description("Day off", "DO")), true);
			ScheduleData.Add(assignment);

			var result = Target.FetchMobileMonthData(null);
			result.ScheduleDays.First().IsDayOff.Should().Be.True();
			result.ScheduleDays.First().Shift.Name.Should().Be("Day off");
			result.ScheduleDays.First().Shift.ShortName.Should().Be("DO");
		}

		[Test]
		public void ShouldMapUnReadMessageCountForMobileMonthView()
		{
			PushMessageDialogueRepository.Add(new PushMessageDialogue(new PushMessage(), User.CurrentUser()));

			var result = Target.FetchMobileMonthData(null);
			var unReadMessagetCount = result.UnReadMessageCount;

			unReadMessagetCount.Should().Be(1);
		}

		[Test]
		public void ShouldMapAsmEnabledForMobileMonthView()
		{
			var result = Target.FetchMobileMonthData(null);
			result.AsmEnabled.Should().Be.True();
		}
	}
}
