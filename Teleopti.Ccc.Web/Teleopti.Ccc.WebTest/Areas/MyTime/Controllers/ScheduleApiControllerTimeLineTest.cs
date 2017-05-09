﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		public FakeUserTimeZone TimeZone;
		public FakePersonRequestRepository PersonRequestRepository;

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
		[Toggle(Domain.FeatureFlags.Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
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
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(7);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(18);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(0);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
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
				TimePeriod = new TimePeriod(TimeSpan.FromHours(8),TimeSpan.FromHours(25)),
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
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(1);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(23);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(59);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
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
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(7);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.Last().Time.Hours.Should().Be.EqualTo(23);
			result.TimeLine.Last().Time.Minutes.Should().Be.EqualTo(59);
		}
	}
}
