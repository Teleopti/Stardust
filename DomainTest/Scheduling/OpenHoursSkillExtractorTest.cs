using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[DomainTest]
	public class OpenHoursSkillExtractorTest
	{
		public OpenHoursSkillExtractor Target;
		public MatrixListFactory MatrixListFactory;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;

		[Test]
		public void ShouldRestrictOnStartAndEndTime()
		{
			var date = new DateOnly(2018, 10, 1);
			var activity = new Activity { RequiresSkill = true }.WithId();
			var scenario = new Scenario();
			var skill = new Skill().For(activity).WithId().InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 16);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skill).WithSchedulePeriodOneDay(date);
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var scheduleStateHolder = SchedulerStateHolder.Fill(scenario, date.ToDateOnlyPeriod(), new[] { agent }, Enumerable.Empty<IPersonAssignment>(), skillDay);
			var matrixList = MatrixListFactory.CreateMatrixListForSelection(scheduleStateHolder.Schedules, new[] {agent}, date.ToDateOnlyPeriod());

			var result = Target.Extract(new []{agent}, new[] { skillDay }, matrixList.First(), date);

			result.OpenHoursDictionary[date].StartTimeLimitation.StartTime.Should().Be.EqualTo(TimeSpan.FromHours(8));
			result.OpenHoursDictionary[date].EndTimeLimitation.EndTime.Should().Be.EqualTo(TimeSpan.FromHours(16));
			result.ForDate(date).TotalHours.Should().Be.EqualTo(8);
		}

		[TestCase(7, ExpectedResult = true )]
		[TestCase(4, ExpectedResult = false)]
		public bool ShouldRestrictOnStartAndEndTimeWhenPeriodHaveUnscheduledDaysExceptCurrent(int scheduledDays)
		{
			var date = new DateOnly(2018, 10, 1);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var activity = new Activity { RequiresSkill = true }.WithId();
			var scenario = new Scenario();
			var skill = new Skill().For(activity).WithId().InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 16);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skill).WithSchedulePeriodOneWeek(date);
			var asses = new List<IPersonAssignment>();

			for (var i = 1; i < scheduledDays; i++)
			{
				var assignment = new PersonAssignment(agent, scenario, date.AddDays(i)).ShiftCategory(new ShiftCategory().WithId()).WithLayer(activity, new TimePeriod(8, 19));
				asses.Add(assignment);
			}

			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var scheduleStateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, asses, skillDay);
			var matrixList = MatrixListFactory.CreateMatrixListForSelection(scheduleStateHolder.Schedules, new[] { agent }, period);

			var result = Target.Extract(new[] { agent }, new[] { skillDay }, matrixList.First(), date);

			return result == null;
		}
	}
}
