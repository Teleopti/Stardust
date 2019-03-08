using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common;
using Teleopti.Ccc.WebTest.Core.IoC;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture(true)]
	[TestFixture(false)]
	[MyTimeWebTest]
	[SetCulture("sv-SE")]
	public class ScheduleApiControllerDayTimeLineTest : IIsolateSystem, IConfigureToggleManager
	{
		public ScheduleApiController Target;
		public ICurrentScenario Scenario;
		public ILoggedOnUser User;
		public MutableNow Now;
		public IPushMessageDialogueRepository PushMessageDialogueRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeOvertimeAvailabilityRepository OvertimeAvailabilityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeUserTimeZone UserTimeZone;

		private readonly ISkillType skillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId();

		public void Isolate(IIsolate isolate)
		{
			var person = PersonFactory.CreatePersonWithId();
			var skill = new Skill("test1").WithId();
			skill.SkillType = skillType;
			
			var personPeriod = PersonPeriodFactory.CreatePersonPeriodWithSkills(new DateOnly(2014, 1, 1), skill);
			personPeriod.Team = TeamFactory.CreateTeam("team1", "site1");
			person.AddPersonPeriod(personPeriod);
			var workflowControlSet = new WorkflowControlSet("test");
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { skillType })
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

		private readonly Action<FakeToggleManager> _configure;

		public ScheduleApiControllerDayTimeLineTest(bool optimizedEnabled)
		{
			_configure = t => t.Set(Toggles.WFM_ProbabilityView_ImproveResponseTime_80040, optimizedEnabled);
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			_configure.Invoke(toggleManager);
		}

		[Test]
		public void ShouldMapOvernightTimeLineOnFetchDayData()
		{
			var date = new DateOnly(2014, 12, 18);
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 18, 45, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 19, 2, 15, 0, DateTimeKind.Utc));

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);

			assignment.AddActivity(new Activity("a") { InWorkTime = true, DisplayColor = Color.Blue }, period);
			assignment.SetShiftCategory(new ShiftCategory("sc") { DisplayColor = Color.Red });
			PersonAssignmentRepository.Add(assignment);

			var result = Target.FetchDayData(date);
			result.TimeLine.Count().Should().Be.EqualTo(10);
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(18);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(30);
			result.TimeLine.First().PositionPercentage.Should().Be.EqualTo(0.0);
			result.TimeLine.ElementAt(1).Time.Hours.Should().Be.EqualTo(19);
			result.TimeLine.ElementAt(1).Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.ElementAt(1).PositionPercentage.Should().Be.EqualTo(0.5 / (26.5 - 18.5));
		}

		[Test]
		public void ShouldAdjustTimelineAccordingOpenPeriodSkillTypeOpenHourDaySchedule()
		{
			var skillType = new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId();
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { skillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			User.CurrentUser().WorkflowControlSet = workflowControlSet;

			var skill1 = addSkill(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			var skill2 = addSkill(TimeSpan.Zero, TimeSpan.FromDays(1));
			skill2.SkillType = skillType;

			var period1 = new DateTimePeriod(new DateTime(2014, 12, 31, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 31, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(new DateOnly(2014, 12, 31), new activityDto { Activity = skill1.Activity, Period = period1 });

			var result = Target.FetchDayData(new DateOnly(2014, 12, 31), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 0, 0, 23, 59);
		}

		[Test]
		public void ShouldAdjustTimelineForDayScheduleWithMultipleSkillTypesMatched()
		{
			var phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId();
			var emailSkillType = new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId();
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13))),
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13))),
				OrderIndex = 2
			});
			User.CurrentUser().WorkflowControlSet = workflowControlSet;

			var skill1 = addSkill(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			var skill2 = addSkill(TimeSpan.Zero, TimeSpan.FromDays(1));
			skill1.SkillType = phoneSkillType;
			skill2.SkillType = emailSkillType;

			var period1 = new DateTimePeriod(new DateTime(2014, 12, 31, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 31, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(new DateOnly(2014, 12, 31), new activityDto { Activity = skill1.Activity, Period = period1 });

			var result = Target.FetchDayData(new DateOnly(2014, 12, 31), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 0, 0, 23, 59);
		}

		[Test]
		public void ShouldAdjustTimelineForDayScheduleWithNotDenySkillType()
		{
			var phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId();
			var emailSkillType = new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId();
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13))),
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13))),
				OrderIndex = 2
			});
			User.CurrentUser().WorkflowControlSet = workflowControlSet;
			
			var skill1 = addSkill(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			var skill2 = addSkill(TimeSpan.Zero, TimeSpan.FromDays(1));
			skill1.SkillType = phoneSkillType;
			skill2.SkillType = emailSkillType;

			var period1 = new DateTimePeriod(new DateTime(2014, 12, 31, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 31, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(new DateOnly(2014, 12, 31), new activityDto { Activity = skill1.Activity, Period = period1 });

			var result = Target.FetchDayData(new DateOnly(2014, 12, 31), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 6, 45, 15, 15);
		}

		[Test]
		public void ShouldAdjustTimelineForOverTimeWhenSiteOpenHourPeriodContainsSchedulePeriodOnFetchDayData()
		{
			addSiteOpenHour();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchDayData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 7, 45, 17, 15);
		}

		[Test]
		public void ShouldNotAdjustTimelineForOverTimeWhenSchedulePeriodContainsSiteOpenHourPeriodOnFetchDayData()
		{
			addSiteOpenHour();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 7, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 17, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchDayData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 7, 0, 18, 0);
		}

		[Test]
		public void ShouldNotAdjustTimelineBySiteOpenHourWhenAskForAbsenceOnFetchDayData()
		{
			addSiteOpenHour();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
			   new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchDayData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Absence);

			AssertTimeLine(result.TimeLine.ToList(), 9, 0, 10, 0);
		}

		[Test]
		public void ShouldNotAdjustTimelineForOverTimeWhenNoSiteOpenHourAvailableOnFetchDayData()
		{
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchDayData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 9, 0, 10, 0);
		}

		[Test]
		public void ShouldRemoveOneHourTimelineOnEnteringDSTDay()
		{
			var timeZone = TimeZoneInfoFactory.CentralStandardTime();
			UserTimeZone.Is(timeZone);
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(timeZone);
			Now.Is(new DateTime(2018, 03, 11, 6, 0, 0, DateTimeKind.Utc));

			addSiteOpenHour();
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 11, 01, 15, 0), timeZone),
				TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 11, 03, 45, 0), timeZone));
			addAssignment(period);

			var result = Target.FetchDayData(null, StaffingPossibilityType.Absence);

			AssertTimeLine(result.TimeLine.ToList(), 1, 0, 4, 0);
			result.TimeLine.Count().Should().Be(3);
			result.TimeLine.ElementAt(0).Time.Should().Be(TimeSpan.FromHours(1));
			result.TimeLine.ElementAt(1).Time.Should().Be(TimeSpan.FromHours(3));
			result.TimeLine.ElementAt(2).Time.Should().Be(TimeSpan.FromHours(4));
		}

		[Test]
		public void ShouldCalculatePercentageCorrectlyOnEnteringDSTDay()
		{
			var timeZone = TimeZoneInfoFactory.CentralStandardTime();
			UserTimeZone.Is(timeZone);
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(timeZone);
			Now.Is(new DateTime(2018, 03, 11, 6, 0, 0, DateTimeKind.Utc));

			addSiteOpenHour();
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 11, 01, 15, 0), timeZone),
				TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 11, 03, 45, 0), timeZone));
			addAssignment(period);

			var result = Target.FetchDayData(null, StaffingPossibilityType.Absence);

			AssertTimeLine(result.TimeLine.ToList(), 1, 0, 4, 0);
			result.TimeLine.Count().Should().Be(3);
			result.TimeLine.ElementAt(0).Time.Should().Be(TimeSpan.FromHours(1));
			result.TimeLine.ElementAt(0).PositionPercentage.Should().Be(0);
			result.TimeLine.ElementAt(1).Time.Should().Be(TimeSpan.FromHours(3));
			result.TimeLine.ElementAt(1).PositionPercentage.Should().Be(1 / (decimal)2);
			result.TimeLine.ElementAt(2).Time.Should().Be(TimeSpan.FromHours(4));
			result.TimeLine.ElementAt(2).PositionPercentage.Should().Be(1);
		}

		[Test]
		public void ShouldCalculatePercentageCorrectlyAfterDSTStarted()
		{
			var timeZone = TimeZoneInfoFactory.CentralStandardTime();
			UserTimeZone.Is(timeZone);
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(timeZone);
			Now.Is(new DateTime(2018, 03, 11, 6, 0, 0, DateTimeKind.Utc));

			addSiteOpenHour();
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 11, 06, 00, 0), timeZone),
				TimeZoneHelper.ConvertToUtc(new DateTime(2018, 03, 11, 08, 00, 0), timeZone));
			addAssignment(period);

			var result = Target.FetchDayData(null, StaffingPossibilityType.Absence);

			result.TimeLine.Count().Should().Be(5);
			result.TimeLine.ElementAt(0).Time.Should().Be(TimeSpan.FromHours(5).Add(TimeSpan.FromMinutes(45)));
			result.TimeLine.ElementAt(0).PositionPercentage.Should().Be(0);
			result.TimeLine.ElementAt(1).Time.Should().Be(TimeSpan.FromHours(6));
			result.TimeLine.ElementAt(1).PositionPercentage.Should().Be(0.25M / 2.5M);
			result.TimeLine.ElementAt(2).Time.Should().Be(TimeSpan.FromHours(7));
			result.TimeLine.ElementAt(2).PositionPercentage.Should().Be(1.25M / 2.5M);
			result.TimeLine.ElementAt(3).Time.Should().Be(TimeSpan.FromHours(8));
			result.TimeLine.ElementAt(3).PositionPercentage.Should().Be(2.25M / 2.5M);
			result.TimeLine.ElementAt(4).Time.Should().Be(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(15)));
			result.TimeLine.ElementAt(4).PositionPercentage.Should().Be(1);
		}

		[Test]
		public void ShouldUseDefaultTimelineForDayWithoutSchedule()
		{
			var date = new DateOnly(2014, 12, 18);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			PersonAssignmentRepository.Add(assignment);

			var result = Target.FetchDayData(date);

			AssertTimeLine(result.TimeLine.ToList(), 8, 0, 17, 0);
		}

		[Test]
		public void ShouldGetUnreadMessageCount()
		{
			PushMessageDialogueRepository.Add(new PushMessageDialogue(new PushMessage(), User.CurrentUser()));
			var result = Target.GetUnreadMessageCount();

			result.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldUseDefaultTimelineForDayWithoutScheduleAndOvertimeYesterdayInvisible()
		{
			var dateOnly = new DateOnly(2014, 12, 18);
			var period = new DateTimePeriod(new DateTime(2014, 12, 17, 22, 0, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 17, 23, 59, 0, DateTimeKind.Utc));

			var activity = new Activity("test activity") { InWorkTime = true, DisplayColor = Color.Blue };
			var multiplicatorDefinicationSet = new MultiplicatorDefinitionSet("aa", MultiplicatorType.Overtime);

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), dateOnly);
			assignment.AddOvertimeActivity(activity, period, multiplicatorDefinicationSet, false);
			PersonAssignmentRepository.Add(assignment);

			var result = Target.FetchDayData(dateOnly, StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 8, 0, 17, 0);
		}

		[Test]
		public void ShouldAdjustTimeLineBySkillOpenHourWhenSiteOpenHourIsNotAvailableDaySchedule()
		{
			var skill = addSkill();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period, skill.Activity);

			var result = Target.FetchDayData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 6, 45, 18, 15);
		}

		[Test]
		public void ShouldAdjustTimeLineStartTimeToZeroWhenSkillOpenHourIsCrossDay()
		{
			var skill = addSkill();
			skill.TimeZone = TimeZoneInfoFactory.MountainTimeZoneInfo();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period, skill.Activity);

			var result = Target.FetchDayData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 0, 0, 23, 59);
		}

		[Test]
		public void ShouldNotAdjustTimeLineBySkillOpenHourWhenSchedulePeriodContainsSkillOpenHour()
		{
			var skill = addSkill();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 6, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 18, 45, 0, DateTimeKind.Utc));
			addAssignment(period, skill.Activity);

			var result = Target.FetchDayData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 6, 0, 19, 0);
		}

		[Test]
		public void ShouldInflateMinMaxTimeAfterAdjustBySiteOpenHourDaySchedule()
		{
			addSiteOpenHour();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchDayData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 7, 45, 17, 15);
		}

		[Test]
		public void ShouldNotAdjustTimeLineBySkillOpenHoursWhenStaffingDataIsNotAvailableForTheDay()
		{
			addSkill(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			var assignmentDate = new DateOnly(2015, 1, 2);
			var period1 = new DateTimePeriod(new DateTime(2015, 1, 2, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2015, 1, 2, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period1, assignmentDate);

			var result = Target.FetchDayData(assignmentDate, StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 9, 0, 10, 0);
		}

		[Test]
		public void ShouldMapTimeLineEdgesOnFetchDayData()
		{
			var date = new DateOnly(2015, 03, 29);
			var timePeriod = new DateTimePeriod("2015-03-29 08:00".Utc(), "2015-03-29 17:00".Utc());
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, timePeriod);
			PersonAssignmentRepository.Has(personAssignment);

			var viewModel = Target.FetchDayData(date);
			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("07:45");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("17:15");
		}

		[Test]
		public void ShouldMapTimeLineWhenScheduleStaredFromZeroOnFetchDayData()
		{
			var date = new DateOnly(2015, 03, 29);
			var timePeriod = new DateTimePeriod("2015-03-29 00:00".Utc(), "2015-03-29 17:00".Utc());
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, timePeriod);
			PersonAssignmentRepository.Has(personAssignment);

			var viewModel = Target.FetchDayData(date);
			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("00:00");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("17:15");
		}

		[Test]
		public void ShouldMapTimeLineWhenScheduleEndedAtZeroOnFetchDayData()
		{
			var date = new DateOnly(2015, 03, 29);
			var timePeriod = new DateTimePeriod("2015-03-29 17:00".Utc(), "2015-03-30 00:00".Utc());
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, timePeriod);
			PersonAssignmentRepository.Has(personAssignment);

			var viewModel = Target.FetchDayData(date);
			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("16:45");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("00:00");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnDayBeforeDstOnFetchDayData()
		{
			UserTimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(UserTimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(CultureInfo.CurrentCulture);

			var date = new DateOnly(2015, 03, 28);
			var timePeriod = new DateTimePeriod("2015-03-28 07:45".Utc(), "2015-03-28 17:00".Utc());
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, timePeriod);
			PersonAssignmentRepository.Has(personAssignment);

			var viewModel = Target.FetchDayData(date);
			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("08:30");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("18:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnFirstDstDayOnFetchDayData()
		{
			UserTimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(UserTimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(CultureInfo.CurrentCulture);
			Now.Is("2015-03-29 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, new DateTimePeriod("2015-03-29 07:45".Utc(), "2015-03-29 17:00".Utc()));
			PersonAssignmentRepository.Has(personAssignment);

			var viewModel = Target.FetchDayData(date);
			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("09:30");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("19:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnFirstDstDayAndNightShiftOnFetchDayData()
		{
			Now.Is("2015-03-29 10:00");
			UserTimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(UserTimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(CultureInfo.CurrentCulture);

			var date = new DateOnly(2015, 03, 29);
			var phone = new Activity("p");
			var personAssignment1 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 3, 28));
			personAssignment1.AddActivity(phone, new DateTimePeriod("2015-03-28 00:00".Utc(), "2015-03-28 04:00".Utc()));
			var personAssignment2 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 3, 29));
			personAssignment2.AddActivity(phone, new DateTimePeriod("2015-03-29 00:00".Utc(), "2015-03-29 04:00".Utc()));
			PersonAssignmentRepository.Has(personAssignment1);
			PersonAssignmentRepository.Has(personAssignment2);

			var viewModel = Target.FetchDayData(date);
			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("00:45");
			viewModel.TimeLine.ElementAt(1).TimeLineDisplay.Should().Be("01:00");
			viewModel.TimeLine.ElementAt(2).TimeLineDisplay.Should().Be("03:00");
			viewModel.TimeLine.ElementAt(3).TimeLineDisplay.Should().Be("04:00");
			viewModel.TimeLine.ElementAt(4).TimeLineDisplay.Should().Be("05:00");
			viewModel.TimeLine.ElementAt(5).TimeLineDisplay.Should().Be("06:00");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("06:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnEndDstDayAndNightShiftOnFetchDayData()
		{
			Now.Is("2015-10-25 10:00");
			UserTimeZone.IsSweden();

			var date = new DateOnly(Now.UtcDateTime());

			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(UserTimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(CultureInfo.CurrentCulture);
			var phone = new Activity("p");
			var personAssignment1 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 10, 24));
			personAssignment1.AddActivity(phone, new DateTimePeriod("2015-10-24 00:00".Utc(), "2015-10-24 04:00".Utc()));
			var personAssignment2 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 10, 25));
			personAssignment2.AddActivity(phone, new DateTimePeriod("2015-10-25 00:00".Utc(), "2015-10-25 04:00".Utc()));
			PersonAssignmentRepository.Has(personAssignment1);
			PersonAssignmentRepository.Has(personAssignment2);

			var viewModel = Target.FetchDayData(date);
			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("01:45");
			viewModel.TimeLine.ElementAt(1).TimeLineDisplay.Should().Be("02:00");
			viewModel.TimeLine.ElementAt(2).TimeLineDisplay.Should().Be("03:00");
			viewModel.TimeLine.ElementAt(3).TimeLineDisplay.Should().Be("04:00");
			viewModel.TimeLine.ElementAt(4).TimeLineDisplay.Should().Be("05:00");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("05:15");
		}

		[Test]
		public void ShouldAdjustTimeLineBySkillOpenHoursOnlyWithDayOff()
		{
			addSkill();
			var dayOffAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(User.CurrentUser(), Scenario.Current(), new DateOnly(Now.UtcDateTime()), DayOffFactory.CreateDayOff(new Description("Dayoff")));
			PersonAssignmentRepository.Add(dayOffAssignment);

			var result = Target.FetchDayData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 6, 45, 18, 15);
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyWhenThereIsAnOvertimeAvaibility()
		{
			var date = new DateOnly(Now.UtcDateTime());
			IOvertimeAvailability overtimeAvailability =
				new OvertimeAvailability(User.CurrentUser(), date, TimeSpan.FromHours(2), TimeSpan.FromHours(8));
			OvertimeAvailabilityRepository.Add(overtimeAvailability);
			var result = Target.FetchDayData(date, StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 1, 45, 8, 15);
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyWhenThereIsAnOvertimeAvaibilityOverlappingTodayShift()
		{
			var date = new DateOnly(Now.UtcDateTime());
			IOvertimeAvailability overtimeAvailability =
				new OvertimeAvailability(User.CurrentUser(), date, TimeSpan.FromHours(2), TimeSpan.FromHours(8));
			OvertimeAvailabilityRepository.Add(overtimeAvailability);

			var period = new DateTimePeriod(date.Utc().AddHours(7), date.Utc().AddHours(17));
			addAssignment(date, new activityDto { Activity = new Activity(), Period = period });

			var result = Target.FetchDayData(date, StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 1, 45, 17, 15);
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyWhenThereIsALongOvertimeAvaibilityOverlappingTodayShift()
		{
			var date = new DateOnly(Now.UtcDateTime());
			IOvertimeAvailability overtimeAvailability =
				new OvertimeAvailability(User.CurrentUser(), date, TimeSpan.FromHours(2), TimeSpan.FromHours(22));
			OvertimeAvailabilityRepository.Add(overtimeAvailability);

			var period = new DateTimePeriod(date.Utc().AddHours(7), date.Utc().AddHours(17));
			addAssignment(date, new activityDto { Activity = new Activity(), Period = period });

			var result = Target.FetchDayData(date, StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 1, 45, 22, 15);
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyWhenThereIsAnOvernightOvertimeAvaibilityOverlappingTodayShift()
		{
			var timezone = TimeZoneInfoFactory.ChinaTimeZoneInfo();
			UserTimeZone.Is(timezone);
			Now.Is("2018-08-02 02:00:00");
			var date = new DateOnly(2018, 08, 02);

			IOvertimeAvailability overtimeAvailability =
				new OvertimeAvailability(User.CurrentUser(), date.AddDays(-1), TimeSpan.FromHours(18), TimeSpan.FromHours(28));
			OvertimeAvailabilityRepository.Add(overtimeAvailability);

			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(date.Date.AddHours(2), timezone),
				TimeZoneHelper.ConvertToUtc(date.Date.AddHours(9), timezone));
			addAssignment(date, new activityDto {Activity = new Activity(), Period = period});

			var result = Target.FetchDayData(date, StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 1, 45, 9, 15);
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyWhenThereIsAnOvernightOvertimeAvaibilityOverlappingTodayShiftWithSameEndTime()
		{
			var date = new DateOnly(Now.UtcDateTime());
			IOvertimeAvailability overtimeAvailability =
				new OvertimeAvailability(User.CurrentUser(), date.AddDays(-1), TimeSpan.FromHours(18), TimeSpan.FromHours(36));
			OvertimeAvailabilityRepository.Add(overtimeAvailability);

			var period = new DateTimePeriod(date.Utc().AddHours(3), date.Utc().AddHours(12));
			addAssignment(date, new activityDto { Activity = new Activity(), Period = period });

			var result = Target.FetchDayData(date, StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 2, 45, 12, 15);
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyWhenThereIsAnLongOvernightOvertimeAvaibilityOverlappingTodayShift()
		{
			var date = new DateOnly(Now.UtcDateTime());
			IOvertimeAvailability overtimeAvailability =
				new OvertimeAvailability(User.CurrentUser(), date.AddDays(-1), TimeSpan.FromHours(18), TimeSpan.FromHours(37));
			OvertimeAvailabilityRepository.Add(overtimeAvailability);

			var period = new DateTimePeriod(date.Utc().AddHours(3), date.Utc().AddHours(12));
			addAssignment(date, new activityDto { Activity = new Activity(), Period = period });

			var result = Target.FetchDayData(date, StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 2, 45, 13, 15);
		}

		[Test]
		public void ShouldUseDefaultTimeLineForNoScheduledDayWhenYesterdayHasNoNextDayOvertimeAvaibility()
		{
			var period = new DateTimePeriod(new DateTime(2014, 12, 17, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 17, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(new DateOnly(2014, 12, 17), new activityDto { Activity = new Activity(), Period = period });

			IOvertimeAvailability overtimeAvailability =
				new OvertimeAvailability(User.CurrentUser(), new DateOnly(Now.UtcDateTime()).AddDays(-1), TimeSpan.FromHours(11), TimeSpan.FromHours(12));
			OvertimeAvailabilityRepository.Add(overtimeAvailability);
			var result = Target.FetchDayData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 8, 0, 17, 0);
		}

		[Test]
		public void ShouldUseDefaultTimeLineForNoScheduledDayWhenYesterdayHasNextDayOvertimeAvaibilityEndsAtZero()
		{
			var period = new DateTimePeriod(new DateTime(2014, 12, 17, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 17, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(new DateOnly(2014, 12, 17), new activityDto { Activity = new Activity(), Period = period });

			IOvertimeAvailability overtimeAvailability =
				new OvertimeAvailability(User.CurrentUser(), new DateOnly(Now.UtcDateTime()).AddDays(-1), TimeSpan.FromHours(23), TimeSpan.FromDays(1));
			OvertimeAvailabilityRepository.Add(overtimeAvailability);
			var result = Target.FetchDayData(new DateOnly(Now.UtcDateTime()), StaffingPossibilityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 8, 0, 17, 0);
		}

		private void AssertTimeLine(IList<TimeLineViewModel> timeLine, int startHour, int startMinute, int endHour, int endMinute)
		{
			timeLine.First().Time.Hours.Should().Be.EqualTo(startHour);
			timeLine.First().Time.Minutes.Should().Be.EqualTo(startMinute);
			timeLine.Last().Time.Hours.Should().Be.EqualTo(endHour);
			timeLine.Last().Time.Minutes.Should().Be.EqualTo(endMinute);
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

		private ISkill addSkill(TimeSpan openHourStart, TimeSpan openHourEnd)
		{
			var skill = createSkillWithOpenHours(openHourStart, openHourEnd);
			getOrAddPersonPeriod().AddPersonSkill(new PersonSkill(skill, new Percent(1)));
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

		private void addAssignment(DateTimePeriod period, DateOnly? belongsToDate = null)
		{
			var activity = new Activity("a") { InWorkTime = true, DisplayColor = Color.Blue };
			addAssignment(belongsToDate, new activityDto { Period = period, Activity = activity });
		}

		private void addAssignment(DateTimePeriod period, IActivity activity)
		{
			addAssignment(null, new activityDto { Period = period, Activity = activity });
		}

		private class activityDto
		{
			public DateTimePeriod Period { get; set; }
			public IActivity Activity { get; set; }
		}

		private void addAssignment(DateOnly? belongsToDate = null, params activityDto[] activityDtos)
		{
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), belongsToDate ?? new DateOnly(Now.UtcDateTime()));
			foreach (var activityDto in activityDtos)
			{
				assignment.AddActivity(activityDto.Activity, activityDto.Period);
			}
			PersonAssignmentRepository.Add(assignment);
		}

		private void addSiteOpenHour()
		{
			var personPeriod = getOrAddPersonPeriod();
			var timePeriod = new TimePeriod(8, 0, 17, 0);
			var team = personPeriod.Team;
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = timePeriod,
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});
		}
	}
}
