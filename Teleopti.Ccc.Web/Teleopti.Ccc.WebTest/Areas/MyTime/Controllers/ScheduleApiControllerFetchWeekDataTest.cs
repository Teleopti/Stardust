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
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.WebTest.Core.IoC;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture(true)]
	[TestFixture(false)]
	[MyTimeWebTest]
	[SetCulture("sv-SE")]
	public class ScheduleApiControllerFetchWeekDataTest : IIsolateSystem, IConfigureToggleManager
	{
		public ScheduleApiController Target;
		public ICurrentScenario Scenario;
		public ILoggedOnUser User;
		public IScheduleStorage ScheduleData;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeMeetingRepository MeetingRepository;
		public FakeSkillTypeRepository SkillTypeRepository;
		public ICurrentDataSource CurrentDataSource;
		public FakeToggleManager ToggleManager;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;

		private readonly ISkillType skillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony)
			.WithId();

		private readonly Action<FakeToggleManager> _configure;

		public ScheduleApiControllerFetchWeekDataTest(bool optimizedEnabled)
		{
			_configure = t => t.Set(Toggles.WFM_ProbabilityView_ImproveResponseTime_80040, optimizedEnabled);
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			_configure.Invoke(toggleManager);
		}

		public void Isolate(IIsolate isolate)
		{
			var person = PersonFactory.CreatePersonWithId();
			var skill = new Skill("test1").WithId();
			skill.SkillType = skillType;
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriodWithSkills(new DateOnly(2014,1,1),skill));
			var workflowControlSet = new WorkflowControlSet("test");
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 13)
			});
			person.WorkflowControlSet = workflowControlSet;

			var skillRepository = new FakeSkillRepository();
			skillRepository.Has(skill);
			isolate.UseTestDouble(skillRepository).For<ISkillRepository>();
			isolate.UseTestDouble(new FakeLoggedOnUser(person)).For<ILoggedOnUser>();
			isolate.UseTestDouble(new FakeSkillTypeRepository(skillType)).For<ISkillTypeRepository>();
		}

		[Test]
		public void ShouldMap()
		{
			Now.Is(new DateTime(2017, 5, 7, 20, 0, 0, DateTimeKind.Utc));
			TimeZone.IsSweden();

			var viewModel = Target.FetchWeekData(null);
			viewModel.Should().Not.Be.Null();
			viewModel.PeriodSelection.Date.Should().Be.EqualTo("2017-05-07");
			viewModel.IsCurrentWeek.Should().Be.True();
		}

		[Test]
		public void ShouldReturnOvertimeProbabilityEnabledTrueWhenItHasBeenToggledOnInFatClient()
		{
			User.CurrentUser().WorkflowControlSet.OvertimeProbabilityEnabled = true;

			var result = Target.FetchWeekData(null);
			result.OvertimeProbabilityEnabled.Should().Be(true);
		}

		[Test]
		public void ShouldReturnOvertimeProbabilityEnabledFalseWhenItHasBeenToggledOffInFatClient()
		{
			User.CurrentUser().WorkflowControlSet.OvertimeProbabilityEnabled = false;
			var result = Target.FetchWeekData(null);
			result.OvertimeProbabilityEnabled.Should().Be(false);
		}

		[Test]
		public void ShouldReturnFalseForOvertimeProbabilityEnabledWhenOvertimeRequestsAndOvertimeAvailabilityLicensesAreNotAvailable()
		{
			var licenseActivator = LicenseProvider.GetLicenseActivator(new OvertimeFakeLicenseService(false, false));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			User.CurrentUser().WorkflowControlSet.OvertimeProbabilityEnabled = true;

			var result = Target.FetchWeekData(null);
			result.OvertimeProbabilityEnabled.Should().Be(false);
		}

		[Test]
		public void ShouldReturnTrueForOvertimeProbabilityEnabledWhenOvertimeAvailabilityLicenseIsAvailable()
		{
			var licenseActivator = LicenseProvider.GetLicenseActivator(new OvertimeFakeLicenseService(true, false));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			User.CurrentUser().WorkflowControlSet.OvertimeProbabilityEnabled = true;

			var result = Target.FetchWeekData(null);
			result.OvertimeProbabilityEnabled.Should().Be(true);
		}

		[Test]
		public void ShouldReturnTrueForOvertimeProbabilityEnabledWhenOvertimeRequestsLicenseIsAvailable()
		{
			var licenseActivator = LicenseProvider.GetLicenseActivator(new OvertimeFakeLicenseService(false, true));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			User.CurrentUser().WorkflowControlSet.OvertimeProbabilityEnabled = true;

			var result = Target.FetchWeekData(null);
			result.OvertimeProbabilityEnabled.Should().Be(true);
		}

		[Test]
		public void ShouldReturnTrueForOvertimeProbabilityEnabledWhenOvertimeAvailabilityAndOvertimeRequestsLicenseAreBothAvailable()
		{
			var licenseActivator = LicenseProvider.GetLicenseActivator(new OvertimeFakeLicenseService(true, true));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			User.CurrentUser().WorkflowControlSet.OvertimeProbabilityEnabled = true;

			var result = Target.FetchWeekData(null);
			result.OvertimeProbabilityEnabled.Should().Be(true);
		}

		[Test]
		public void ShouldReturnFalseForOvertimeProbabilityEnabledWhenTheWholeWeekIsOutsideOpenPeriod()
		{
			User.CurrentUser().WorkflowControlSet.OvertimeProbabilityEnabled = true;

			var result = Target.FetchWeekData(Now.ServerDate_DontUse().AddWeeks(3));
			result.OvertimeProbabilityEnabled.Should().Be(false);
		}

		[Test]
		public void ShouldReturnTrueForOvertimeProbabilityEnabledWhenThePartialWeekIsWithinOpenPeriod()
		{
			var workFlowControlSet = new WorkflowControlSet { OvertimeProbabilityEnabled = true };
			workFlowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				BetweenDays = new MinMax<int>(0, 19)
			});
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = Target.FetchWeekData(Now.ServerDate_DontUse().AddWeeks(3));
			result.OvertimeProbabilityEnabled.Should().Be(true);
		}

		[Test]
		public void ShouldReturnTrueForOvertimeProbabilityEnabledWhenMatchedSkillTypeIsNotDeny()
		{
			var phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId();
			var emailSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId();

			var workFlowControlSet = new WorkflowControlSet { OvertimeProbabilityEnabled = true };
			workFlowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 13),
				OrderIndex = 0
			});
			workFlowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				BetweenDays = new MinMax<int>(0, 13),
				OrderIndex = 1
			});
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var timeZone = User.CurrentUser().PermissionInformation.DefaultTimeZone();
			var emailActivity = createActivity("emailActivity");
			var phoneActivity = createActivity("phoneActivity");

			var criticalUnderStaffedPhoneSkill = createSkill("criticalUnderStaffedPhoneSkill", null, timeZone);
			criticalUnderStaffedPhoneSkill.SkillType = phoneSkillType;

			var mostCriticalUnderStaffedEmailSkill = createSkill("mostCriticalUnderStaffedEmailSkill", null, timeZone);
			mostCriticalUnderStaffedEmailSkill.SkillType = emailSkillType;
			mostCriticalUnderStaffedEmailSkill.DefaultResolution = 60;

			var personEmailSkill = createPersonSkill(emailActivity, mostCriticalUnderStaffedEmailSkill);
			var personPhoneSkill = createPersonSkill(phoneActivity, criticalUnderStaffedPhoneSkill);
			addPersonSkillsToPersonPeriod(personPhoneSkill, personEmailSkill);

			var result = Target.FetchWeekData(null);
			result.OvertimeProbabilityEnabled.Should().Be(true);
		}

		[Test]
		public void ShouldReturnAbsenceProbabilityEnabledTrueWhenItHasBeenToggledOnInFatClient()
		{
			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AbsenceProbabilityEnabled = true;
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = Target.FetchWeekData(null);
			result.AbsenceProbabilityEnabled.Should().Be(true);
		}

		[Test]
		public void ShouldShowAbsenceProbabilityOptionWhen14DaysAreWithinAbsencePeriod()
		{
			var absenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(Now.ServerDate_DontUse(), Now.ServerDate_DontUse().AddDays(13)),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.ServerDate_DontUse(), Now.ServerDate_DontUse().AddDays(13)),
				StaffingThresholdValidator = new StaffingThresholdValidator()
			};
			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenDatePeriod);
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = Target.FetchWeekData(null);
			result.CheckStaffingByIntraday.Should().Be(true);

			result = Target.FetchWeekData(Now.ServerDate_DontUse().AddWeeks(1));
			result.CheckStaffingByIntraday.Should().Be(true);

			result = Target.FetchWeekData(Now.ServerDate_DontUse().AddWeeks(2));
			result.CheckStaffingByIntraday.Should().Be(true);
		}

		[Test]
		public void ShouldShowAbsenceProbabilityOptionWhenSomeDaysWithinAbsencePeriod()
		{
			var absenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(Now.ServerDate_DontUse().AddDays(6), Now.ServerDate_DontUse().AddDays(8)),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.ServerDate_DontUse(), Now.ServerDate_DontUse().AddDays(8)),
				StaffingThresholdValidator = new StaffingThresholdValidator()
			};
			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenDatePeriod);
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = Target.FetchWeekData(Now.ServerDate_DontUse().AddDays(4));
			result.CheckStaffingByIntraday.Should().Be(true);
		}

		[Test]
		public void ShouldNotShowAbsenceProbabilityOptionWhenNoDaysWithinAbsencePeriod()
		{
			var absenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(Now.ServerDate_DontUse().AddDays(6), Now.ServerDate_DontUse().AddDays(8)),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.ServerDate_DontUse().AddDays(6), Now.ServerDate_DontUse().AddDays(8)),
				StaffingThresholdValidator = new StaffingThresholdValidator()
			};
			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenDatePeriod);
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = Target.FetchWeekData(null);
			result.CheckStaffingByIntraday.Should().Be(false);
		}

		[Test]
		public void ShouldMapTimeLineEdges()
		{
			Now.Is("2015-03-29 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, new DateTimePeriod("2015-03-29 08:00".Utc(), "2015-03-29 17:00".Utc()));
			PersonAssignmentRepository.Has(personAssignment);

			var viewModel = Target.FetchWeekData(date);

			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("07:45");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("17:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnDayBeforeDst()
		{
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(CultureInfo.CurrentCulture);
			Now.Is("2015-03-28 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, new DateTimePeriod("2015-03-28 07:45".Utc(), "2015-03-28 17:00".Utc()));
			PersonAssignmentRepository.Has(personAssignment);

			var viewModel = Target.FetchWeekData(date);

			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("08:30");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("18:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnFirstDstDay()
		{
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(CultureInfo.CurrentCulture);
			Now.Is("2015-03-29 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, new DateTimePeriod("2015-03-29 07:45".Utc(), "2015-03-29 17:00".Utc()));
			PersonAssignmentRepository.Has(personAssignment);

			var viewModel = Target.FetchWeekData(date);

			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("09:30");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("19:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnFirstDstDayAndNightShift()
		{
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(CultureInfo.CurrentCulture);
			Now.Is("2015-03-29 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var phone = new Activity("p");
			var personAssignment1 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 3, 28));
			personAssignment1.AddActivity(phone, new DateTimePeriod("2015-03-28 00:00".Utc(), "2015-03-28 04:00".Utc()));
			var personAssignment2 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 3, 29));
			personAssignment2.AddActivity(phone, new DateTimePeriod("2015-03-29 00:00".Utc(), "2015-03-29 04:00".Utc()));
			PersonAssignmentRepository.Has(personAssignment1);
			PersonAssignmentRepository.Has(personAssignment2);

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
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(CultureInfo.CurrentCulture);
			Now.Is("2015-10-25 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var phone = new Activity("p");
			var personAssignment1 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 10, 24));
			personAssignment1.AddActivity(phone, new DateTimePeriod("2015-10-24 00:00".Utc(), "2015-10-24 04:00".Utc()));
			var personAssignment2 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 10, 25));
			personAssignment2.AddActivity(phone, new DateTimePeriod("2015-10-25 00:00".Utc(), "2015-10-25 04:00".Utc()));
			PersonAssignmentRepository.Has(personAssignment1);
			PersonAssignmentRepository.Has(personAssignment2);

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
			TimeZone.IsHawaii();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			Now.Is("2015-03-29 10:00");

			var viewModel = Target.FetchWeekData(null);
			viewModel.BaseUtcOffsetInMinutes.Should().Be(-10 * 60);
		}

		[Test]
		public void ShouldMapDaylightSavingTimeAdjustment()
		{
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			Now.Is("2015-03-29 10:00");

			var viewModel = Target.FetchWeekData(null);

			viewModel.DaylightSavingTimeAdjustment.Should().Not.Be.Null();
			viewModel.DaylightSavingTimeAdjustment.StartDateTime.Should().Be(new DateTime(2015, 3, 29, 1, 0, 0, DateTimeKind.Utc));
			viewModel.DaylightSavingTimeAdjustment.EndDateTime.Should().Be(new DateTime(2015, 10, 25, 2, 0, 0, DateTimeKind.Utc));
			viewModel.DaylightSavingTimeAdjustment.AdjustmentOffsetInMinutes.Should().Be(60);
		}

		[Test]
		public void ShouldCalculateCorrectPercentageforActivityLayersForEnteringDSTDay()
		{
			var timeZone = TimeZoneInfoFactory.CentralStandardTime();
			TimeZone.Is(timeZone);
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(timeZone);
			Now.Is(new DateTime(2018, 03, 11, 6, 0, 0, DateTimeKind.Utc));

			var date = new DateOnly(2018, 03, 11);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var period1 = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 11, 01, 0, 0), timeZone),
				TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 11, 03, 0, 0), timeZone));
			var phoneActivity = new Activity("Phone")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Green
			};
			assignment.AddActivity(phoneActivity, period1);

			var period2 = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 11, 03, 0, 0), timeZone),
				TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 11, 04, 0, 0), timeZone));
			var emailActivity = new Activity("Email")
			{
				InWorkTime = true,
				InContractTime = true,
				DisplayColor = Color.Purple
			};

			assignment.AddActivity(emailActivity, period2);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(date);
			var firstLayerDetails = result.Days.FirstOrDefault(d => d != null && d.Date == date.ToShortDateString()).Periods.First();
			firstLayerDetails.StartPositionPercentage.Should().Be.EqualTo(0.25M / 3.5M);
			firstLayerDetails.EndPositionPercentage.Should().Be.EqualTo(2.25M / 3.5M);

			var secondLayerDetails = result.Days.FirstOrDefault(d => d != null && d.Date == date.ToShortDateString()).Periods.Last();
			secondLayerDetails.StartPositionPercentage.Should().Be.EqualTo(2.25M / 3.5M);
			secondLayerDetails.EndPositionPercentage.Should().Be.EqualTo(3.25M / 3.5M);
		}

		[Test]
		public void ShouldNotMapDaylightSavingTimeAdjustment()
		{
			TimeZone.IsChina();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			Now.Is("2015-03-29 10:00");

			var viewModel = Target.FetchWeekData(null);
			Assert.IsNull(viewModel.DaylightSavingTimeAdjustment);
		}

		[Test]
		public void ShouldValidatePeriodSelectionStartDateAndEndDateFormatCorrectly()
		{
			TimeZone.IsSweden();
			var date = new DateOnly(2015, 07, 06);
			var viewModel = Target.FetchWeekData(date);

			Assert.AreEqual("2015-07-06", viewModel.CurrentWeekStartDate);
			Assert.AreEqual("2015-07-12", viewModel.CurrentWeekEndDate);
		}

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
		public void ShouldCreatePeriodViewModelFromMeetingLayer()
		{
			var meeting = new Meeting(User.CurrentUser(), new[] { new MeetingPerson(User.CurrentUser(), false) }, "subj", "loc",
				"desc",
				new Activity("Phone") { InWorkTime = true, InContractTime = true, DisplayColor = Color.Green }, null);
			meeting.SetScenario(Scenario.Current());
			meeting.StartDate = meeting.EndDate = new DateOnly(2014, 12, 15);
			meeting.StartTime = TimeSpan.FromHours(16);
			meeting.EndTime = TimeSpan.FromHours(17);
			MeetingRepository.Has(meeting);

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2014, 12, 15));
			var period = new DateTimePeriod(2014, 12, 15, 8, 2014, 12, 15, 17);
			assignment.AddActivity(new Activity("Phone") { InWorkTime = true, InContractTime = true, DisplayColor = Color.Green },
				period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			PersonAssignmentRepository.Add(assignment);

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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
		public void ShouldNotCreateOvertimeAvailabilityPeriodViewModelForYesterdayIfNotSpanToToday()
		{
			var start = new TimeSpan(12, 0, 0);
			var end = new TimeSpan(13, 0, 0);
			var overtimeAvailability = new OvertimeAvailability(User.CurrentUser(), new DateOnly(2014, 12, 14), start, end);
			ScheduleData.Add(overtimeAvailability);

			var result = Target.FetchWeekData(null).Days.First().Periods;

			result.Should().Be.Empty();
		}

		[Test]
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

		[Test]
		public void ShouldMapStateToday()
		{
			var result = Target.FetchWeekData(null);

			result.Days.ElementAt(3).State.Should().Be.EqualTo(SpecialDateState.Today);
		}

		[Test]
		public void ShouldMapNoSpecialState()
		{
			var result = Target.FetchWeekData(Now.ServerDate_DontUse().AddDays(-2));

			result.Days.First().State.Should().Be.EqualTo((SpecialDateState)0);
		}

		[Test, SetUICulture("sv-SE")]
		public void ShouldMapDayHeader()
		{
			var date = Now.ServerDate_DontUse().AddDays(-2);
			var result = Target.FetchWeekData(date).Days.ElementAt(1);

			result.Header.Date.Should().Be.EqualTo(date.ToShortDateString());
			result.Header.Title.Should().Be.EqualTo("tisdag");
			result.Header.DayDescription.Should().Be.Empty();
			result.Header.DayNumber.Should().Be.EqualTo("16");
		}

		[Test, SetUICulture("sv-SE")]
		public void ShouldMapDayHeaderWithMontNameForFirstDayOfWeek()
		{
			var date = Now.ServerDate_DontUse().AddDays(-2);
			var result = Target.FetchWeekData(date).Days.First();

			result.Header.DayDescription.Should().Be.EqualTo("december");
		}

		[Test, SetUICulture("sv-SE")]
		public void ShouldMapDayHeaderWithMontNameForFirstDayOfMonth()
		{
			var result = Target.FetchWeekData(new DateOnly(2014, 12, 29)).Days.ElementAt(3);

			result.Header.DayDescription.Should().Be.EqualTo("januari");
		}

		[Test]
		public void ShouldMapPublicNote()
		{
			ScheduleData.Add(new PublicNote(User.CurrentUser(), Now.ServerDate_DontUse(), Scenario.Current(), "TestNote"));

			var result = Target.FetchWeekData(null);

			result.Days.ElementAt(3).Note.Message.Should().Be.EqualTo("TestNote");
		}

		[Test]
		public void ShouldMapOvertimeAvailability()
		{
			var overtimeAvailability = new OvertimeAvailability(User.CurrentUser(), Now.ServerDate_DontUse(), new TimeSpan(1, 1, 1), new TimeSpan(1, 2, 2, 2));
			ScheduleData.Add(overtimeAvailability);

			var result = Target.FetchWeekData(null).Days.ElementAt(3);
			result.OvertimeAvailabililty.StartTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(overtimeAvailability.StartTime.Value, CultureInfo.GetCultureInfo("sv-SE")));
			result.OvertimeAvailabililty.EndTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(overtimeAvailability.EndTime.Value, CultureInfo.GetCultureInfo("sv-SE")));
			result.OvertimeAvailabililty.EndTimeNextDay.Should().Be.EqualTo(true);
			result.OvertimeAvailabililty.HasOvertimeAvailability.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldMapOvertimeAvailabilityDefaultValuesForEmpty()
		{
			ScheduleData.Add(new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.ServerDate_DontUse()));

			var result = Target.FetchWeekData(null).Days.ElementAt(3);
			result.OvertimeAvailabililty.DefaultStartTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(new TimeSpan(8, 0, 0), CultureInfo.CurrentCulture));
			result.OvertimeAvailabililty.DefaultEndTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(new TimeSpan(17, 0, 0), CultureInfo.CurrentCulture));
			result.OvertimeAvailabililty.DefaultEndTimeNextDay.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldMapOvertimeAvailabilityDefaultValuesForDayOff()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.ServerDate_DontUse());
			assignment.SetDayOff(new DayOffTemplate(new Description("Day off", "DO")), true);
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null).Days.ElementAt(3);
			result.OvertimeAvailabililty.DefaultStartTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(new TimeSpan(8, 0, 0), CultureInfo.CurrentCulture));
			result.OvertimeAvailabililty.DefaultEndTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(new TimeSpan(17, 0, 0), CultureInfo.CurrentCulture));
			result.OvertimeAvailabililty.DefaultEndTimeNextDay.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldMapOvertimeAvailabilityDefaultValuesIfHasShift()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.ServerDate_DontUse());
			var dateTimePeriod = new DateTimePeriod(new DateTime(2014, 12, 18, 6, 0, 0, DateTimeKind.Utc), new DateTime(2014, 12, 18, 15, 0, 0, DateTimeKind.Utc));
			assignment.AddActivity(new Activity("a") { InWorkTime = true }, dateTimePeriod);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null).Days.ElementAt(3);
			var timeZone = User.CurrentUser().PermissionInformation.DefaultTimeZone();
			result.OvertimeAvailabililty.DefaultStartTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(dateTimePeriod.TimePeriod(timeZone).EndTime, CultureInfo.CurrentCulture));
			result.OvertimeAvailabililty.DefaultEndTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(dateTimePeriod.TimePeriod(timeZone).EndTime.Add(TimeSpan.FromHours(1)), CultureInfo.CurrentCulture));
		}

		[Test]
		public void ShouldMapTextRequestCount()
		{
			var period = new DateTimePeriod(2014, 12, 18, 8, 2014, 12, 18, 17);
			var textRequest = new PersonRequest(User.CurrentUser(), new TextRequest(period));
			PersonRequestRepository.Add(textRequest);
			PersonRequestRepository.Add(textRequest);

			var result = Target.FetchWeekData(null).Days.ElementAt(3);
			result.RequestsCount.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldMapSummaryForDayWithOtherSignificantPart()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.ServerDate_DontUse());
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null).Days.ElementAt(3);
			result.Summary.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldMapSummaryForDayOff()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.ServerDate_DontUse());
			assignment.SetDayOff(new DayOffTemplate(new Description("Day off", "DO")), true);
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null).Days.ElementAt(3);
			result.Summary.Title.Should().Be.EqualTo("Day off");
			result.Summary.StyleClassName.Should().Contain(StyleClasses.DayOff);
			result.Summary.StyleClassName.Should().Contain(StyleClasses.Striped);
		}

		[Test]
		public void ShouldMapSummaryForMainShift()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.ServerDate_DontUse());
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

		[Test]
		public void ShouldNotMapPersonalActivityToSummaryTimespan()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.ServerDate_DontUse());
			var period = new DateTimePeriod(2014, 12, 18, 7, 2014, 12, 18, 16);
			assignment.AddActivity(new Activity("a") { InWorkTime = true }, period);
			assignment.SetShiftCategory(new ShiftCategory("sc"));
			assignment.AddPersonalActivity(new Activity("b") { InWorkTime = true }, period.MovePeriod(TimeSpan.FromHours(-2)));
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null).Days.ElementAt(3);
			result.Summary.TimeSpan.Should()
				.Be.EqualTo(period.TimePeriod(User.CurrentUser().PermissionInformation.DefaultTimeZone()).ToShortTimeString());
		}

		[Test]
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

		[Test]
		public void ShouldMapStyleClassViewModelsFromScheduleColors()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.ServerDate_DontUse());
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

		[Test]
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

		[Test]
		public void ShouldMapTimeLine()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.ServerDate_DontUse());
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
		public void ShouldHaveCompleteProjectionForShiftStartingYesterdayEndingTomorrow()
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.ServerDate_DontUse());
			var period = new DateTimePeriod(new DateTime(2014, 12, 17, 20, 45, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 19, 2, 15, 0, DateTimeKind.Utc));
			assignment.AddActivity(new Activity("a") { InWorkTime = true, DisplayColor = Color.Blue }, period);
			assignment.SetShiftCategory(new ShiftCategory("sc") { DisplayColor = Color.Red });
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null);
			result.Days.ElementAt(3).Periods.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldMapAsmEnabled()
		{
			var result = Target.FetchWeekData(null);
			result.AsmEnabled.Should().Be.True();
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

			User.CurrentUser().PersonPeriods(Now.ServerDate_DontUse().ToDateOnlyPeriod()).FirstOrDefault().Team = team;

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Overtime).Days.ElementAt(3);
			result.OpenHourPeriod.Equals(timePeriod).Should().Be.True();
		}

		[Test]
		public void ShouldMapDateFormatForUser()
		{
			var result = Target.FetchWeekData(null);
			var expectedFormat = User.CurrentUser().PermissionInformation.Culture().DateTimeFormat.ShortDatePattern;
			result.DatePickerFormat.Should().Be.EqualTo(expectedFormat);
		}

		[Test]
		public void ShouldReportNoNoteWhenNull()
		{
			var result = Target.FetchWeekData(null).Days.First();
			result.HasNote.Should().Be.False();
		}

		[Test]
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
			User.CurrentUser().PersonPeriods(date.ToDateOnlyPeriod()).FirstOrDefault().Team = team;

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Overtime);
			result.Days.ElementAt(3).OpenHourPeriod.Value.StartTime.Should().Be(TimeSpan.FromHours(9));
			result.Days.ElementAt(3).OpenHourPeriod.Value.EndTime.Should().Be(TimeSpan.FromHours(10));
			result.Days.ElementAt(4).OpenHourPeriod.Value.StartTime.Should().Be(TimeSpan.FromHours(10));
			result.Days.ElementAt(4).OpenHourPeriod.Value.EndTime.Should().Be(TimeSpan.FromHours(11));
		}

		[Test]
		public void ShouldGetSkillOpenHourPeriodForDayView()
		{
			var date = new DateOnly(2014, 12, 18);
			var team = TeamFactory.CreateTeam("team1", "site1");
			User.CurrentUser().PersonPeriods(date.ToDateOnlyPeriod()).FirstOrDefault().Team = team;

			var skill = addSkill();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period, skill.Activity);

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Overtime);
			result.Days.ElementAt(3).OpenHourPeriod.Value.StartTime.Should().Be(TimeSpan.FromHours(7));
			result.Days.ElementAt(3).OpenHourPeriod.Value.EndTime.Should().Be(TimeSpan.FromHours(18));
		}

		[Test]
		public void ShouldGetSkillOpenHourPeriodInUsersTimeZoneForDayView()
		{
			var date = new DateOnly(2014, 12, 18);
			var team = TeamFactory.CreateTeam("team1", "site1");
			User.CurrentUser().PersonPeriods(date.ToDateOnlyPeriod()).FirstOrDefault().Team = team;

			var skill = addSkill();
			skill.TimeZone = TimeZoneInfoFactory.MountainTimeZoneInfo();

			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.NewYorkTimeZoneInfo());
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period, skill.Activity);

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Overtime);
			result.Days.ElementAt(3).OpenHourPeriod.Value.StartTime.Should().Be(TimeSpan.FromHours(9));
			result.Days.ElementAt(3).OpenHourPeriod.Value.EndTime.Should().Be(TimeSpan.FromHours(20));
		}

		[Test]
		public void ShouldGetSkillOpenHourPeriodInUsersTimeZoneWhenSkillOpenHourIsCrossDay()
		{
			var date = new DateOnly(2014, 12, 18);
			var team = TeamFactory.CreateTeam("team1", "site1");
			User.CurrentUser().PersonPeriods(date.ToDateOnlyPeriod()).FirstOrDefault().Team = team;

			var skill = addSkill();
			skill.TimeZone = TimeZoneInfoFactory.MountainTimeZoneInfo();

			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Cape Verde Standard Time"));
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period, skill.Activity);

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Overtime);
			result.Days.ElementAt(3).OpenHourPeriod.Value.StartTime.Should().Be(TimeSpan.Zero);
			result.Days.ElementAt(3).OpenHourPeriod.Value.EndTime.Should().Be(TimeSpan.FromHours(24));
		}

		[Test]
		public void ShouldReturnFalseForCheckStaffingByIntradayWhenIntradayAbsencePeriodIsInLowPriority()
		{
			var intradayAbsenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(Now.ServerDate_DontUse(), Now.ServerDate_DontUse().AddDays(13)),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.ServerDate_DontUse(), Now.ServerDate_DontUse().AddDays(13)),
				StaffingThresholdValidator = new StaffingThresholdValidator()
			};
			var budgetGroupAbsenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(Now.ServerDate_DontUse(), Now.ServerDate_DontUse().AddDays(13)),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.ServerDate_DontUse(), Now.ServerDate_DontUse().AddDays(13)),
				StaffingThresholdValidator = new BudgetGroupHeadCountValidator()
			};
			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AddOpenAbsenceRequestPeriod(intradayAbsenceRequestOpenDatePeriod);
			workFlowControlSet.AddOpenAbsenceRequestPeriod(budgetGroupAbsenceRequestOpenDatePeriod);
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = Target.FetchWeekData(null);
			result.CheckStaffingByIntraday.Should().Be(false);
		}

		[Test]
		public void ShouldReturnTrueForCheckStaffingByIntradayWhenAnyIntradayAbsencePeriodIsAvailable()
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

			var result = Target.FetchWeekData(null);
			result.CheckStaffingByIntraday.Should().Be(true);
		}

		private void addAssignment(DateTimePeriod period, IActivity activity)
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.ServerDate_DontUse());

			assignment.AddActivity(activity, period);

			ScheduleData.Add(assignment);
		}

		private ISkill createSkillWithOpenHours(TimeSpan start, TimeSpan end)
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			skill.Activity.InWorkTime = true;
			skill.Activity.RequiresSkill = true;
			skill.SkillType = skillType;

			foreach (var workload in skill.WorkloadCollection)
			{
				foreach (var templateWeek in workload.TemplateWeekCollection)
				{
					templateWeek.Value.ChangeOpenHours(new List<TimePeriod>
					{
						new TimePeriod(start, end)
					});
				}
			}

			SkillRepository.Has(skill);
			return skill;
		}

		private ISkill addSkill()
		{
			var skill = createSkillWithOpenHours(TimeSpan.FromHours(7), TimeSpan.FromHours(18));
			var personPeriod = getOrAddPersonPeriod();
			personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			return skill;
		}

		private PersonPeriod getOrAddPersonPeriod()
		{
			var personPeriod = (PersonPeriod)User.CurrentUser().PersonPeriods(new DateOnly(2014, 1, 1).ToDateOnlyPeriod()).FirstOrDefault();
			if (personPeriod != null) return personPeriod;
			var team = TeamFactory.CreateTeam("team1", "site1");
			personPeriod = (PersonPeriod)PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2014, 1, 1), team);
			User.CurrentUser().AddPersonPeriod(personPeriod);
			return personPeriod;
		}

		private IPersonSkill createPersonSkill(IActivity activity, ISkill skill)
		{
			skill.Activity = activity;
			var personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);
			return personSkill;
		}

		private IActivity createActivity(string name)
		{
			var activity = ActivityFactory.CreateActivity(name);
			activity.RequiresSkill = true;
			activity.InWorkTime = true;
			ActivityRepository.Add(activity);
			return activity;
		}

		private ISkill createSkill(string name, TimePeriod? skillOpenHourPeriod = null, TimeZoneInfo timeZone = null)
		{
			var skill = SkillFactory.CreateSkill(name, timeZone).WithId();
			skill.SkillType.Description = new Description(SkillTypeIdentifier.Phone);
			skill.StaffingThresholds = createStaffingThresholds();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, skillOpenHourPeriod ?? new TimePeriod(8, 00, 21, 00));
			SkillRepository.Has(skill);
			return skill;
		}

		private StaffingThresholds createStaffingThresholds()
		{
			return new StaffingThresholds(new Percent(-0.3), new Percent(-0.1), new Percent(0.1));
		}

		private void addPersonSkillsToPersonPeriod(params IPersonSkill[] personSkills)
		{
			var personPeriod = getOrAddPersonPeriod();
			foreach (var personSkill in personSkills)
			{
				personPeriod.AddPersonSkill(personSkill);
			}
		}
	}
}
