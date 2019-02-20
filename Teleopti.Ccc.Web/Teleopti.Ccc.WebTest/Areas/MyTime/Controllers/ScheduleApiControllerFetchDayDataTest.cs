using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.WebTest.Core.IoC;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	[MyTimeWebTest]
	[SetCulture("sv-SE")]
	public class ScheduleApiControllerFetchDayDataTest : IIsolateSystem
	{
		public ScheduleApiController Target;
		public ICurrentScenario Scenario;
		public FakeLoggedOnUser User;
		public IScheduleStorage ScheduleData;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeMeetingRepository MeetingRepository;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakePushMessageDialogueRepository PushMessageDialogueRepository;
		public ICurrentDataSource CurrentDataSource;
		public FakeSkillTypeRepository SkillTypeRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ThreadPrincipalContext>().For<IThreadPrincipalContext>();
		}

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
		public void ShouldMapEnteringDSTOnDSTDay()
		{
			TimeZone.Is(TimeZoneInfoFactory.CentralStandardTime());
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());

			Now.Is("2018-03-11 6:00");
			var viewModel = Target.FetchDayData(null);
			viewModel.DaylightSavingTimeAdjustment.Should().Not.Be.Null();
			viewModel.DaylightSavingTimeAdjustment.EnteringDST.Should().Be.True();
		}

		[Test]
		public void ShouldNotMapEnteringDSTOnNormalDay()
		{
			TimeZone.Is(TimeZoneInfoFactory.CentralStandardTime());
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());

			Now.Is("2018-03-12 6:00");
			var viewModel = Target.FetchDayData(null);
			viewModel.DaylightSavingTimeAdjustment.Should().Not.Be.Null();
			viewModel.DaylightSavingTimeAdjustment.EnteringDST.Should().Be.False();
		}

		[Test]
		public void ShouldMapLocalDSTStartTimeInMinutesOnDSTDay()
		{
			TimeZone.Is(TimeZoneInfoFactory.CentralStandardTime());
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());

			Now.Is("2018-03-11 6:00");
			var viewModel = Target.FetchDayData(null);
			viewModel.DaylightSavingTimeAdjustment.Should().Not.Be.Null();
			viewModel.DaylightSavingTimeAdjustment.LocalDSTStartTimeInMinutes.Should().Be(180);
		}

		[Test]
		public void ShouldMapLocalDSTStartTimeInMinutesOnDSTDayInSweden()
		{
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());

			Now.Is("2018-03-24 23:00");
			var viewModel = Target.FetchDayData(null);
			viewModel.DaylightSavingTimeAdjustment.Should().Not.Be.Null();
			viewModel.DaylightSavingTimeAdjustment.LocalDSTStartTimeInMinutes.Should().Be(180);
		}

		[Test]
		public void ShouldCalculateCorrectPositionPercentageforActivityLayersOnEnteringDSTDay()
		{
			var timeZone = TimeZoneInfoFactory.CentralStandardTime();
			TimeZone.Is(timeZone);
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(timeZone);
			Now.Is(new DateTime(2018, 03, 11, 6, 0, 0, DateTimeKind.Utc));

			var date = new DateOnly(2018, 03, 11);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 11, 06, 0, 0), timeZone),
				TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 11, 08, 0, 0), timeZone));
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
			layerDetails.StartPositionPercentage.Should().Be.EqualTo(0.25M / 2.5M);
			layerDetails.EndPositionPercentage.Should().Be.EqualTo(2.25M / 2.5M);
		}

		[Test]
		public void ShouldCalculateCorrectPercentageforActivityLayersOnFetchDayData()
		{
			var timeZone = TimeZoneInfoFactory.CentralStandardTime();
			TimeZone.Is(timeZone);
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(timeZone);
			Now.Is(new DateTime(2018, 03, 11, 6, 0, 0, DateTimeKind.Utc));

			var date = new DateOnly(2018, 03, 11);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 11, 01, 0, 0), timeZone),
				TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 11, 4, 0, 0), timeZone));
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
			layerDetails.StartPositionPercentage.Should().Be.EqualTo(0.25M / 2.5M);
			layerDetails.EndPositionPercentage.Should().Be.EqualTo(2.25M / 2.5M);
		}

		[Test]
		public void ShouldCalculateCorrectPercentageForOvernightAbsenceLayersOnFetchDayData()
		{
			var timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			TimeZone.Is(timeZone);
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(timeZone);
			Now.Is(new DateTime(2018, 03, 11, 6, 0, 0, DateTimeKind.Utc));

			var date = new DateOnly(2018, 03, 11);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 11, 20, 0, 0), timeZone),
				TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 12, 4, 0, 0), timeZone));
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
			var absencePeriod = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 12, 1, 0, 0), timeZone),
				TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 12, 4, 0, 0), timeZone));

			var personAbsence = new PersonAbsence(User.CurrentUser(), Scenario.Current(), new AbsenceLayer(absence, absencePeriod));
			ScheduleData.Add(personAbsence);

			var result = Target.FetchDayData(date);
			var activityLayer = result.Schedule.Periods.First();
			activityLayer.StartPositionPercentage.Should().Be.EqualTo(0.25M / 8.5M);
			activityLayer.EndPositionPercentage.Should().Be.EqualTo(5.25M / 8.5M);

			var absenceLayer = result.Schedule.Periods.Last();
			absenceLayer.StartPositionPercentage.Should().Be.EqualTo(5.25M / 8.5M);
			absenceLayer.EndPositionPercentage.Should().Be.EqualTo(8.25M / 8.5M);
		}

		[Test]
		public void ShouldNotShowYesterdayOvernightShiftCauseItBelongsToYesterdayOnFetchDayData()
		{
			var timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			TimeZone.Is(timeZone);
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(timeZone);
			Now.Is(new DateTime(2018, 03, 11, 6, 0, 0, DateTimeKind.Utc));

			var today = new DateOnly(2018, 03, 11);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), today);
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 11, 20, 0, 0), timeZone),
				TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 12, 4, 0, 0), timeZone));
			var phoneActivity = new Activity("Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Green
			};
			assignment.AddActivity(phoneActivity, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));

			ScheduleData.Add(assignment);

			var assignmentYesterday = new PersonAssignment(User.CurrentUser(), Scenario.Current(), today.AddDays(-1));
			var periodYesterday = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 10, 20, 0, 0), timeZone),
				TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 11, 4, 0, 0), timeZone));
			var phoneActivityYesterday = new Activity("Yesterday Overnight Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Yellow
			};
			assignmentYesterday.AddActivity(phoneActivityYesterday, periodYesterday);
			assignmentYesterday.SetShiftCategory(new ShiftCategory("sc"));

			ScheduleData.Add(assignmentYesterday);

			var result = Target.FetchDayData(today);
			var activityLayers = result.Schedule.Periods;

			activityLayers.Count().Should().Be.EqualTo(1);
			activityLayers.First().StartPositionPercentage.Should().Be.EqualTo(0.25M / 8.5M);
			activityLayers.First().EndPositionPercentage.Should().Be.EqualTo(8.25M / 8.5M);
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
				.Be.EqualTo(0.25 / (26.25 - 17.75));
			layerDetails.EndPositionPercentage.Should().Be.EqualTo((26 - 17.75) / (26.25 - 17.75));
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
			MeetingRepository.Has(meeting);

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var period = new DateTimePeriod(2014, 12, 15, 8, 2014, 12, 15, 17);
			assignment.AddActivity(new Activity("Phone") { InWorkTime = true, InContractTime = true, DisplayColor = Color.Green },
				period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			PersonAssignmentRepository.Add(assignment);

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
		public void ShouldHaveEvaluatedPeriods()
		{
			var date = new DateOnly(2014, 12, 15);
			var start = new TimeSpan(12, 0, 0);
			var end = new TimeSpan(13, 0, 0);
			var overtimeAvailability = new OvertimeAvailability(User.CurrentUser(), date, start, end);
			ScheduleData.Add(overtimeAvailability);

			Target.FetchDayData(date).Schedule.Periods.GetType().Should().Be.EqualTo<PeriodViewModel[]>();
		}

		[Test]
		public void ShouldCreateOvernightOvertimeAvailabilityPeriodViewModelCorrectlyOnFetchDayData()
		{
			var timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			TimeZone.Is(timeZone);
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(timeZone);
			Now.Is(new DateTime(2014, 12, 13, 6, 0, 0, DateTimeKind.Utc));

			var date = new DateOnly(2014, 12, 15);
			var start = new TimeSpan(22, 0, 0);
			var end = new TimeSpan(34, 0, 0);
			var overtimeAvailability = new OvertimeAvailability(User.CurrentUser(), date.AddDays(-1), start, end);
			ScheduleData.Add(overtimeAvailability);

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(date.Date.AddHours(3), timeZone),
				TimeZoneHelper.ConvertToUtc(date.Date.AddHours(8), timeZone));
			assignment.AddActivity(new Activity("Phone"), period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date).Schedule.Periods.First(p => p is OvertimeAvailabilityPeriodViewModel) as OvertimeAvailabilityPeriodViewModel;

			result.Title.Should().Be.EqualTo(Resources.OvertimeAvailabilityWeb);
			result.TimeSpan.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(TimeSpan.FromHours(3), CultureInfo.CurrentCulture) + " - " +
											TimeHelper.TimeOfDayFromTimeSpan(TimeSpan.FromHours(10), CultureInfo.CurrentCulture));
			result.StartPositionPercentage.Should().Be.EqualTo(0.25M / 7.5M);
			result.EndPositionPercentage.Should().Be.EqualTo(7.25M / 7.5M);
			result.IsOvertimeAvailability.Should().Be.True();
			result.Color.Should().Be.EqualTo(Color.Gray.ToCSV());
		}

		[Test]
		public void ShouldCreateOvernightOvertimeAvailabilityPeriodViewModelCorrectlyOnFetchDayDataIfOverlappingTodayShift()
		{
			var timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			TimeZone.Is(timeZone);
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(timeZone);
			Now.Is(new DateTime(2014, 12, 13, 6, 0, 0, DateTimeKind.Utc));

			var date = new DateOnly(2014, 12, 15);
			var start = new TimeSpan(22, 0, 0);
			var end = new TimeSpan(34, 0, 0);
			var overtimeAvailability = new OvertimeAvailability(User.CurrentUser(), date.AddDays(-1), start, end);
			ScheduleData.Add(overtimeAvailability);

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(date.Date.AddHours(3), timeZone),
				TimeZoneHelper.ConvertToUtc(date.Date.AddHours(11), timeZone));
			assignment.AddActivity(new Activity("Phone"), period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date).Schedule.Periods.First(p => p is OvertimeAvailabilityPeriodViewModel) as OvertimeAvailabilityPeriodViewModel;

			result.Title.Should().Be.EqualTo(Resources.OvertimeAvailabilityWeb);
			result.TimeSpan.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(TimeSpan.FromHours(3), CultureInfo.CurrentCulture) + " - " +
												TimeHelper.TimeOfDayFromTimeSpan(TimeSpan.FromHours(10), CultureInfo.CurrentCulture));
			result.StartPositionPercentage.Should().Be.EqualTo(0.25M / 8.5M);
			result.EndPositionPercentage.Should().Be.EqualTo(7.25M / 8.5M);
			result.IsOvertimeAvailability.Should().Be.True();
			result.Color.Should().Be.EqualTo(Color.Gray.ToCSV());
		}

		[Test]
		public void ShouldNotCreateOvertimeAvailabilityPeriodViewModelForYesterdayIfNotOverlappingTodayShift()
		{
			var timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			TimeZone.Is(timeZone);
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(timeZone);
			Now.Is(new DateTime(2014, 12, 13, 6, 0, 0, DateTimeKind.Utc));

			var date = new DateOnly(2014, 12, 13);
			var start = new TimeSpan(12, 0, 0);
			var end = new TimeSpan(28, 0, 0);
			var overtimeAvailability = new OvertimeAvailability(User.CurrentUser(), date, start, end);
			ScheduleData.Add(overtimeAvailability);

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date.AddDays(1));
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(new DateTime(2014, 12, 14, 05, 0, 0), timeZone),
				TimeZoneHelper.ConvertToUtc(new DateTime(2014, 12, 14, 13, 0, 0), timeZone));
			assignment.AddActivity(new Activity("Phone"), period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date.AddDays(1)).Schedule.Periods;
			result.Count().Should().Be.EqualTo(1);
			result.Any(p => p is OvertimeAvailabilityPeriodViewModel).Should().Be.False();
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
			var result = Target.FetchDayData(Now.UtcDateTime().ToDateOnly().AddDays(-2));
			result.Schedule.State.Should().Be.EqualTo((SpecialDateState)0);
		}

		[Test, SetUICulture("sv-SE")]
		public void ShouldMapDayHeaderOnFetchDayData()
		{
			var date = Now.UtcDateTime().ToDateOnly().AddDays(-2);
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
			result.RequestsCount.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldMapOvertimeRequestCountOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 18);
			var period = new DateTimePeriod(2014, 12, 18, 8, 2014, 12, 18, 17);
			var overtimeRequest = new PersonRequest(User.CurrentUser(), new OvertimeRequest(new MultiplicatorDefinitionSet("test",MultiplicatorType.Overtime),period));
			PersonRequestRepository.Add(overtimeRequest);
			PersonRequestRepository.Add(overtimeRequest);

			var result = Target.FetchDayData(date).Schedule;
			result.RequestsCount.Should().Be.EqualTo(2);
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
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(17);
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
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.UtcDateTime().ToDateOnly());
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
			var localDateOnly = Now.UtcDateTime().ToDateOnly();

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
		public void ShouldReturnTrueForOvertimeProbabilityEnabledAfterItHasBeenToggledOnAtFatClient()
		{
			initData();
			var workFlowControlSet = new WorkflowControlSet {OvertimeProbabilityEnabled = true};
			workFlowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod()
			{
				BetweenDays = new MinMax<int>(0, 13)
			});
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = Target.FetchDayData(Now.UtcDateTime().ToDateOnly());
			result.OvertimeProbabilityEnabled.Should().Be(true);
		}

		[Test]
		public void ShouldReturnFalseForOvertimeProbabilityEnabledAfterItHasBeenToggledOffAtFatClient()
		{
			var workFlowControlSet = new WorkflowControlSet { OvertimeProbabilityEnabled = false };
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = Target.FetchDayData(Now.UtcDateTime().ToDateOnly());
			result.OvertimeProbabilityEnabled.Should().Be(false);
		}

		[Test]
		public void ShouldReturnFalseForOvertimeProbabilityEnabledWhenOvertimeRequestsAndOvertimeAvailabilityLicensesAreNotAvailable()
		{
			var licenseActivator = LicenseProvider.GetLicenseActivator(new OvertimeFakeLicenseService(false, false));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			var workFlowControlSet = new WorkflowControlSet {OvertimeProbabilityEnabled = true};
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = Target.FetchDayData(Now.UtcDateTime().ToDateOnly());
			result.OvertimeProbabilityEnabled.Should().Be(false);
		}

		[Test]
		public void ShouldReturnTrueForOvertimeProbabilityEnabledWhenOvertimeAvailabilityLicenseIsAvailable()
		{
			initData();
			var licenseActivator = LicenseProvider.GetLicenseActivator(new OvertimeFakeLicenseService(true, false));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			var workFlowControlSet = new WorkflowControlSet { OvertimeProbabilityEnabled = true };
			workFlowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod()
			{
				BetweenDays = new MinMax<int>(0, 13)
			});
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = Target.FetchDayData(Now.UtcDateTime().ToDateOnly());
			result.OvertimeProbabilityEnabled.Should().Be(true);
		}

		[Test]
		public void ShouldReturnTrueForOvertimeProbabilityEnabledWhenOvertimeRequestsLicenseIsAvailable()
		{
			initData();
			var licenseActivator = LicenseProvider.GetLicenseActivator(new OvertimeFakeLicenseService(false, true));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			var workFlowControlSet = new WorkflowControlSet { OvertimeProbabilityEnabled = true };
			workFlowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod()
			{
				BetweenDays = new MinMax<int>(0, 13)
			});
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = Target.FetchDayData(Now.UtcDateTime().ToDateOnly());
			result.OvertimeProbabilityEnabled.Should().Be(true);
		}

		[Test]
		public void ShouldReturnTrueForOvertimeProbabilityEnabledWhenOvertimeAvailabilityAndOvertimeRequestsLicenseAreBothAvailable()
		{
			initData();
			var licenseActivator = LicenseProvider.GetLicenseActivator(new OvertimeFakeLicenseService(true, true));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			var workFlowControlSet = new WorkflowControlSet { OvertimeProbabilityEnabled = true };
			workFlowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod()
			{
				BetweenDays = new MinMax<int>(0, 13)
			});
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = Target.FetchDayData(Now.UtcDateTime().ToDateOnly());
			result.OvertimeProbabilityEnabled.Should().Be(true);
		}

		[Test]
		public void ShouldReturnTrueForAbsenceProbabilityEnabledAfterItHasBeenToggledOnAtFatClient()
		{
			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AbsenceProbabilityEnabled = true;
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = Target.FetchDayData(Now.UtcDateTime().ToDateOnly());
			result.AbsenceProbabilityEnabled.Should().Be(true);
		}

		[Test]
		public void ShouldReturnTrueForCheckStaffingByIntradayWhenOnlyIntradayAbsencePeriodIsAvailable()
		{
			var intradayAbsenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(Now.UtcDateTime().ToDateOnly(), Now.UtcDateTime().ToDateOnly().AddDays(2)),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.UtcDateTime().ToDateOnly(), Now.UtcDateTime().ToDateOnly().AddDays(2)),
				StaffingThresholdValidator = new StaffingThresholdValidator()
			};
			var budgetGroupAbsenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(Now.UtcDateTime().ToDateOnly(), Now.UtcDateTime().ToDateOnly().AddDays(1)),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.UtcDateTime().ToDateOnly(), Now.UtcDateTime().ToDateOnly().AddDays(1)),
				StaffingThresholdValidator = new BudgetGroupHeadCountValidator()
			};
			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AddOpenAbsenceRequestPeriod(intradayAbsenceRequestOpenDatePeriod);
			workFlowControlSet.AddOpenAbsenceRequestPeriod(budgetGroupAbsenceRequestOpenDatePeriod);
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = Target.FetchDayData(Now.UtcDateTime().ToDateOnly().AddDays(2));
			result.CheckStaffingByIntraday.Should().Be(true);
		}

		[Test]
		public void ShouldReturnTrueForCheckStaffingByIntradayWhenOnlyIntradayAbsencePeriodWithStaffingCheckOn()
		{
			var intradayAbsenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(Now.UtcDateTime().ToDateOnly(), Now.UtcDateTime().ToDateOnly().AddDays(2)),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.UtcDateTime().ToDateOnly(), Now.UtcDateTime().ToDateOnly().AddDays(2)),
				StaffingThresholdValidator = new StaffingThresholdValidator()
			};
			var budgetGroupAbsenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(Now.UtcDateTime().ToDateOnly(), Now.UtcDateTime().ToDateOnly().AddDays(2)),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.UtcDateTime().ToDateOnly(), Now.UtcDateTime().ToDateOnly().AddDays(2)),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator()
			};
			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AddOpenAbsenceRequestPeriod(intradayAbsenceRequestOpenDatePeriod);
			workFlowControlSet.AddOpenAbsenceRequestPeriod(budgetGroupAbsenceRequestOpenDatePeriod);
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = Target.FetchDayData(Now.UtcDateTime().ToDateOnly());
			result.CheckStaffingByIntraday.Should().Be(true);
		}

		private void initData()
		{
			var person = User.CurrentUser();
			var skillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony)
				.WithId();
			var skill = new Skill("test1").WithId();
			skill.SkillType = skillType;
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriodWithSkills(new DateOnly(Now.UtcDateTime()), skill));

			SkillTypeRepository.Add(skillType);
		}
	}
}
