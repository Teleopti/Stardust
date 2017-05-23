using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	[MyTimeWebTest]
	[SetCulture("sv-SE")]
	public class ScheduleApiControllerTimeLineTest
	{
		public ScheduleApiController Target;
		public ICurrentScenario Scenario;
		public ILoggedOnUser User;
		public FakeScheduleDataReadScheduleStorage ScheduleData;
		public MutableNow Now;

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
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(7);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(45);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(17);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(15);
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
		[Toggle(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
		public void ShouldAdjustTimelineAccordingSiteOpenHourInWeek()
		{
			var team = TeamFactory.CreateTeam("team1", "site1");
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(8, 0, 17, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(7, 0, 18, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Friday
			});
			User.CurrentUser().AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(Now.LocalDateOnly(), team));

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			assignment.AddActivity(new Activity("a") { InWorkTime = true, DisplayColor = Color.Blue }, period);
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Overtime);
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(6);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(45);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(18);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
		public void ShouldAdjustTimelineAccordingCrossDaySiteOpenHourInWeek()
		{
			var team = TeamFactory.CreateTeam("team1", "site1");
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(8, 0, 17, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(7, 0, 18, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Friday
			});
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(25)),
				IsClosed = false,
				WeekDay = DayOfWeek.Saturday
			});
			User.CurrentUser().AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(Now.LocalDateOnly(), team));

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			assignment.AddActivity(new Activity("a") { InWorkTime = true, DisplayColor = Color.Blue }, period);
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Overtime);
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(0);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(45);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(23);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(59);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
		public void ShouldAdjustTimelineAccordingCrossWeekSiteOpenHourInWeek()
		{
			var team = TeamFactory.CreateTeam("team1", "site1");
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(8, 0, 17, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(7, 0, 18, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Friday
			});
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(25)),
				IsClosed = false,
				WeekDay = DayOfWeek.Sunday
			});
			User.CurrentUser().AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(Now.LocalDateOnly(), team));

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			assignment.AddActivity(new Activity("a") { InWorkTime = true, DisplayColor = Color.Blue }, period);
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Overtime);
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(6);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(45);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(23);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(59);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
		public void ShouldNotAdjustTimelineWithSiteOpenHourWhenCurrentWeekOutOfRange()
		{
			var team = TeamFactory.CreateTeam("team1", "site1");
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(8, 0, 17, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(7, 0, 18, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Friday
			});
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(25)),
				IsClosed = false,
				WeekDay = DayOfWeek.Sunday
			});
			User.CurrentUser().AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(Now.LocalDateOnly(), team));

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015,1,5));
			var period = new DateTimePeriod(new DateTime(2015, 1, 5, 9, 0, 0, DateTimeKind.Utc),
				new DateTime(2015, 1, 5, 9, 45, 0, DateTimeKind.Utc));
			assignment.AddActivity(new Activity("a") { InWorkTime = true, DisplayColor = Color.Blue }, period);
			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(new DateOnly(2015, 1, 5), StaffingPossiblityType.Overtime);
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(8);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(45);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(10);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(0);
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
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(7);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(45);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(17);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(15);
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

		[Test]
		public void ShouldUseDefaultTimelineForDayWithoutSchedule()
		{
			var date = new DateOnly(2014, 12, 18);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date);
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(8);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(15);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(0);
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
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(dateOnly, StaffingPossiblityType.Overtime);
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(8);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(15);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldAdjustTimeLineBySkillOpenHourWhenSiteOpenHourIsNotAvailableDaySchedule()
		{
			var skill = createSkillWithOpenHours(TimeSpan.FromHours(7), TimeSpan.FromHours(18));
			var team = TeamFactory.CreateTeam("team1", "site1");
			var personPeriod = (PersonPeriod)PersonPeriodFactory.CreatePersonPeriod(Now.LocalDateOnly(), team); 
			personPeriod.AddPersonSkill(new PersonSkill(skill,new Percent(1)));
			User.CurrentUser().AddPersonPeriod(personPeriod);

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));

			assignment.AddActivity(skill.Activity, period);
			
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(null, StaffingPossiblityType.Overtime);

			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(6);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(45);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(18);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldNotAdjustTimeLineBySkillOpenHourWhenSchedulePeriodContainsSkillOpenHour()
		{
			var skill = createSkillWithOpenHours(TimeSpan.FromHours(7), TimeSpan.FromHours(18));

			var team = TeamFactory.CreateTeam("team1", "site1");
			var personPeriod = (PersonPeriod)PersonPeriodFactory.CreatePersonPeriod(Now.LocalDateOnly(), team); 
			personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			User.CurrentUser().AddPersonPeriod(personPeriod);

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 6, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 18, 45, 0, DateTimeKind.Utc));
			assignment.AddActivity(skill.Activity, period);
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(null, StaffingPossiblityType.Overtime);

			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(6);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(19);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(0);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
		public void ShouldAdjustTimeLineBySkillOpenHourWhenSiteOpenHourIsNotAvailableWeekSchedule()
		{
			var skill = createSkillWithOpenHours(TimeSpan.FromHours(7), TimeSpan.FromHours(18));
			var team = TeamFactory.CreateTeam("team1", "site1");
			var personPeriod = (PersonPeriod)PersonPeriodFactory.CreatePersonPeriod(Now.LocalDateOnly(), team);
			personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			User.CurrentUser().AddPersonPeriod(personPeriod);

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));

			assignment.AddActivity(skill.Activity, period);

			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Overtime);

			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(6);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(45);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(18);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
		public void ShouldAdjustTimeLineByFullDaySkillOpenHourWhenSiteOpenHourIsNotAvailableWeekSchedule()
		{
			var skill = createSkillWithOpenHours(TimeSpan.Zero, TimeSpan.FromDays(1));
			var team = TeamFactory.CreateTeam("team1", "site1");
			var personPeriod = (PersonPeriod)PersonPeriodFactory.CreatePersonPeriod(Now.LocalDateOnly(), team);
			personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			User.CurrentUser().AddPersonPeriod(personPeriod);

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));

			assignment.AddActivity(skill.Activity, period);

			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Overtime);

			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(0);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.Last().Time.Days.Should().Be.EqualTo(0);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(23);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(59);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
		public void ShouldAdjustTimeLineByMultipleDaySkillOpenHoursWhenSiteOpenHourIsNotAvailableWeekSchedule()
		{
			var skill1 = createSkillWithOpenHours(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			var skill2 = createSkillWithOpenHours(TimeSpan.FromHours(8), TimeSpan.FromHours(19));
			var team = TeamFactory.CreateTeam("team1", "site1");
			var personPeriod = (PersonPeriod)PersonPeriodFactory.CreatePersonPeriod(Now.LocalDateOnly(), team);
			personPeriod.AddPersonSkill(new PersonSkill(skill1, new Percent(1)));
			personPeriod.AddPersonSkill(new PersonSkill(skill2, new Percent(1)));
			User.CurrentUser().AddPersonPeriod(personPeriod);

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period1 = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2014, 12, 18, 10, 00, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 11, 00, 0, DateTimeKind.Utc));
			assignment.AddActivity(skill1.Activity, period1);
			assignment.AddActivity(skill2.Activity, period2);

			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Overtime);

			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(6);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(45);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(19);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
		public void ShouldNotAdjustTimeLineByNonInBoundPhoneSkillOpenHoursWhenSiteOpenHourIsNotAvailableWeekSchedule()
		{
			var skill = createSkillWithOpenHours(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			skill.SkillType.Description = new Description("test");
			var team = TeamFactory.CreateTeam("team1", "site1");
			var personPeriod = (PersonPeriod)PersonPeriodFactory.CreatePersonPeriod(Now.LocalDateOnly(), team);
			personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			User.CurrentUser().AddPersonPeriod(personPeriod);

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period1 = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			assignment.AddActivity(skill.Activity, period1);

			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Overtime);

			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(9);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(10);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(0);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
		public void ShouldNotAdjustTimeLineBySkillOpenHoursWhenNoSkillAreScheduled()
		{
			var skill = createSkillWithOpenHours(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			var team = TeamFactory.CreateTeam("team1", "site1");
			var personPeriod = (PersonPeriod)PersonPeriodFactory.CreatePersonPeriod(Now.LocalDateOnly(), team);
			personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			User.CurrentUser().AddPersonPeriod(personPeriod);

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period1 = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			assignment.AddActivity(new Activity(), period1);

			ScheduleData.Add(assignment);

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Overtime);

			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(9);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(10);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldInflateMinMaxTimeAfterAdjustBySkillOpenHourDaySchedule()
		{
			var skill = createSkillWithOpenHours(TimeSpan.FromHours(8), TimeSpan.FromHours(18));

			var team = TeamFactory.CreateTeam("team1", "site1");
			var personPeriod = (PersonPeriod)PersonPeriodFactory.CreatePersonPeriod(Now.LocalDateOnly(), team);
			personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			User.CurrentUser().AddPersonPeriod(personPeriod);

			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 11, 0, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 20, 0, 0, DateTimeKind.Utc));
			assignment.AddActivity(skill.Activity, period);
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(null, StaffingPossiblityType.Overtime);

			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(7);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(45);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(20);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
		public void ShouldInflateMinMaxTimeAfterAdjustBySkillOpenHourWeekSchedule()
		{
			var skill1 = createSkillWithOpenHours(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			var team = TeamFactory.CreateTeam("team1", "site1");
			var personPeriod = (PersonPeriod)PersonPeriodFactory.CreatePersonPeriod(Now.LocalDateOnly(), team);
			personPeriod.AddPersonSkill(new PersonSkill(skill1, new Percent(1)));
			User.CurrentUser().AddPersonPeriod(personPeriod); 

			var day = DateHelper.GetFirstDateInWeek(Now.UtcDateTime().Date,CultureInfo.CurrentCulture);
			
			for (int i = 0; i < 7; i++)
			{
				var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly()); 
				day = day.AddDays(i);

				var period = new DateTimePeriod(day.AddHours(6).AddMinutes(15),
					day.AddHours(9).AddMinutes(45));

				assignment.AddActivity(skill1.Activity, period);
				ScheduleData.Add(assignment);
			} 

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Overtime);

			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(6);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(15);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldInflateMinMaxTimeAfterAdjustBySiteOpenHourDaySchedule()
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
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(7);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(45);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(17);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
		public void ShouldInflateMinMaxTimeAfterAdjustBySiteOpenHourWeekSchedule()
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

			var day = DateHelper.GetFirstDateInWeek(Now.UtcDateTime().Date, CultureInfo.CurrentCulture);
			var activity = new Activity("a") { InWorkTime = true, DisplayColor = Color.Blue };
			for (int i = 0; i < 7; i++)
			{
				var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly());
				day = day.AddDays(i);

				var period = new DateTimePeriod(day.AddHours(6).AddMinutes(15),
					day.AddHours(9).AddMinutes(45));

				assignment.AddActivity(activity, period);
				ScheduleData.Add(assignment);
			}

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Overtime);

			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(6);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(00);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(17);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(15);
		}


		private static ISkill createSkillWithOpenHours(TimeSpan start, TimeSpan end)
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			skill.Activity.InWorkTime = true;
			skill.Activity.RequiresSkill = true;
			skill.SkillType.Description = new Description("SkillTypeInboundTelephony");

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
			return skill;
		}

	}
}
