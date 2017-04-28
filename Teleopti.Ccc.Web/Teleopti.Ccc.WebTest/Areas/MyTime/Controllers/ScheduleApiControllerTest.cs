using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	[MyTimeWebTest]
	public class ScheduleApiControllerTest
	{
		public ScheduleApiController Target;
		public ICurrentScenario Scenario;
		public ILoggedOnUser User;
		public FakeScheduleDataReadScheduleStorage ScheduleData;
		public MutableNow Now;
		public FakeUserCulture Culture;
		public FakeUserTimeZone TimeZone;
		public FakePersonRequestRepository PersonRequestRepository;

		#region Test cases for FetchWeekData()
		[Test]
		public void ShouldMap()
		{
			var viewModel = Target.FetchWeekData(null);
			viewModel.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldMapTimeLineEdges()
		{
			Culture.IsSwedish();
			Now.Is("2015-03-29 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, new DateTimePeriod("2015-03-29 08:00".Utc(), "2015-03-29 17:00".Utc()));
			ScheduleData.Set(new IScheduleData[] { personAssignment });

			var viewModel = Target.FetchWeekData(date);

			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("07:45");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("17:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnDayBeforeDst()
		{
			Culture.IsSwedish();
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-03-28 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, new DateTimePeriod("2015-03-28 07:45".Utc(), "2015-03-28 17:00".Utc()));
			ScheduleData.Set(new IScheduleData[] { personAssignment });

			var viewModel = Target.FetchWeekData(date);

			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("08:30");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("18:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnFirstDstDay()
		{
			Culture.IsSwedish();
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-03-29 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, new DateTimePeriod("2015-03-29 07:45".Utc(), "2015-03-29 17:00".Utc()));
			ScheduleData.Set(new IScheduleData[] { personAssignment });

			var viewModel = Target.FetchWeekData(date);

			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("09:30");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("19:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnFirstDstDayAndNightShift()
		{
			Culture.IsSwedish();
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-03-29 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var phone = new Activity("p");
			var personAssignment1 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 3, 28));
			personAssignment1.AddActivity(phone, new DateTimePeriod("2015-03-28 00:00".Utc(), "2015-03-28 04:00".Utc()));
			var personAssignment2 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 3, 29));
			personAssignment2.AddActivity(phone, new DateTimePeriod("2015-03-29 00:00".Utc(), "2015-03-29 04:00".Utc()));
			ScheduleData.Set(new IScheduleData[] { personAssignment1, personAssignment2 });

			var viewModel = Target.FetchWeekData(date);

			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("00:45");
			viewModel.TimeLine.ElementAt(1).TimeLineDisplay.Should().Be("01:00");
			viewModel.TimeLine.ElementAt(2).TimeLineDisplay.Should().Be("02:00");
			viewModel.TimeLine.ElementAt(3).TimeLineDisplay.Should().Be("03:00");
			viewModel.TimeLine.ElementAt(4).TimeLineDisplay.Should().Be("04:00");
			viewModel.TimeLine.ElementAt(5).TimeLineDisplay.Should().Be("05:00");
			viewModel.TimeLine.ElementAt(6).TimeLineDisplay.Should().Be("06:00");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("06:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnEndDstDayAndNightShift()
		{
			Culture.IsSwedish();
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-10-25 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var phone = new Activity("p");
			var personAssignment1 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 10, 24));
			personAssignment1.AddActivity(phone, new DateTimePeriod("2015-10-24 00:00".Utc(), "2015-10-24 04:00".Utc()));
			var personAssignment2 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 10, 25));
			personAssignment2.AddActivity(phone, new DateTimePeriod("2015-10-25 00:00".Utc(), "2015-10-25 04:00".Utc()));
			ScheduleData.Set(new IScheduleData[] { personAssignment1, personAssignment2 });

			var viewModel = Target.FetchWeekData(date);

			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("01:45");
			viewModel.TimeLine.ElementAt(1).TimeLineDisplay.Should().Be("02:00");
			viewModel.TimeLine.ElementAt(2).TimeLineDisplay.Should().Be("03:00");
			viewModel.TimeLine.ElementAt(3).TimeLineDisplay.Should().Be("04:00");
			viewModel.TimeLine.ElementAt(4).TimeLineDisplay.Should().Be("05:00");
			viewModel.TimeLine.ElementAt(5).TimeLineDisplay.Should().Be("06:00");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("06:15");
		}

		[Test]
		public void ShouldMapBaseUtcOffset()
		{
			Culture.IsSwedish();
			TimeZone.IsHawaii();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-03-29 10:00");

			var viewModel = Target.FetchWeekData(null);
			viewModel.BaseUtcOffsetInMinutes.Should().Be(-10 * 60);
		}

		[Test]
		public void ShouldMapDaylightSavingTimeAdjustment()
		{
			Culture.IsSwedish();
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-03-29 10:00");

			var viewModel = Target.FetchWeekData(null);

			viewModel.DaylightSavingTimeAdjustment.Should().Not.Be.Null();
			viewModel.DaylightSavingTimeAdjustment.StartDateTime.Should().Be(new DateTime(2015, 3, 29, 1, 0, 0, DateTimeKind.Utc));
			viewModel.DaylightSavingTimeAdjustment.EndDateTime.Should().Be(new DateTime(2015, 10, 25, 2, 0, 0, DateTimeKind.Utc));
			viewModel.DaylightSavingTimeAdjustment.AdjustmentOffsetInMinutes.Should().Be(60);
		}

		[Test]
		public void ShouldNotMapDaylightSavingTimeAdjustment()
		{
			Culture.IsSwedish();
			TimeZone.IsChina();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-03-29 10:00");

			var viewModel = Target.FetchWeekData(null);
			Assert.IsNull(viewModel.DaylightSavingTimeAdjustment);
		}

		[Test]
		public void ShouldValidatePeriodSelectionStartDateAndEndDateFormatCorrectly()
		{
			Culture.IsSwedish();
			TimeZone.IsSweden();
			var date = new DateOnly(2015, 07, 06);
			var viewModel = Target.FetchWeekData(date);

			Assert.AreEqual("2015-07-06", viewModel.CurrentWeekStartDate);
			Assert.AreEqual("2015-07-12", viewModel.CurrentWeekEndDate);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapPeriodSelectionForWeek()
		{
			var result = Target.FetchWeekData(null).PeriodSelection;

			result.Date.Should().Be.EqualTo(new DateOnly(2014, 12, 18).ToFixedClientDateOnlyFormat());
			result.Display.Should().Be.EqualTo("2014-12-15 - 2014-12-21");

			result.PeriodNavigation.CanPickPeriod.Should().Be.False();
			result.PeriodNavigation.HasNextPeriod.Should().Be.True();
			result.PeriodNavigation.HasPrevPeriod.Should().Be.True();
			result.PeriodNavigation.NextPeriod.Should().Be.EqualTo(new DateOnly(2014, 12, 22).ToFixedClientDateOnlyFormat());
			result.PeriodNavigation.PrevPeriod.Should().Be.EqualTo(new DateOnly(2014, 12, 14).ToFixedClientDateOnlyFormat());

			result.SelectedDateRange.MinDate.Should().Be.EqualTo(new DateOnly(2014, 12, 15).ToFixedClientDateOnlyFormat());
			result.SelectedDateRange.MaxDate.Should().Be.EqualTo(new DateOnly(2014, 12, 21).ToFixedClientDateOnlyFormat());

			result.SelectableDateRange.MinDate.Should().Be.EqualTo(DateOnly.MinValue.ToFixedClientDateOnlyFormat());
			result.SelectableDateRange.MaxDate.Should().Be.EqualTo(DateOnly.MaxValue.ToFixedClientDateOnlyFormat());
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldCreatePeriodViewModelFromActivityLayer()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 15));
			var period = new DateTimePeriod(2014, 12, 15, 8, 2014, 12, 15, 17);
			assignment.AddActivity(new Activity("Phone") { InWorkTime = true, InContractTime = true, DisplayColor = Color.Green }, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null);

			var layerDetails = result.Days.First().Periods.Single();
			layerDetails.StyleClassName.Should().Be.EqualTo("color_008000");
			layerDetails.Summary.Should().Be.EqualTo("9:00");
			layerDetails.Title.Should().Be.EqualTo("Phone");
			layerDetails.TimeSpan.Should().Be.EqualTo("08:00 - 17:00");
			layerDetails.Meeting.Should().Be.Null();
			layerDetails.Color.Should().Be.EqualTo("0,128,0");
			layerDetails.StartPositionPercentage.Should().Be.EqualTo(0.25M / 9.5M);
			layerDetails.EndPositionPercentage.Should().Be.EqualTo(9.25M / 9.5M);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldCreatePeriodViewModelFromActivityLayerForNightShift()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 15));
			var period = new DateTimePeriod(2014, 12, 15, 18, 2014, 12, 16, 02);
			assignment.AddActivity(new Activity("Phone") { InWorkTime = true, InContractTime = true, DisplayColor = Color.Green },
				period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null);
			var layerDetails = result.Days.First().Periods.Single();
			layerDetails.StyleClassName.Should().Be.EqualTo("color_008000");
			layerDetails.Summary.Should().Be.EqualTo("8:00");
			layerDetails.Title.Should().Be.EqualTo("Phone");
			layerDetails.TimeSpan.Should().Be.EqualTo("18:00 - 02:00 +1");
			layerDetails.Meeting.Should().Be.Null();
			layerDetails.Color.Should().Be.EqualTo("0,128,0");
			layerDetails.StartPositionPercentage.Should().Be.EqualTo(18M / (24M - 1 / 3600M));
			layerDetails.EndPositionPercentage.Should().Be.EqualTo(1M);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldCreatePeriodViewModelFromAbsenceLayer()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 15));
			var period = new DateTimePeriod(2014, 12, 15, 08, 2014, 12, 15, 17);
			assignment.AddActivity(new Activity("Phone") { InWorkTime = true, InContractTime = true, DisplayColor = Color.Green },
				period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);
			ScheduleData.Add(new PersonAbsence(User.CurrentUser(), Scenario.Current()
				, new AbsenceLayer(new Absence { Description = new Description("Holiday", "HO"), DisplayColor = Color.Red }, period)));

			var result = Target.FetchWeekData(null);

			var layerDetails = result.Days.First().Periods.Single();
			layerDetails.StyleClassName.Should().Be.EqualTo("color_FF0000");
			layerDetails.Summary.Should().Be.EqualTo("9:00");
			layerDetails.Title.Should().Be.EqualTo("Holiday");
			layerDetails.TimeSpan.Should().Be.EqualTo("08:00 - 17:00");
			layerDetails.Meeting.Should().Be.Null();
			layerDetails.Color.Should().Be.EqualTo("255,0,0");
			layerDetails.StartPositionPercentage.Should().Be.EqualTo(0.25M / 9.5M);
			layerDetails.EndPositionPercentage.Should().Be.EqualTo(9.25M / 9.5M);
		}


		[Test, SetCulture("sv-SE")]
		public void ShouldCreatePeriodViewModelFromOvertimeLayer()
		{
			var definitionSet = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("Overtime",
				MultiplicatorType.Overtime);

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 15));
			var period = new DateTimePeriod(2014, 12, 15, 08, 2014, 12, 15, 17);
			assignment.AddOvertimeActivity(
				new Activity("Phone") { InWorkTime = true, InContractTime = true, DisplayColor = Color.Green },
				period, definitionSet);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null);

			var layerDetails = result.Days.First().Periods.Single();
			layerDetails.StyleClassName.Should().Be.EqualTo("color_008000");
			layerDetails.Summary.Should().Be.EqualTo("9:00");
			layerDetails.Title.Should().Be.EqualTo("Phone");
			layerDetails.TimeSpan.Should().Be.EqualTo("08:00 - 17:00");
			layerDetails.Meeting.Should().Be.Null();
			layerDetails.Color.Should().Be.EqualTo("0,128,0");
			layerDetails.StartPositionPercentage.Should().Be.EqualTo(0.25M / 9.5M);
			layerDetails.EndPositionPercentage.Should().Be.EqualTo(9.25M / 9.5M);
			layerDetails.IsOvertime.Should().Be(true);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldCreatePeriodViewModelFromMeetingLayer()
		{
			var meeting = new Meeting(User.CurrentUser(), new[] { new MeetingPerson(User.CurrentUser(), false) }, "subj", "loc",
				"desc",
				new Activity("Phone") { InWorkTime = true, InContractTime = true, DisplayColor = Color.Green }, null);
			meeting.SetScenario(Scenario.Current());
			meeting.StartDate = meeting.EndDate = new DateOnly(2014, 12, 15);
			meeting.StartTime = TimeSpan.FromHours(16);
			meeting.EndTime = TimeSpan.FromHours(17);
			ScheduleData.Set(meeting.GetPersonMeetings(User.CurrentUser()).OfType<IScheduleData>().ToList());

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 15));
			var period = new DateTimePeriod(2014, 12, 15, 8, 2014, 12, 15, 17);
			assignment.AddActivity(new Activity("Phone") { InWorkTime = true, InContractTime = true, DisplayColor = Color.Green },
				period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null);

			var layerDetails = result.Days.First().Periods.Last();
			layerDetails.StyleClassName.Should().Be.EqualTo("color_008000");
			layerDetails.Summary.Should().Be.EqualTo("1:00");
			layerDetails.Title.Should().Be.EqualTo("Phone");
			layerDetails.TimeSpan.Should().Be.EqualTo("16:00 - 17:00");
			layerDetails.Meeting.Title.Should().Be.EqualTo("subj");
			layerDetails.Meeting.Location.Should().Be.EqualTo("loc");
			layerDetails.Meeting.Description.Should().Be.EqualTo("desc");
			layerDetails.Color.Should().Be.EqualTo("0,128,0");
			layerDetails.StartPositionPercentage.Should().Be.EqualTo(8.25M / 9.5M);
			layerDetails.EndPositionPercentage.Should().Be.EqualTo(9.25M / 9.5M);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldCreateOvertimeAvailabilityPeriodViewModelNotSpanToTomorrow()
		{
			var start = new TimeSpan(12, 0, 0);
			var end = new TimeSpan(24, 0, 1);
			var overtimeAvailability = new OvertimeAvailability(User.CurrentUser(), new DateOnly(2014, 12, 15), start, end);
			ScheduleData.Add(overtimeAvailability);

			var result = Target.FetchWeekData(null).Days.First().Periods.Single() as OvertimeAvailabilityPeriodViewModel;
			result.Title.Should().Be.EqualTo(Resources.OvertimeAvailabilityWeb);
			result.TimeSpan.Should()
				.Equals(TimeHelper.TimeOfDayFromTimeSpan(start, CultureInfo.CurrentCulture) + " - " +
						TimeHelper.TimeOfDayFromTimeSpan(end, CultureInfo.CurrentCulture));
			result.StartPositionPercentage.Should().Be.EqualTo(12M / (24M - 1M / 3600M));
			result.EndPositionPercentage.Should().Be.EqualTo(1M);
			result.IsOvertimeAvailability.Should().Be.True();
			result.Color.Should().Be.EqualTo(Color.Gray.ToCSV());
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldCreateOvertimeAvailabilityPeriodViewModel()
		{
			var start = new TimeSpan(12, 0, 0);
			var end = new TimeSpan(13, 0, 0);
			var overtimeAvailability = new OvertimeAvailability(User.CurrentUser(), new DateOnly(2014, 12, 15), start, end);
			ScheduleData.Add(overtimeAvailability);

			var result = Target.FetchWeekData(null).Days.First().Periods.Single() as OvertimeAvailabilityPeriodViewModel;
			result.Title.Should().Be.EqualTo(Resources.OvertimeAvailabilityWeb);
			result.TimeSpan.Should()
				.Equals(TimeHelper.TimeOfDayFromTimeSpan(start, CultureInfo.CurrentCulture) + " - " +
						TimeHelper.TimeOfDayFromTimeSpan(end, CultureInfo.CurrentCulture));
			result.StartPositionPercentage.Should().Be.EqualTo(0.25M / 1.5M);
			result.EndPositionPercentage.Should().Be.EqualTo(1.25M / 1.5M);
			result.IsOvertimeAvailability.Should().Be.True();
			result.Color.Should().Be.EqualTo(Color.Gray.ToCSV());
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldCreateOvertimeAvailabilityPeriodViewModelForYesterday()
		{
			var start = new TimeSpan(12, 0, 0);
			var end = new TimeSpan(25, 0, 0);
			var overtimeAvailability = new OvertimeAvailability(User.CurrentUser(), new DateOnly(2014, 12, 14), start, end);
			ScheduleData.Add(overtimeAvailability);

			var result = Target.FetchWeekData(null).Days.First().Periods.Single() as OvertimeAvailabilityPeriodViewModel;

			result.Title.Should().Be.EqualTo(Resources.OvertimeAvailabilityWeb);
			result.TimeSpan.Should()
				.Equals(TimeHelper.TimeOfDayFromTimeSpan(start, CultureInfo.CurrentCulture) + " - " +
						TimeHelper.TimeOfDayFromTimeSpan(end, CultureInfo.CurrentCulture));
			result.StartPositionPercentage.Should().Be.EqualTo(0);
			result.EndPositionPercentage.Should().Be.EqualTo(1M / 1.25M);
			result.IsOvertimeAvailability.Should().Be.True();
			result.Color.Should().Be.EqualTo(Color.Gray.ToCSV());
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldNotCreateOvertimeAvailabilityPeriodViewModelForYesterdayIfNotSpanToToday()
		{
			var start = new TimeSpan(12, 0, 0);
			var end = new TimeSpan(13, 0, 0);
			var overtimeAvailability = new OvertimeAvailability(User.CurrentUser(), new DateOnly(2014, 12, 14), start, end);
			ScheduleData.Add(overtimeAvailability);

			var result = Target.FetchWeekData(null).Days.First().Periods;

			result.Should().Be.Empty();
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapDayOfWeekNumber()
		{
			var result = Target.FetchWeekData(null);

			result.Days.First().DayOfWeekNumber.Should().Be.EqualTo((int)new DateOnly(2014, 12, 15).DayOfWeek);
		}

		[Test]
		public void ShouldMapAvailability()
		{
			var result = Target.FetchWeekData(null);

			result.Days.First().Availability.Should().Be.EqualTo(false);
		}


		[Test, SetCulture("sv-SE")]
		public void ShouldMapStateToday()
		{
			var result = Target.FetchWeekData(null);

			result.Days.ElementAt(3).State.Should().Be.EqualTo(SpecialDateState.Today);
		}


		[Test, SetCulture("sv-SE")]
		public void ShouldMapNoSpecialState()
		{
			var result = Target.FetchWeekData(Now.LocalDateOnly().AddDays(-2));

			result.Days.First().State.Should().Be.EqualTo((SpecialDateState)0);
		}


		[Test, SetCulture("sv-SE"), SetUICulture("sv-SE")]
		public void ShouldMapDayHeader()
		{
			var date = Now.LocalDateOnly().AddDays(-2);
			var result = Target.FetchWeekData(date).Days.ElementAt(1);

			result.Header.Date.Should().Be.EqualTo(date.ToShortDateString());
			result.Header.Title.Should().Be.EqualTo("tisdag");
			result.Header.DayDescription.Should().Be.Empty();
			result.Header.DayNumber.Should().Be.EqualTo("16");
		}

		[Test, SetCulture("sv-SE"), SetUICulture("sv-SE")]
		public void ShouldMapDayHeaderWithMontNameForFirstDayOfWeek()
		{
			var date = Now.LocalDateOnly().AddDays(-2);
			var result = Target.FetchWeekData(date).Days.First();

			result.Header.DayDescription.Should().Be.EqualTo("december");
		}

		[Test, SetCulture("sv-SE"), SetUICulture("sv-SE")]
		public void ShouldMapDayHeaderWithMontNameForFirstDayOfMonth()
		{
			var result = Target.FetchWeekData(new DateOnly(2014, 12, 29)).Days.ElementAt(3);

			result.Header.DayDescription.Should().Be.EqualTo("januari");
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapPublicNote()
		{
			ScheduleData.Add(new PublicNote(User.CurrentUser(), Now.LocalDateOnly(), Scenario.Current(), "TestNote"));

			var result = Target.FetchWeekData(null);

			result.Days.ElementAt(3).Note.Message.Should().Be.EqualTo("TestNote");
		}


		[Test, SetCulture("sv-SE")]
		public void ShouldMapOvertimeAvailability()
		{
			var overtimeAvailability = new OvertimeAvailability(User.CurrentUser(), Now.LocalDateOnly(), new TimeSpan(1, 1, 1), new TimeSpan(1, 2, 2, 2));
			ScheduleData.Add(overtimeAvailability);

			var result = Target.FetchWeekData(null).Days.ElementAt(3);
			result.OvertimeAvailabililty.StartTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(overtimeAvailability.StartTime.Value, CultureInfo.GetCultureInfo("sv-SE")));
			result.OvertimeAvailabililty.EndTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(overtimeAvailability.EndTime.Value, CultureInfo.GetCultureInfo("sv-SE")));
			result.OvertimeAvailabililty.EndTimeNextDay.Should().Be.EqualTo(true);
			result.OvertimeAvailabililty.HasOvertimeAvailability.Should().Be.EqualTo(true);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapOvertimeAvailabilityDefaultValuesForEmpty()
		{
			ScheduleData.Add(new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly()));

			var result = Target.FetchWeekData(null).Days.ElementAt(3);
			result.OvertimeAvailabililty.DefaultStartTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(new TimeSpan(8, 0, 0), CultureInfo.CurrentCulture));
			result.OvertimeAvailabililty.DefaultEndTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(new TimeSpan(17, 0, 0), CultureInfo.CurrentCulture));
			result.OvertimeAvailabililty.DefaultEndTimeNextDay.Should().Be.EqualTo(false);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapOvertimeAvailabilityDefaultValuesForDayOff()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			assignment.SetDayOff(new DayOffTemplate(new Description("Day off", "DO")), true);
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null).Days.ElementAt(3);
			result.OvertimeAvailabililty.DefaultStartTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(new TimeSpan(8, 0, 0), CultureInfo.CurrentCulture));
			result.OvertimeAvailabililty.DefaultEndTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(new TimeSpan(17, 0, 0), CultureInfo.CurrentCulture));
			result.OvertimeAvailabililty.DefaultEndTimeNextDay.Should().Be.EqualTo(false);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapOvertimeAvailabilityDefaultValuesIfHasShift()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var dateTimePeriod = new DateTimePeriod(new DateTime(2014, 12, 18, 6, 0, 0, DateTimeKind.Utc), new DateTime(2014, 12, 18, 15, 0, 0, DateTimeKind.Utc));
			assignment.AddActivity(new Activity("a") { InWorkTime = true }, dateTimePeriod);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null).Days.ElementAt(3);
			var timeZone = User.CurrentUser().PermissionInformation.DefaultTimeZone();
			result.OvertimeAvailabililty.DefaultStartTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(dateTimePeriod.TimePeriod(timeZone).EndTime, CultureInfo.CurrentCulture));
			result.OvertimeAvailabililty.DefaultEndTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(dateTimePeriod.TimePeriod(timeZone).EndTime.Add(TimeSpan.FromHours(1)), CultureInfo.CurrentCulture));
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapTextRequestCount()
		{
			var period = new DateTimePeriod(2014, 12, 18, 8, 2014, 12, 18, 17);
			var textRequest = new PersonRequest(User.CurrentUser(), new TextRequest(period));
			PersonRequestRepository.Add(textRequest);
			PersonRequestRepository.Add(textRequest);

			var result = Target.FetchWeekData(null).Days.ElementAt(3);
			result.TextRequestCount.Should().Be.EqualTo(2);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapSummaryForDayWithOtherSignificantPart()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null).Days.ElementAt(3);
			result.Summary.Should().Not.Be.Null();
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapSummaryForDayOff()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			assignment.SetDayOff(new DayOffTemplate(new Description("Day off", "DO")), true);
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null).Days.ElementAt(3);
			result.Summary.Title.Should().Be.EqualTo("Day off");
			result.Summary.StyleClassName.Should().Contain(StyleClasses.DayOff);
			result.Summary.StyleClassName.Should().Contain(StyleClasses.Striped);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapSummaryForMainShift()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period = new DateTimePeriod(2014, 12, 18, 7, 2014, 12, 18, 16);
			assignment.AddActivity(new Activity("a") { InWorkTime = true }, period);
			var shiftCategory = new ShiftCategory("sc");
			assignment.SetShiftCategory(shiftCategory);
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null).Days.ElementAt(3);
			result.Summary.TimeSpan.Should().Be.EqualTo(period.TimePeriod(User.CurrentUser().PermissionInformation.DefaultTimeZone()).ToShortTimeString());
			result.Summary.Title.Should().Be.EqualTo(shiftCategory.Description.Name);
			result.Summary.StyleClassName.Should().Be.EqualTo(shiftCategory.DisplayColor.ToStyleClass());
			result.Summary.Summary.Should().Be.EqualTo(TimeHelper.GetLongHourMinuteTimeString(period.ElapsedTime(), CultureInfo.CurrentUICulture));
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldNotMapPersonalActivityToSummaryTimespan()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period = new DateTimePeriod(2014, 12, 18, 7, 2014, 12, 18, 16);
			assignment.AddActivity(new Activity("a") { InWorkTime = true }, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			assignment.AddPersonalActivity(new Activity("b") { InWorkTime = true }, period.MovePeriod(TimeSpan.FromHours(-2)));
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null).Days.ElementAt(3);
			result.Summary.TimeSpan.Should()
				.Be.EqualTo(period.TimePeriod(User.CurrentUser().PermissionInformation.DefaultTimeZone()).ToShortTimeString());
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapSummaryForAbsence()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 15));
			assignment.AddActivity(new Activity("Phone") { InContractTime = true }, new DateTimePeriod(2014, 12, 15, 8, 2014, 12, 15, 17), true);
			ScheduleData.Add(assignment);

			var absenceToDisplay = new PersonAbsence(User.CurrentUser(), Scenario.Current(),
				new AbsenceLayer(new Absence { Description = new Description("Illness", "IL"), InContractTime = true },
					new DateTimePeriod(2014, 12, 15, 8, 2014, 12, 15, 17)));
			absenceToDisplay.Layer.Payload.Priority = 1;
			ScheduleData.Add(absenceToDisplay);

			var highPriority = new PersonAbsence(User.CurrentUser(), Scenario.Current(),
				new AbsenceLayer(new Absence { Description = new Description("Holiday", "HO"), InContractTime = true },
					new DateTimePeriod(2014, 12, 15, 8, 2014, 12, 15, 17)));
			highPriority.Layer.Payload.Priority = 2;
			ScheduleData.Add(highPriority);

			var result = Target.FetchWeekData(null).Days.First();
			result.Summary.Title.Should().Be.EqualTo(absenceToDisplay.Layer.Payload.Description.Name);
			result.Summary.StyleClassName.Should().Be.EqualTo(absenceToDisplay.Layer.Payload.DisplayColor.ToStyleClass());
			result.Summary.Summary.Should().Be.EqualTo(TimeHelper.GetLongHourMinuteTimeString(TimeSpan.FromHours(9), CultureInfo.CurrentUICulture));
		}


		[Test, SetCulture("sv-SE")]
		public void ShouldMapStyleClassViewModelsFromScheduleColors()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period = new DateTimePeriod(2014, 12, 18, 7, 2014, 12, 18, 16);
			assignment.AddActivity(new Activity("a") { InWorkTime = true, DisplayColor = Color.Blue }, period);
			assignment.SetShiftCategory(new ShiftCategory("sc") { DisplayColor = Color.Red });
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null);

			result.Styles.Select(s => s.Name)
				.Should().Have.SameValuesAs(Color.Blue.ToStyleClass(), Color.Red.ToStyleClass());
			result.Styles.Select(s => s.ColorHex)
				.Should().Have.SameValuesAs(Color.Blue.ToHtml(), Color.Red.ToHtml());
			result.Styles.Select(s => s.RgbColor)
				.Should().Have.SameValuesAs(Color.Blue.ToCSV(), Color.Red.ToCSV());
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapStyleClassForAbsenceOnPersonDayOff()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 15));
			assignment.SetDayOff(new DayOffTemplate(new Description("Day off", "DO")), true);
			ScheduleData.Add(assignment);
			var absence = new PersonAbsence(User.CurrentUser(), Scenario.Current(),
				new AbsenceLayer(new Absence { Description = new Description("Illness", "IL"), InContractTime = true },
					new DateTimePeriod(2014, 12, 15, 8, 2014, 12, 15, 17)));
			ScheduleData.Add(absence);

			var result = Target.FetchWeekData(null).Days.First();
			result.Summary.StyleClassName.Should().Contain(StyleClasses.Striped);
		}

		[Test]
		public void ShouldMapTextRequestPermission()
		{
			var result = Target.FetchWeekData(null);

			result.RequestPermission.TextRequestPermission.Should().Be.True();
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapTimeLine()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 8, 45, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 17, 15, 0, DateTimeKind.Utc));
			assignment.AddActivity(new Activity("a") { InWorkTime = true, DisplayColor = Color.Blue }, period);
			assignment.SetShiftCategory(new ShiftCategory("sc") { DisplayColor = Color.Red });
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null);
			result.TimeLine.Count().Should().Be.EqualTo(11);
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(8);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(30);
			result.TimeLine.First().PositionPercentage.Should().Be.EqualTo(0.0);
			result.TimeLine.ElementAt(1).Time.Hours.Should().Be.EqualTo(9);
			result.TimeLine.ElementAt(1).Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.ElementAt(1).PositionPercentage.Should().Be.EqualTo(0.5 / (17.5 - 8.5));
		}

		[Test]
		public void ShouldMapAsmPermission()
		{
			var result = Target.FetchWeekData(null);
			result.AsmPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapViewPossibilityPermission()
		{
			var result = Target.FetchWeekData(null);
			result.ViewPossibilityPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapAbsenceRequestPermission()
		{
			var result = Target.FetchWeekData(null);
			result.RequestPermission.AbsenceRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapOvertimeAvailabilityPermission()
		{
			var result = Target.FetchWeekData(null);
			result.RequestPermission.OvertimeAvailabilityPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapIsCurrentWeek()
		{
			var result = Target.FetchWeekData(null);
			result.IsCurrentWeek.Should().Be.True();
		}

		[Test]
		public void ShouldMapSiteOpenHourIntradayTimePeriod()
		{
			var timePeriod = new TimePeriod(8, 0, 17, 0);
			var team = TeamFactory.CreateTeam("team1", "site1");
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = timePeriod,
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});
			User.CurrentUser().AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(Now.LocalDateOnly(), team));

			var result = Target.FetchWeekData(null);
			result.SiteOpenHourIntradayPeriod.Equals(timePeriod).Should().Be.True();
		}


		[Test, SetCulture("sv-SE")]
		public void ShouldMapDateFormatForUser()
		{
			Culture.IsSwedish();

			var result = Target.FetchWeekData(null);
			var expectedFormat = User.CurrentUser().PermissionInformation.Culture().DateTimeFormat.ShortDatePattern;
			result.DatePickerFormat.Should().Be.EqualTo(expectedFormat);
		}

		[Test]
		public void ShouldAdjustTimelineForOverTimeWhenSiteOpenHourPeriodContainsSchedulePeriod()
		{
			var timePeriod = new TimePeriod(8, 0, 17, 0);
			var team = TeamFactory.CreateTeam("team1", "site1");
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = timePeriod,
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});
			User.CurrentUser().AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(Now.LocalDateOnly(), team));

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			assignment.AddActivity(new Activity("a") { InWorkTime = true, DisplayColor = Color.Blue }, period);
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Overtime);
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(8);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(17);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldNotAdjustTimelineForOverTimeWhenSchedulePeriodContainsSiteOpenHourPeriod()
		{
			var timePeriod = new TimePeriod(8, 0, 17, 0);
			var team = TeamFactory.CreateTeam("team1", "site1");
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = timePeriod,
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});
			User.CurrentUser().AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(Now.LocalDateOnly(), team));

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 7, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 17, 45, 0, DateTimeKind.Utc));
			assignment.AddActivity(new Activity("a") { InWorkTime = true, DisplayColor = Color.Blue }, period);
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Overtime);
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(7);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(18);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldNotAdjustTimelineBySiteOpenHourWhenAskForAbsence()
		{
			var timePeriod = new TimePeriod(8, 0, 17, 0);
			var team = TeamFactory.CreateTeam("team1", "site1");
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = timePeriod,
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});
			User.CurrentUser().AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(Now.LocalDateOnly(), team));

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			assignment.AddActivity(new Activity("a") { InWorkTime = true, DisplayColor = Color.Blue }, period);
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Absence);
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(9);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(10);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldNotAdjustTimelineForOverTimeWhenNoSiteOpenHourAvailable()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			assignment.AddActivity(new Activity("a") { InWorkTime = true, DisplayColor = Color.Blue }, period);
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Overtime);
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(9);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(10);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReportNoNoteWhenNull()
		{
			var result = Target.FetchWeekData(null).Days.First();
			result.HasNote.Should().Be.False();
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldGetSiteOpenHourPeriodForDayView()
		{
			var date = new DateOnly(2014, 12, 18);
			var team = TeamFactory.CreateTeam("team1", "site1");
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(9, 0, 10, 0),
				WeekDay = DayOfWeek.Thursday
			});
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(10, 0, 11, 0),
				WeekDay = DayOfWeek.Friday
			});
			User.CurrentUser().AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(date, team));
			var result = Target.FetchWeekData(null);
			result.Days.ElementAt(3).SiteOpenHourPeriod.Value.StartTime.Should().Be(TimeSpan.FromHours(9));
			result.Days.ElementAt(3).SiteOpenHourPeriod.Value.EndTime.Should().Be(TimeSpan.FromHours(10));
			result.Days.ElementAt(4).SiteOpenHourPeriod.Value.StartTime.Should().Be(TimeSpan.FromHours(10));
			result.Days.ElementAt(4).SiteOpenHourPeriod.Value.EndTime.Should().Be(TimeSpan.FromHours(11));
		}
		#endregion

		#region Test cases for FetchMonthData()
		[Test, SetCulture("sv-SE")]
		public void ShouldHaveTheFirstDayOfTheFirstWeekInMonth()
		{
			var result = Target.FetchMonthData(null);
			result.ScheduleDays.First().Date.Should().Be.EqualTo(new DateTime(2014, 12, 1));
			result.ScheduleDays.First().FixedDate.Should().Be.EqualTo(new DateOnly(2014, 12, 1).ToFixedClientDateOnlyFormat());
		}

		[Test, SetCulture("sv-SE")]
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

		[Test, SetCulture("sv-SE")]
		public void ShouldMapAbsenceName()
		{
			ScheduleData.Add(new PersonAbsence(User.CurrentUser(), Scenario.Current(),
				new AbsenceLayer(new Absence { Description = new Description("Illness", "IL") }, new DateTimePeriod(2014, 12, 1, 8, 2014, 12, 1, 17))));

			var result = Target.FetchMonthData(null);

			result.ScheduleDays.First().Absence.Name.Should().Be.EqualTo("Illness");
			result.ScheduleDays.First().Absence.ShortName.Should().Be.EqualTo("IL");
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapAbsenceRespectingPriority()
		{
			ScheduleData.Add(new PersonAbsence(User.CurrentUser(), Scenario.Current(),
							new AbsenceLayer(new Absence { Description = new Description("a", "IL"), Priority = 1 }, new DateTimePeriod(2014, 12, 1, 8, 2014, 12, 1, 17))));

			ScheduleData.Add(new PersonAbsence(User.CurrentUser(), Scenario.Current(),
							new AbsenceLayer(new Absence { Description = new Description("a", "HO"), Priority = 100 }, new DateTimePeriod(2014, 12, 1, 8, 2014, 12, 1, 17))));

			var result = Target.FetchMonthData(null);
			result.ScheduleDays.First().Absence.ShortName.Should().Be.EqualTo("IL");
		}

		[Test, SetCulture("sv-SE")]
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

		[Test, SetCulture("sv-SE")]
		public void ShouldMapIsDayOff()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 01));
			assignment.SetDayOff(new DayOffTemplate(new Description("Day off", "DO")), true);
			ScheduleData.Add(assignment);

			var result = Target.FetchMonthData(null);
			result.ScheduleDays.First().IsDayOff.Should().Be.True();
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapIsNotDayOffForWorkingDay()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 01));
			assignment.AddActivity(new Activity("Phone"), new DateTimePeriod(2014, 12, 1, 8, 2014, 12, 1, 17), true);
			ScheduleData.Add(assignment);

			var result = Target.FetchMonthData(null);
			result.ScheduleDays.First().IsDayOff.Should().Be.False();
		}

		[Test, SetCulture("sv-SE")]
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
		#endregion

		#region Test cases for FetchDayData()
		[Test]
		public void ShouldMapScheduleForTodayWithoutParameter()
		{
			var result = Target.FetchDayData(null);
			result.Should().Not.Be.Null();
			result.IsToday.Should().Be.True();
			result.Schedule.State.Should().Be.EqualTo(SpecialDateState.Today);
			result.Schedule.Availability.Should().Be.EqualTo(false);
			result.Schedule.HasNote.Should().Be.False();
		}

		[Test]
		public void ShouldMapTimeLineEdgesOnFetchDayData()
		{
			Culture.IsSwedish();
			var date = new DateOnly(2015, 03, 29);
			var timePeriod = new DateTimePeriod("2015-03-29 08:00".Utc(), "2015-03-29 17:00".Utc());
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, timePeriod);
			ScheduleData.Set(new IScheduleData[] { personAssignment });

			var viewModel = Target.FetchDayData(date);
			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("07:45");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("17:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnDayBeforeDstOnFetchDayData()
		{
			Culture.IsSwedish();
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());

			var date = new DateOnly(2015, 03, 28);
			var timePeriod = new DateTimePeriod("2015-03-28 07:45".Utc(), "2015-03-28 17:00".Utc());
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, timePeriod);
			ScheduleData.Set(new IScheduleData[] { personAssignment });

			var viewModel = Target.FetchDayData(date);
			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("08:30");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("18:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnFirstDstDayOnFetchDayData()
		{
			Culture.IsSwedish();
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-03-29 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, new DateTimePeriod("2015-03-29 07:45".Utc(), "2015-03-29 17:00".Utc()));
			ScheduleData.Set(new IScheduleData[] { personAssignment });

			var viewModel = Target.FetchDayData(date);
			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("09:30");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("19:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnFirstDstDayAndNightShiftOnFetchDayData()
		{
			Culture.IsSwedish();
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());

			var date = new DateOnly(2015, 03, 29);
			var phone = new Activity("p");
			var personAssignment1 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 3, 28));
			personAssignment1.AddActivity(phone, new DateTimePeriod("2015-03-28 00:00".Utc(), "2015-03-28 04:00".Utc()));
			var personAssignment2 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 3, 29));
			personAssignment2.AddActivity(phone, new DateTimePeriod("2015-03-29 00:00".Utc(), "2015-03-29 04:00".Utc()));
			ScheduleData.Set(new IScheduleData[] { personAssignment1, personAssignment2 });

			var viewModel = Target.FetchDayData(date);
			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("00:45");
			viewModel.TimeLine.ElementAt(1).TimeLineDisplay.Should().Be("01:00");
			viewModel.TimeLine.ElementAt(2).TimeLineDisplay.Should().Be("02:00");
			viewModel.TimeLine.ElementAt(3).TimeLineDisplay.Should().Be("03:00");
			viewModel.TimeLine.ElementAt(4).TimeLineDisplay.Should().Be("04:00");
			viewModel.TimeLine.ElementAt(5).TimeLineDisplay.Should().Be("05:00");
			viewModel.TimeLine.ElementAt(6).TimeLineDisplay.Should().Be("06:00");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("06:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnEndDstDayAndNightShiftOnFetchDayData()
		{
			Now.Is("2015-10-25 10:00");
			var date = new DateOnly(Now.UtcDateTime());

			Culture.IsSwedish();
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			var phone = new Activity("p");
			var personAssignment1 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 10, 24));
			personAssignment1.AddActivity(phone, new DateTimePeriod("2015-10-24 00:00".Utc(), "2015-10-24 04:00".Utc()));
			var personAssignment2 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 10, 25));
			personAssignment2.AddActivity(phone, new DateTimePeriod("2015-10-25 00:00".Utc(), "2015-10-25 04:00".Utc()));
			ScheduleData.Set(new IScheduleData[] { personAssignment1, personAssignment2 });

			var viewModel = Target.FetchDayData(date);
			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("01:45");
			viewModel.TimeLine.ElementAt(1).TimeLineDisplay.Should().Be("02:00");
			viewModel.TimeLine.ElementAt(2).TimeLineDisplay.Should().Be("03:00");
			viewModel.TimeLine.ElementAt(3).TimeLineDisplay.Should().Be("04:00");
			viewModel.TimeLine.ElementAt(4).TimeLineDisplay.Should().Be("05:00");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("05:15");
		}

		[Test]
		public void ShouldMapBaseUtcOffsetOnFetchDayData()
		{
			Culture.IsSwedish();
			TimeZone.IsHawaii();

			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());

			var date = new DateOnly(2015, 03, 29);
			var viewModel = Target.FetchDayData(date);
			viewModel.BaseUtcOffsetInMinutes.Should().Be(-10 * 60);
		}

		[Test]
		public void ShouldMapDaylightSavingTimeAdjustmentOnFetchDayData()
		{
			Culture.IsSwedish();
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());

			Now.Is("2015-03-29 10:00");
			var viewModel = Target.FetchDayData(null);
			viewModel.DaylightSavingTimeAdjustment.Should().Not.Be.Null();
			viewModel.DaylightSavingTimeAdjustment.StartDateTime.Should()
				.Be(new DateTime(2015, 3, 29, 1, 0, 0, DateTimeKind.Utc));
			viewModel.DaylightSavingTimeAdjustment.EndDateTime.Should()
				.Be(new DateTime(2015, 10, 25, 2, 0, 0, DateTimeKind.Utc));
			viewModel.DaylightSavingTimeAdjustment.AdjustmentOffsetInMinutes.Should().Be(60);
		}

		[Test]
		public void ShouldNotMapDaylightSavingTimeAdjustmentOnFetchDayData()
		{
			Culture.IsSwedish();
			TimeZone.IsChina();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());

			var date = new DateOnly(2015, 03, 29);
			var viewModel = Target.FetchDayData(date);
			Assert.IsNull(viewModel.DaylightSavingTimeAdjustment);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldCreatePeriodViewModelFromActivityLayerOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 15);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var period = new DateTimePeriod(2014, 12, 15, 8, 2014, 12, 15, 17);
			var phoneActivity = new Activity("Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Green
			};
			assignment.AddActivity(phoneActivity, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date);
			var layerDetails = result.Schedule.Periods.Single();
			layerDetails.StyleClassName.Should().Be.EqualTo("color_008000");
			layerDetails.Summary.Should().Be.EqualTo("9:00");
			layerDetails.Title.Should().Be.EqualTo("Phone");
			layerDetails.TimeSpan.Should().Be.EqualTo("08:00 - 17:00");
			layerDetails.Meeting.Should().Be.Null();
			layerDetails.Color.Should().Be.EqualTo("0,128,0");
			layerDetails.StartPositionPercentage.Should().Be.EqualTo(0.25M / 9.5M);
			layerDetails.EndPositionPercentage.Should().Be.EqualTo(9.25M / 9.5M);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldCreatePeriodViewModelFromActivityLayerForNightShiftOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 15);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var period = new DateTimePeriod(2014, 12, 15, 18, 2014, 12, 16, 02);
			var phoneActivity = new Activity("Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Green
			};
			assignment.AddActivity(phoneActivity, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date);
			var layerDetails = result.Schedule.Periods.Single();
			layerDetails.StyleClassName.Should().Be.EqualTo("color_008000");
			layerDetails.Summary.Should().Be.EqualTo("8:00");
			layerDetails.Title.Should().Be.EqualTo("Phone");
			layerDetails.TimeSpan.Should().Be.EqualTo("18:00 - 02:00 +1");
			layerDetails.Meeting.Should().Be.Null();
			layerDetails.Color.Should().Be.EqualTo("0,128,0");
			layerDetails.StartPositionPercentage.Should()
				.Be.EqualTo(15M * 60 / ((23M * 3600M + 59M * 60 + 59) - (17M * 3600M + 45M * 60)));
			layerDetails.EndPositionPercentage.Should().Be.EqualTo(1M);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldCreatePeriodViewModelFromAbsenceLayerOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 15);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var period = new DateTimePeriod(2014, 12, 15, 08, 2014, 12, 15, 17);
			var phoneActivity = new Activity("Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Green
			};
			assignment.AddActivity(phoneActivity, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var absence = new Absence
			{
				Description = new Description("Holiday", "HO"),
				DisplayColor = Color.Red
			};
			var personAbsence = new PersonAbsence(User.CurrentUser(), Scenario.Current(), new AbsenceLayer(absence, period));
			ScheduleData.Add(personAbsence);

			var result = Target.FetchDayData(date);

			var layerDetails = result.Schedule.Periods.Single();
			layerDetails.StyleClassName.Should().Be.EqualTo("color_FF0000");
			layerDetails.Summary.Should().Be.EqualTo("9:00");
			layerDetails.Title.Should().Be.EqualTo("Holiday");
			layerDetails.TimeSpan.Should().Be.EqualTo("08:00 - 17:00");
			layerDetails.Meeting.Should().Be.Null();
			layerDetails.Color.Should().Be.EqualTo("255,0,0");
			layerDetails.StartPositionPercentage.Should().Be.EqualTo(0.25M / 9.5M);
			layerDetails.EndPositionPercentage.Should().Be.EqualTo(9.25M / 9.5M);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldCreatePeriodViewModelFromOvertimeLayerOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 15);
			var definitionSet = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("Overtime",
				MultiplicatorType.Overtime);

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var period = new DateTimePeriod(2014, 12, 15, 08, 2014, 12, 15, 17);
			assignment.AddOvertimeActivity(
				new Activity("Phone") { InWorkTime = true, InContractTime = true, DisplayColor = Color.Green },
				period, definitionSet);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date);

			var layerDetails = result.Schedule.Periods.Single();
			layerDetails.StyleClassName.Should().Be.EqualTo("color_008000");
			layerDetails.Summary.Should().Be.EqualTo("9:00");
			layerDetails.Title.Should().Be.EqualTo("Phone");
			layerDetails.TimeSpan.Should().Be.EqualTo("08:00 - 17:00");
			layerDetails.Meeting.Should().Be.Null();
			layerDetails.Color.Should().Be.EqualTo("0,128,0");
			layerDetails.StartPositionPercentage.Should().Be.EqualTo(0.25M / 9.5M);
			layerDetails.EndPositionPercentage.Should().Be.EqualTo(9.25M / 9.5M);
			layerDetails.IsOvertime.Should().Be(true);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldCreatePeriodViewModelFromMeetingLayerOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 15);
			var phoneActivity = new Activity("Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Green
			};
			var meeting = new Meeting(User.CurrentUser(), new[] { new MeetingPerson(User.CurrentUser(), false) }, "subj", "loc",
				"desc", phoneActivity, null);
			meeting.SetScenario(Scenario.Current());
			meeting.StartDate = meeting.EndDate = date;
			meeting.StartTime = TimeSpan.FromHours(16);
			meeting.EndTime = TimeSpan.FromHours(17);
			ScheduleData.Set(meeting.GetPersonMeetings(User.CurrentUser()).OfType<IScheduleData>().ToList());

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var period = new DateTimePeriod(2014, 12, 15, 8, 2014, 12, 15, 17);
			assignment.AddActivity(new Activity("Phone") { InWorkTime = true, InContractTime = true, DisplayColor = Color.Green },
				period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date);

			var layerDetails = result.Schedule.Periods.Last();
			layerDetails.StyleClassName.Should().Be.EqualTo("color_008000");
			layerDetails.Summary.Should().Be.EqualTo("1:00");
			layerDetails.Title.Should().Be.EqualTo("Phone");
			layerDetails.TimeSpan.Should().Be.EqualTo("16:00 - 17:00");
			layerDetails.Meeting.Title.Should().Be.EqualTo("subj");
			layerDetails.Meeting.Location.Should().Be.EqualTo("loc");
			layerDetails.Meeting.Description.Should().Be.EqualTo("desc");
			layerDetails.Color.Should().Be.EqualTo("0,128,0");
			layerDetails.StartPositionPercentage.Should().Be.EqualTo(8.25M / 9.5M);
			layerDetails.EndPositionPercentage.Should().Be.EqualTo(9.25M / 9.5M);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldCreateOvertimeAvailabilityPeriodViewModelOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 15);
			var start = new TimeSpan(12, 0, 0);
			var end = new TimeSpan(13, 0, 0);
			var overtimeAvailability = new OvertimeAvailability(User.CurrentUser(), date, start, end);
			ScheduleData.Add(overtimeAvailability);

			var result = Target.FetchDayData(date).Schedule.Periods.Single() as OvertimeAvailabilityPeriodViewModel;
			result.Title.Should().Be.EqualTo(Resources.OvertimeAvailabilityWeb);
			result.TimeSpan.Should()
				.Equals(TimeHelper.TimeOfDayFromTimeSpan(start, CultureInfo.CurrentCulture) + " - " +
						TimeHelper.TimeOfDayFromTimeSpan(end, CultureInfo.CurrentCulture));
			result.StartPositionPercentage.Should().Be.EqualTo(0.25M / 1.5M);
			result.EndPositionPercentage.Should().Be.EqualTo(1.25M / 1.5M);
			result.IsOvertimeAvailability.Should().Be.True();
			result.Color.Should().Be.EqualTo(Color.Gray.ToCSV());
		}

		[Test]
		public void ShouldCreateOvertimeAvailabilityPeriodViewModelForYesterdayOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 14);
			var start = new TimeSpan(12, 0, 0);
			var end = new TimeSpan(25, 0, 0);
			var overtimeAvailability = new OvertimeAvailability(User.CurrentUser(), date, start, end);
			ScheduleData.Add(overtimeAvailability);

			var result = Target.FetchDayData(date.AddDays(1)).Schedule.Periods.Single() as OvertimeAvailabilityPeriodViewModel;

			result.Title.Should().Be.EqualTo(Resources.OvertimeAvailabilityWeb);
			result.TimeSpan.Should()
				.Equals(TimeHelper.TimeOfDayFromTimeSpan(start, CultureInfo.CurrentCulture) + " - " +
						TimeHelper.TimeOfDayFromTimeSpan(end, CultureInfo.CurrentCulture));
			result.StartPositionPercentage.Should().Be.EqualTo(0);
			result.EndPositionPercentage.Should().Be.EqualTo(1M / 1.25M);
			result.IsOvertimeAvailability.Should().Be.True();
			result.Color.Should().Be.EqualTo(Color.Gray.ToCSV());
		}

		[Test]
		public void ShouldNotCreateOvertimeAvailabilityPeriodViewModelForYesterdayIfNotSpanToTodayOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 14);
			var start = new TimeSpan(12, 0, 0);
			var end = new TimeSpan(13, 0, 0);
			var overtimeAvailability = new OvertimeAvailability(User.CurrentUser(), date, start, end);
			ScheduleData.Add(overtimeAvailability);

			var result = Target.FetchDayData(date.AddDays(-1)).Schedule.Periods;
			result.Should().Be.Empty();
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapDayOfWeekNumberOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 15);
			var result = Target.FetchDayData(date);
			result.Schedule.DayOfWeekNumber.Should().Be.EqualTo((int)date.DayOfWeek);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapNoSpecialStateOnFetchDayData()
		{
			var result = Target.FetchDayData(Now.LocalDateOnly().AddDays(-2));
			result.Schedule.State.Should().Be.EqualTo((SpecialDateState)0);
		}

		[Test, SetCulture("sv-SE"), SetUICulture("sv-SE")]
		public void ShouldMapDayHeaderOnFetchDayData()
		{
			var date = Now.LocalDateOnly().AddDays(-2);
			var result = Target.FetchDayData(date).Schedule;

			result.Header.Date.Should().Be.EqualTo(date.ToShortDateString());
			result.Header.Title.Should().Be.EqualTo("tisdag");
			result.Header.DayDescription.Should().Be.Empty();
			result.Header.DayNumber.Should().Be.EqualTo("16");
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapDayHeaderWithMontNameForFirstDayOfWeekOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 29);
			var result = Target.FetchDayData(date).Schedule;
			result.Header.DayDescription.Should().Be.EqualTo("December");
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapDayHeaderWithMontNameForCurrentOnFetchDayData()
		{
			var result = Target.FetchDayData(new DateOnly(2014, 12, 29)).Schedule;
			result.Header.DayDescription.Should().Be.EqualTo("December");
		}

		[Test]
		public void ShouldMapPublicNoteOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 29);
			ScheduleData.Add(new PublicNote(User.CurrentUser(), date, Scenario.Current(), "TestNote"));

			var result = Target.FetchDayData(date);
			result.Schedule.Note.Message.Should().Be.EqualTo("TestNote");
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapOvertimeAvailabilityOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 29);
			var overtimeAvailability = new OvertimeAvailability(User.CurrentUser(), date, new TimeSpan(1, 1, 1),
				new TimeSpan(1, 2, 2, 2));
			ScheduleData.Add(overtimeAvailability);

			var result = Target.FetchDayData(date).Schedule;
			result.OvertimeAvailabililty.StartTime.Should()
				.Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(overtimeAvailability.StartTime.Value,
					CultureInfo.GetCultureInfo("sv-SE")));
			result.OvertimeAvailabililty.EndTime.Should()
				.Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(overtimeAvailability.EndTime.Value,
					CultureInfo.GetCultureInfo("sv-SE")));
			result.OvertimeAvailabililty.EndTimeNextDay.Should().Be.EqualTo(true);
			result.OvertimeAvailabililty.HasOvertimeAvailability.Should().Be.EqualTo(true);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapOvertimeAvailabilityDefaultValuesForEmptyOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 29);
			ScheduleData.Add(new PersonAssignment(User.CurrentUser(), Scenario.Current(), date));

			var result = Target.FetchDayData(date).Schedule;
			result.OvertimeAvailabililty.DefaultStartTime.Should()
				.Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(new TimeSpan(8, 0, 0), CultureInfo.CurrentCulture));
			result.OvertimeAvailabililty.DefaultEndTime.Should()
				.Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(new TimeSpan(17, 0, 0), CultureInfo.CurrentCulture));
			result.OvertimeAvailabililty.DefaultEndTimeNextDay.Should().Be.EqualTo(false);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapOvertimeAvailabilityDefaultValuesForDayOffOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 29);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			assignment.SetDayOff(new DayOffTemplate(new Description("Day off", "DO")), true);
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date).Schedule;
			result.OvertimeAvailabililty.DefaultStartTime.Should()
				.Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(new TimeSpan(8, 0, 0), CultureInfo.CurrentCulture));
			result.OvertimeAvailabililty.DefaultEndTime.Should()
				.Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(new TimeSpan(17, 0, 0), CultureInfo.CurrentCulture));
			result.OvertimeAvailabililty.DefaultEndTimeNextDay.Should().Be.EqualTo(false);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapOvertimeAvailabilityDefaultValuesIfHasShiftOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 18);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var dateTimePeriod = new DateTimePeriod(new DateTime(2014, 12, 18, 6, 0, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 15, 0, 0, DateTimeKind.Utc));
			assignment.AddActivity(new Activity("a") { InWorkTime = true }, dateTimePeriod);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date).Schedule;
			var timeZone = User.CurrentUser().PermissionInformation.DefaultTimeZone();
			result.OvertimeAvailabililty.DefaultStartTime.Should()
				.Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(dateTimePeriod.TimePeriod(timeZone).EndTime,
					CultureInfo.CurrentCulture));
			result.OvertimeAvailabililty.DefaultEndTime.Should()
				.Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(dateTimePeriod.TimePeriod(timeZone).EndTime.Add(TimeSpan.FromHours(1)),
					CultureInfo.CurrentCulture));
		}

		[Test]
		public void ShouldMapTextRequestCountOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 18);
			var period = new DateTimePeriod(2014, 12, 18, 8, 2014, 12, 18, 17);
			var textRequest = new PersonRequest(User.CurrentUser(), new TextRequest(period));
			PersonRequestRepository.Add(textRequest);
			PersonRequestRepository.Add(textRequest);

			var result = Target.FetchDayData(date).Schedule;
			result.TextRequestCount.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldMapSummaryForDayWithOtherSignificantPartOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 18);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date).Schedule;
			result.Summary.Should().Not.Be.Null();
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapSummaryForDayOffOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 18);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			assignment.SetDayOff(new DayOffTemplate(new Description("Day off", "DO")), true);
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date).Schedule;
			result.Summary.Title.Should().Be.EqualTo("Day off");
			result.Summary.StyleClassName.Should().Contain(StyleClasses.DayOff);
			result.Summary.StyleClassName.Should().Contain(StyleClasses.Striped);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapSummaryForMainShiftOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 18);
			var period = new DateTimePeriod(2014, 12, 18, 7, 2014, 12, 18, 16);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			assignment.AddActivity(new Activity("a") { InWorkTime = true }, period);
			var shiftCategory = new ShiftCategory("sc");
			assignment.SetShiftCategory(shiftCategory);
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date).Schedule;
			result.Summary.TimeSpan.Should()
				.Be.EqualTo(period.TimePeriod(User.CurrentUser().PermissionInformation.DefaultTimeZone()).ToShortTimeString());
			result.Summary.Title.Should().Be.EqualTo(shiftCategory.Description.Name);
			result.Summary.StyleClassName.Should().Be.EqualTo(shiftCategory.DisplayColor.ToStyleClass());
			result.Summary.Summary.Should()
				.Be.EqualTo(TimeHelper.GetLongHourMinuteTimeString(period.ElapsedTime(), CultureInfo.CurrentUICulture));
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldNotMapPersonalActivityToSummaryTimespanOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 18);
			var period = new DateTimePeriod(2014, 12, 18, 7, 2014, 12, 18, 16);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			assignment.AddActivity(new Activity("a") { InWorkTime = true }, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			assignment.AddPersonalActivity(new Activity("b") { InWorkTime = true }, period.MovePeriod(TimeSpan.FromHours(-2)));
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date).Schedule;
			result.Summary.TimeSpan.Should()
				.Be.EqualTo(period.TimePeriod(User.CurrentUser().PermissionInformation.DefaultTimeZone()).ToShortTimeString());
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapSummaryForAbsenceOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 15);
			var timePeriod = new DateTimePeriod(2014, 12, 15, 8, 2014, 12, 15, 17);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phoneActivity = new Activity("Phone")
			{
				InContractTime = true
			};
			assignment.AddActivity(phoneActivity, timePeriod, true);
			ScheduleData.Add(assignment);

			var illnessAbsense = new AbsenceLayer(
				new Absence
				{
					Description = new Description("Illness", "IL"),
					InContractTime = true
				},
				timePeriod);
			var absenceToDisplay = new PersonAbsence(User.CurrentUser(), Scenario.Current(), illnessAbsense);
			absenceToDisplay.Layer.Payload.Priority = 1;
			ScheduleData.Add(absenceToDisplay);

			var holidayAbsence = new AbsenceLayer(
				new Absence
				{
					Description = new Description("Holiday", "HO"),
					InContractTime = true
				},
				timePeriod);
			var highPriority = new PersonAbsence(User.CurrentUser(), Scenario.Current(), holidayAbsence);
			highPriority.Layer.Payload.Priority = 2;
			ScheduleData.Add(highPriority);

			var result = Target.FetchDayData(date).Schedule;
			result.Summary.Title.Should().Be.EqualTo(absenceToDisplay.Layer.Payload.Description.Name);
			result.Summary.StyleClassName.Should().Be.EqualTo(absenceToDisplay.Layer.Payload.DisplayColor.ToStyleClass());
			result.Summary.Summary.Should().Be.EqualTo(TimeHelper.GetLongHourMinuteTimeString(TimeSpan.FromHours(9), CultureInfo.CurrentUICulture));
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapStyleClassForAbsenceOnPersonDayOffOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 15);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			assignment.SetDayOff(new DayOffTemplate(new Description("Day off", "DO")), true);
			ScheduleData.Add(assignment);
			var absence = new PersonAbsence(User.CurrentUser(), Scenario.Current(),
				new AbsenceLayer(new Absence { Description = new Description("Illness", "IL"), InContractTime = true },
					new DateTimePeriod(2014, 12, 15, 8, 2014, 12, 15, 17)));
			ScheduleData.Add(absence);

			var result = Target.FetchDayData(date).Schedule;
			result.Summary.StyleClassName.Should().Contain(StyleClasses.Striped);
		}

		[Test]
		public void ShouldMapTextRequestPermissionOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 18);
			var result = Target.FetchDayData(date);
			result.RequestPermission.TextRequestPermission.Should().Be.True();
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapTimeLineOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 18);
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 8, 45, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 17, 15, 0, DateTimeKind.Utc));
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			assignment.AddActivity(new Activity("a") { InWorkTime = true, DisplayColor = Color.Blue }, period);
			assignment.SetShiftCategory(new ShiftCategory("sc") { DisplayColor = Color.Red });
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date);
			result.TimeLine.Count().Should().Be.EqualTo(11);
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(8);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(30);
			result.TimeLine.First().PositionPercentage.Should().Be.EqualTo(0.0);
			result.TimeLine.ElementAt(1).Time.Hours.Should().Be.EqualTo(9);
			result.TimeLine.ElementAt(1).Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.ElementAt(1).PositionPercentage.Should().Be.EqualTo(0.5 / (17.5 - 8.5));
		}

		[Test]
		public void ShouldMapPermissionsOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 18);
			var result = Target.FetchDayData(date);
			result.AsmPermission.Should().Be.True();
			result.ViewPossibilityPermission.Should().Be.True();
			result.RequestPermission.AbsenceRequestPermission.Should().Be.True();
			result.RequestPermission.OvertimeAvailabilityPermission.Should().Be.True();
			result.RequestPermission.AbsenceReportPermission.Should().Be.True();
			result.RequestPermission.PersonAccountPermission.Should().Be.True();
			result.RequestPermission.ShiftExchangePermission.Should().Be.True();
			result.RequestPermission.ShiftTradeBulletinBoardPermission.Should().Be.True();
			result.RequestPermission.TextRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapSiteOpenHourIntradayTimePeriodOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 18);
			var timePeriod = new TimePeriod(8, 0, 17, 0);
			var team = TeamFactory.CreateTeam("team1", "site1");
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = timePeriod,
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});
			User.CurrentUser().AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(date, team));

			var result = Target.FetchDayData(date);
			result.SiteOpenHourIntradayPeriod.Equals(timePeriod).Should().Be.True();
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapDateFormatForUserOnFetchDayData()
		{
			Culture.IsSwedish();
			var date = new DateOnly(2014, 12, 18);
			var result = Target.FetchDayData(date);
			var expectedFormat = User.CurrentUser().PermissionInformation.Culture().DateTimeFormat.ShortDatePattern;
			result.DatePickerFormat.Should().Be.EqualTo(expectedFormat);
		}

		[Test]
		public void ShouldAdjustTimelineForOverTimeWhenSiteOpenHourPeriodContainsSchedulePeriodOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 18);
			var timePeriod = new TimePeriod(8, 0, 17, 0);
			var team = TeamFactory.CreateTeam("team1", "site1");
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = timePeriod,
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});
			User.CurrentUser().AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(date, team));

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			var activity = new Activity("a") { InWorkTime = true, DisplayColor = Color.Blue };
			assignment.AddActivity(activity, period);
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date, StaffingPossiblityType.Overtime);
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(8);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(17);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotAdjustTimelineForOverTimeWhenSchedulePeriodContainsSiteOpenHourPeriodOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 18);
			var timePeriod = new TimePeriod(8, 0, 17, 0);
			var team = TeamFactory.CreateTeam("team1", "site1");
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = timePeriod,
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});
			User.CurrentUser().AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(date, team));

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 7, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 17, 45, 0, DateTimeKind.Utc));
			assignment.AddActivity(new Activity("a") { InWorkTime = true, DisplayColor = Color.Blue }, period);
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date, StaffingPossiblityType.Overtime);
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(7);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(18);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldNotAdjustTimelineBySiteOpenHourWhenAskForAbsenceOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 18);
			var timePeriod = new TimePeriod(8, 0, 17, 0);
			var team = TeamFactory.CreateTeam("team1", "site1");
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = timePeriod,
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});
			User.CurrentUser().AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(date, team));

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			assignment.AddActivity(new Activity("a") { InWorkTime = true, DisplayColor = Color.Blue }, period);
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date, StaffingPossiblityType.Absence);
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(9);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(10);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotAdjustTimelineForOverTimeWhenNoSiteOpenHourAvailableOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 18);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			assignment.AddActivity(new Activity("a") { InWorkTime = true, DisplayColor = Color.Blue }, period);
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date, StaffingPossiblityType.Overtime);
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(9);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(10);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(0);
		}
		#endregion
	}
}