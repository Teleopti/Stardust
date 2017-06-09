using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.WorkflowControl;
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
	[SetCulture("sv-SE")]
	public class ScheduleApiControllerFetchDayDataTest
	{
		public ScheduleApiController Target;
		public ICurrentScenario Scenario;
		public ILoggedOnUser User;
		public FakeScheduleDataReadScheduleStorage ScheduleData;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakePushMessageDialogueRepository PushMessageDialogueRepository;

		[Test, SetCulture("sv-SE")]
		public void ShouldGetUnReadMessageCountOnFetchDayData()
		{
			PushMessageDialogueRepository.Add(new PushMessageDialogue(new PushMessage(), User.CurrentUser()));
			var result = Target.FetchDayData(null);
			result.UnReadMessageCount.Should().Be.GreaterThan(0);
		}

		[Test]
		public void ShouldMapScheduleForTodayWithoutParameter()
		{
			Now.Is(new DateTime(2017, 5, 7, 20, 0, 0, DateTimeKind.Utc));
			TimeZone.IsSweden();
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
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(CultureInfo.CurrentCulture);

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
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(CultureInfo.CurrentCulture);
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
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(CultureInfo.CurrentCulture);

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
			TimeZone.IsSweden();

			Now.Is("2015-10-25 10:00");
			var date = new DateOnly(Now.UtcDateTime());

			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(CultureInfo.CurrentCulture);
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
			TimeZone.IsHawaii();

			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(CultureInfo.CurrentCulture);

			var date = new DateOnly(2015, 03, 29);
			var viewModel = Target.FetchDayData(date);
			viewModel.BaseUtcOffsetInMinutes.Should().Be(-10 * 60);
		}

		[Test]
		public void ShouldMapDaylightSavingTimeAdjustmentOnFetchDayData()
		{
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());

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
			TimeZone.IsChina();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());

			var date = new DateOnly(2015, 03, 29);
			var viewModel = Target.FetchDayData(date);
			Assert.IsNull(viewModel.DaylightSavingTimeAdjustment);
		}

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
		public void ShouldMapDayOfWeekNumberOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 15);
			var result = Target.FetchDayData(date);
			result.Schedule.DayOfWeekNumber.Should().Be.EqualTo((int)date.DayOfWeek);
		}

		[Test]
		public void ShouldMapNoSpecialStateOnFetchDayData()
		{
			var result = Target.FetchDayData(Now.ServerDate_DontUse().AddDays(-2));
			result.Schedule.State.Should().Be.EqualTo((SpecialDateState)0);
		}

		[Test, SetUICulture("sv-SE")]
		public void ShouldMapDayHeaderOnFetchDayData()
		{
			var date = Now.ServerDate_DontUse().AddDays(-2);
			var result = Target.FetchDayData(date).Schedule;

			result.Header.Date.Should().Be.EqualTo(date.ToShortDateString());
			result.Header.Title.Should().Be.EqualTo("tisdag");
			result.Header.DayDescription.Should().Be.Empty();
			result.Header.DayNumber.Should().Be.EqualTo("16");
		}

		[Test]
		public void ShouldMapDayHeaderWithMontNameForFirstDayOfWeekOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 29);
			var result = Target.FetchDayData(date).Schedule;
			result.Header.DayDescription.Should().Be.EqualTo("December");
		}

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
		public void ShouldMapSummaryForDayOffOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 18);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			assignment.SetDayOff(new DayOffTemplate(new Description("Day off", "DO")), true);
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date);
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(8);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(15);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(0);

			var schedule = result.Schedule;
			schedule.Summary.Title.Should().Be.EqualTo("Day off");
			schedule.Summary.StyleClassName.Should().Contain(StyleClasses.DayOff);
			schedule.Summary.StyleClassName.Should().Contain(StyleClasses.Striped);
		}

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
		public void ShouldMapTimeLineOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 18);
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 8, 45, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 17, 15, 0, DateTimeKind.Utc));
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.ServerDate_DontUse());
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

			var result = Target.FetchDayData(date, StaffingPossiblityType.Overtime).Schedule;
			result.OpenHourPeriod.Equals(timePeriod).Should().Be.True();
		}

		[Test]
		public void ShouldMapDateFormatForUserOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 18);
			var result = Target.FetchDayData(date);
			var expectedFormat = User.CurrentUser().PermissionInformation.Culture().DateTimeFormat.ShortDatePattern;
			result.DatePickerFormat.Should().Be.EqualTo(expectedFormat);
		}

		[Test]
		public void ShouldHasShiftTradeRequestPermission()
		{
			var result = Target.FetchDayData(null);
			result.RequestPermission.ShiftTradeRequestPermission.Should().Be(true);
		}

		[Test]
		public void ShouldMapShiftTradeRequestSetting()
		{
			var workflowControlSet = new WorkflowControlSet { ShiftTradeOpenPeriodDaysForward = new MinMax<int>(1, 99) };
			User.CurrentUser().WorkflowControlSet = workflowControlSet;
			var localDateOnly = Now.ServerDate_DontUse();

			var shiftTradeRequestSetting = Target.FetchDayData(null).ShiftTradeRequestSetting;
			shiftTradeRequestSetting.Should().Not.Be(null);

			shiftTradeRequestSetting.NowDay.Should().Be(localDateOnly.Date.Day);
			shiftTradeRequestSetting.NowMonth.Should().Be(localDateOnly.Date.Month);
			shiftTradeRequestSetting.NowYear.Should().Be(localDateOnly.Date.Year);

			shiftTradeRequestSetting.OpenPeriodRelativeStart.Should().Be(1);
			shiftTradeRequestSetting.OpenPeriodRelativeEnd.Should().Be(99);
		}

	    [Test]
	    public void ShouldMapHasNotScheduledAsTrueWhenNoScheduled()
	    {
			var date = new DateOnly(2014, 12, 18);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date).Schedule;
			result.HasNotScheduled.Should().Be.True();
		}

		[Test]
	    public void ShouldMapHasNotScheduledAsFalseWhenHasScheduled()
	    {
			var date = new DateOnly(2017, 05, 22);
			var timePeriod = new DateTimePeriod(2017, 05, 22, 8, 2017, 05, 22, 17);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phoneActivity = new Activity("Phone")
			{
				InContractTime = true
			};
			assignment.AddActivity(phoneActivity, timePeriod, true);
			assignment.SetShiftCategory(new ShiftCategory("sc") { DisplayColor = Color.Red });
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date).Schedule;
			result.HasNotScheduled.Should().Be.False();
		}

		[Test]
		public void ShouldReturnTrueForCheckStaffingByIntradayWhenOnlyIntradayAbsencePeriodIsAvailable()
		{
			var intradayAbsenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(Now.ServerDate_DontUse(), Now.ServerDate_DontUse().AddDays(2)),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.ServerDate_DontUse(), Now.ServerDate_DontUse().AddDays(2)),
				StaffingThresholdValidator = new StaffingThresholdValidator()
			};
			var budgetGroupAbsenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(Now.ServerDate_DontUse(), Now.ServerDate_DontUse().AddDays(1)),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.ServerDate_DontUse(), Now.ServerDate_DontUse().AddDays(1)),
				StaffingThresholdValidator = new BudgetGroupHeadCountValidator()
			};
			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AddOpenAbsenceRequestPeriod(intradayAbsenceRequestOpenDatePeriod);
			workFlowControlSet.AddOpenAbsenceRequestPeriod(budgetGroupAbsenceRequestOpenDatePeriod);
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = Target.FetchDayData(Now.ServerDate_DontUse().AddDays(2));
			result.CheckStaffingByIntraday.Should().Be(true);
		}

		[Test]
		public void ShouldReturnTrueForCheckStaffingByIntradayWhenOnlyIntradayAbsencePeriodWithStaffingCheckOn()
		{
			var intradayAbsenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(Now.ServerDate_DontUse(), Now.ServerDate_DontUse().AddDays(2)),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.ServerDate_DontUse(), Now.ServerDate_DontUse().AddDays(2)),
				StaffingThresholdValidator = new StaffingThresholdValidator()
			};
			var budgetGroupAbsenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(Now.ServerDate_DontUse(), Now.ServerDate_DontUse().AddDays(2)),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.ServerDate_DontUse(), Now.ServerDate_DontUse().AddDays(2)),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator()
			};
			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AddOpenAbsenceRequestPeriod(intradayAbsenceRequestOpenDatePeriod);
			workFlowControlSet.AddOpenAbsenceRequestPeriod(budgetGroupAbsenceRequestOpenDatePeriod);
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = Target.FetchDayData(Now.ServerDate_DontUse());
			result.CheckStaffingByIntraday.Should().Be(true);
		}
	}
}
