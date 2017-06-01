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
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
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
			addSiteOpenHour();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);
			var result = Target.FetchWeekData(null, StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),7,45,17,15); 
		}

		[Test]
		public void ShouldNotAdjustTimelineForOverTimeWhenSchedulePeriodContainsSiteOpenHourPeriod()
		{
			addSiteOpenHour();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 7, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 17, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchWeekData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),7,0,18,0);
		}

		[Test]
		public void ShouldNotAdjustTimelineBySiteOpenHourWhenAskForAbsence()
		{
			addSiteOpenHour();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Absence);

			AssertTimeLine(result.TimeLine.ToList(),9,0,10,0);
		}

		[Test]
		public void ShouldNotAdjustTimelineForOverTimeWhenNoSiteOpenHourAvailable()
		{
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
			   new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchWeekData(null, StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),9,0,10,0);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
		public void ShouldAdjustTimelineAccordingSiteOpenHourInWeek()
		{
			addSiteOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(8, 0, 17, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			}, new SiteOpenHour
			{
				TimePeriod = new TimePeriod(7, 0, 18, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Friday
			});

			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchWeekData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),6,45,18,15);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
		public void ShouldAdjustTimelineAccordingCrossDaySiteOpenHourInWeek()
		{
			addSiteOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(8, 0, 17, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			}, new SiteOpenHour
			{
				TimePeriod = new TimePeriod(7, 0, 18, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Friday
			}, new SiteOpenHour
			{
				TimePeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(25)),
				IsClosed = false,
				WeekDay = DayOfWeek.Saturday
			});

			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchWeekData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 0, 45, 23, 59);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
		public void ShouldAdjustTimelineAccordingCrossWeekSiteOpenHourInWeek()
		{
			addSiteOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(8, 0, 17, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			},new SiteOpenHour
			{
				TimePeriod = new TimePeriod(7, 0, 18, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Friday
			},new SiteOpenHour
			{
				TimePeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(25)),
				IsClosed = false,
				WeekDay = DayOfWeek.Sunday
			});
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchWeekData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),6,45,23,59);
		}

		[Test]
		
		public void ShouldNotAdjustTimelineWithSiteOpenHourWhenCurrentWeekOutOfRange()
		{
			addSiteOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(8, 0, 17, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			}, new SiteOpenHour
			{
				TimePeriod = new TimePeriod(7, 0, 18, 0),
				IsClosed = false,
				WeekDay = DayOfWeek.Friday
			}, new SiteOpenHour
			{
				TimePeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(25)),
				IsClosed = false,
				WeekDay = DayOfWeek.Sunday
			});

			var period = new DateTimePeriod(new DateTime(2015, 1, 5, 9, 0, 0, DateTimeKind.Utc),
				   new DateTime(2015, 1, 5, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period, new DateOnly(2015, 1, 5));

			var result = Target.FetchWeekData(new DateOnly(2015, 1, 5), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),8,45,10,0);
		}

		[Test]
		public void ShouldAdjustTimelineForOverTimeWhenSiteOpenHourPeriodContainsSchedulePeriodOnFetchDayData()
		{
			addSiteOpenHour();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchDayData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),7,45,17,15);
		}

		[Test]
		public void ShouldNotAdjustTimelineForOverTimeWhenSchedulePeriodContainsSiteOpenHourPeriodOnFetchDayData()
		{
			addSiteOpenHour();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 7, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 17, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchDayData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),7,0,18,0);
		}

		[Test]
		public void ShouldNotAdjustTimelineBySiteOpenHourWhenAskForAbsenceOnFetchDayData()
		{
			addSiteOpenHour();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
			   new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchDayData(Now.LocalDateOnly(), StaffingPossiblityType.Absence);

			AssertTimeLine(result.TimeLine.ToList(),9,0,10,0);
		}

		[Test]
		public void ShouldNotAdjustTimelineForOverTimeWhenNoSiteOpenHourAvailableOnFetchDayData()
		{
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchDayData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),9,0,10,0);
		}

		[Test]
		public void ShouldUseDefaultTimelineForDayWithoutSchedule()
		{
			var date = new DateOnly(2014, 12, 18);
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			ScheduleData.Add(assignment);

			var result = Target.FetchDayData(date);

			AssertTimeLine(result.TimeLine.ToList(),8,0,15,0);
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

			AssertTimeLine(result.TimeLine.ToList(),8,0,15,0);
		}

		[Test]
		public void ShouldAdjustTimeLineBySkillOpenHourWhenSiteOpenHourIsNotAvailableDaySchedule()
		{
			var skill = addSkill();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period, skill.Activity);

			var result = Target.FetchDayData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),6,45,18,15);
		}


		[Test]
		public void ShouldNotAdjustTimeLineBySkillOpenHourWhenSchedulePeriodContainsSkillOpenHour()
		{
			var skill = addSkill();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 6, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 18, 45, 0, DateTimeKind.Utc));
			addAssignment(period, skill.Activity);

			var result = Target.FetchDayData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),6,0,19,0);
		}

		[Test]
		
		public void ShouldAdjustTimeLineBySkillOpenHourWhenSiteOpenHourIsNotAvailableWeekSchedule()
		{
			var skill = addSkill();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period, skill.Activity);

			var result = Target.FetchWeekData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),6,45,18,15);
		}

		[Test]
		
		public void ShouldAdjustTimeLineByFullDaySkillOpenHourWhenSiteOpenHourIsNotAvailableWeekSchedule()
		{
			var skill = addSkill(TimeSpan.Zero, TimeSpan.FromDays(1));
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period, skill.Activity);

			var result = Target.FetchWeekData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),0,0,23,59);
		}

		[Test]
		
		public void ShouldAdjustTimeLineByMultipleDaySkillOpenHoursWhenSiteOpenHourIsNotAvailableWeekSchedule()
		{
			var skill1 = addSkill(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			var skill2 = addSkill(TimeSpan.FromHours(8), TimeSpan.FromHours(19));
			var period1 = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2014, 12, 18, 10, 00, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 11, 00, 0, DateTimeKind.Utc));
			addAssignment(null, new activityDto { Period = period1, Activity = skill1.Activity },
				new activityDto { Period = period2, Activity = skill2.Activity });

			var result = Target.FetchWeekData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),6,45,19,15);
		}

		[Test]
		
		public void ShouldNotAdjustTimeLineByNonInBoundPhoneSkillOpenHoursWhenSiteOpenHourIsNotAvailableWeekSchedule()
		{
			var skill = addSkill(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			skill.SkillType.Description = new Description("test");
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchWeekData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),9,0,10,0);
		}

		[Test]
		
		public void ShouldNotAdjustTimeLineBySkillOpenHoursWhenNoSkillAreScheduled()
		{
			addSkill(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchWeekData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),9,0,10,0);
		}

		[Test]
		public void ShouldInflateMinMaxTimeAfterAdjustBySkillOpenHourDaySchedule()
		{
			var skill = addSkill(TimeSpan.FromHours(8), TimeSpan.FromHours(18));
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 11, 0, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 20, 0, 0, DateTimeKind.Utc));
			addAssignment(period, skill.Activity);

			var result = Target.FetchDayData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 7, 45, 20, 15);
		}

		[Test]
		
		public void ShouldInflateMinMaxTimeAfterAdjustBySkillOpenHourWeekSchedule()
		{
			var skill = addSkill(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			var day = DateHelper.GetFirstDateInWeek(Now.UtcDateTime().Date, CultureInfo.CurrentCulture);
			for (var i = 0; i < 7; i++)
			{
				day = day.AddDays(i);
				var period = new DateTimePeriod(day.AddHours(6).AddMinutes(15),
					day.AddHours(9).AddMinutes(45));
				addAssignment(period, skill.Activity);
			}

			var result = Target.FetchWeekData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),6,0,15,15);
		}

		[Test]
		public void ShouldInflateMinMaxTimeAfterAdjustBySiteOpenHourDaySchedule()
		{
			addSiteOpenHour();
			var period = new DateTimePeriod(new DateTime(2014, 12, 18, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 18, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period);

			var result = Target.FetchDayData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),7,45,17,15);
		}


		[Test]
		
		public void ShouldInflateMinMaxTimeAfterAdjustBySiteOpenHourWeekSchedule()
		{
			addSiteOpenHour();
			var day = DateHelper.GetFirstDateInWeek(Now.UtcDateTime().Date, CultureInfo.CurrentCulture);
			for (var i = 0; i < 7; i++)
			{
				day = day.AddDays(i);
				var period = new DateTimePeriod(day.AddHours(6).AddMinutes(15),
					day.AddHours(9).AddMinutes(45));
				addAssignment(period);
			}

			var result = Target.FetchWeekData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),6,0,17,15);
		}

		[Test]
		
		public void ShouldNotAdjustTimeLineBySkillOpenHoursWhenStaffingDataIsNotAvailableForTheDay()
		{
			addSkill(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			var assignmentDate = new DateOnly(2015, 1, 2);
			var period1 = new DateTimePeriod(new DateTime(2015, 1, 2, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2015, 1, 2, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(period1, assignmentDate);

			var result = Target.FetchDayData(assignmentDate, StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(),9,0,10,0);
		}

		[Test]
		
		public void ShouldAdjustTimelineBySiteOpenHourAndSkillOpenHourWeekSchedule()
		{
			var skill = addSkill(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			addSiteOpenHour();

			var day = DateHelper.GetFirstDateInWeek(Now.UtcDateTime().Date, CultureInfo.CurrentCulture);
			for (var i = 0; i < 7; i++)
			{
				day = day.AddDays(i);
				var period = new DateTimePeriod(day.AddHours(8).AddMinutes(15),
					day.AddHours(9).AddMinutes(45));
				addAssignment(new DateOnly(day), new activityDto { Period = period, Activity = skill.Activity });
			}

			var result = Target.FetchWeekData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 6, 45, 17, 15);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
		public void ShouldAdjustTimeLineBySkillOpenHoursOnlyWithDayWhenStaffingDataIsAvailable()
		{
			var skill1 = addSkill(TimeSpan.FromHours(7), TimeSpan.FromHours(15));
			var skill2 = addSkill(TimeSpan.Zero, TimeSpan.FromDays(1));
			var period1 = new DateTimePeriod(new DateTime(2014, 12, 31, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 31, 9, 45, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2015, 1, 1, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2015, 1, 1, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(new DateOnly(2014, 12, 31), new activityDto {Activity = skill1.Activity, Period = period1});
			addAssignment(new DateOnly(2015, 1, 1), new activityDto {Activity = skill2.Activity, Period = period2});

			var result = Target.FetchWeekData(new DateOnly(2014, 12, 31), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 6, 45, 15, 15);
		}

		[Test]
		
		public void ShouldAdjustTimeLineBySkillOpenHoursOnlyWithDayOff()
		{
			addSkill();
			var dayOffAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(User.CurrentUser(), Scenario.Current(), Now.LocalDateOnly(), DayOffFactory.CreateDayOff(new Description("Dayoff")));
			ScheduleData.Add(dayOffAssignment);

			var result = Target.FetchDayData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 6, 45, 18, 15);
		}

		[Test]
		public void ShouldUseDefaultTimeLineForNoScheduledDayWhenYesterdayHasNoNextDayOvertimeAvaibility()
		{
			var period = new DateTimePeriod(new DateTime(2014, 12, 17, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 17, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(new DateOnly(2014, 12, 17), new activityDto { Activity = new Activity(), Period = period });

			IOvertimeAvailability overtimeAvailability =
				new OvertimeAvailability(User.CurrentUser(), Now.LocalDateOnly().AddDays(-1), TimeSpan.FromHours(11), TimeSpan.FromHours(12));
			ScheduleData.Add(overtimeAvailability);
			var result = Target.FetchDayData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 8, 0, 15, 0);
		}

		[Test]
		public void ShouldUseDefaultTimeLineForNoScheduledDayWhenYesterdayHasNextDayOvertimeAvaibilityEndsAtZero()
		{
			var period = new DateTimePeriod(new DateTime(2014, 12, 17, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 17, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(new DateOnly(2014, 12, 17), new activityDto { Activity = new Activity(), Period = period });

			IOvertimeAvailability overtimeAvailability =
				new OvertimeAvailability(User.CurrentUser(), Now.LocalDateOnly().AddDays(-1), TimeSpan.FromHours(23), TimeSpan.FromDays(1));
			ScheduleData.Add(overtimeAvailability);
			var result = Target.FetchDayData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			AssertTimeLine(result.TimeLine.ToList(), 8, 0, 15, 0);
		}

		[Test]
		public void ShouldGetCorrectPercentageForNoScheduledDayWhenYesterdayHasNextDayOvertimeAvaibilityEndsInFirstHour()
		{
			var period = new DateTimePeriod(new DateTime(2014, 12, 17, 9, 15, 0, DateTimeKind.Utc),
				new DateTime(2014, 12, 17, 9, 45, 0, DateTimeKind.Utc));
			addAssignment(new DateOnly(2014, 12, 17), new activityDto { Activity = new Activity(), Period = period });

			IOvertimeAvailability overtimeAvailability =
				new OvertimeAvailability(User.CurrentUser(), Now.LocalDateOnly().AddDays(-1), TimeSpan.FromHours(23),
					TimeSpan.FromDays(1).Add(TimeSpan.FromMinutes(30)));
			ScheduleData.Add(overtimeAvailability);
			var result = Target.FetchDayData(Now.LocalDateOnly(), StaffingPossiblityType.Overtime);

			var timelines = result.TimeLine.ToList();
			AssertTimeLine(timelines, 0, 0, 1, 0);
			timelines.Count.Should().Be(2);
			timelines[0].PositionPercentage.Should().Be(0);
			timelines[1].PositionPercentage.Should().Be(1);
			result.Schedule.Periods.ElementAt(0).StartPositionPercentage.Should().Be(0);
			result.Schedule.Periods.ElementAt(0).EndPositionPercentage.Should().Be(0.5);
		}

		private void AssertTimeLine(IList<TimeLineViewModel> timeLine, int startHour, int startMinute, int endHour, int endMinute)
		{
			timeLine.First().Time.Hours.Should().Be.EqualTo(startHour);
			timeLine.First().Time.Minutes.Should().Be.EqualTo(startMinute);
			timeLine.Last().Time.Hours.Should().Be.EqualTo(endHour);
			timeLine.Last().Time.Minutes.Should().Be.EqualTo(endMinute); 
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
			var assignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), belongsToDate ?? Now.LocalDateOnly());
			foreach (var activityDto in activityDtos)
			{
				assignment.AddActivity(activityDto.Activity, activityDto.Period);
			}
			ScheduleData.Add(assignment);
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

		private void addSiteOpenHour(params SiteOpenHour[] siteOpenHours)
		{
			var personPeriod = getOrAddPersonPeriod();
			var team = personPeriod.Team;
			foreach (var siteOpenHour in siteOpenHours)
			{
				team.Site.AddOpenHour(siteOpenHour);
			}
		}

	}
}
