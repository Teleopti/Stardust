using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class SchedulingTimeZoneTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		//these shouldn't effect scheduling at all, but it does currently.... Replace with "throw stubs" later
		public FakeTimeZoneGuard TimeZoneGuard; 
		public FakeUserTimeZone UserTimeZone;
		//

		[Test]
		public void ShouldBeAbleToScheduleNoMatterTimeZoneSettingsAlsoWhenRequiresSkillIsTrue(
			[Values("Taipei Standard Time", "UTC", "Mountain Standard Time")] string userTimeZone,
			[Values("Taipei Standard Time", "UTC", "Mountain Standard Time")] string userViewPointTimeZone,
			[Values("Taipei Standard Time", "UTC", "Mountain Standard Time")] string agentTimeZone,
			[Values("Taipei Standard Time", "UTC", "Mountain Standard Time")] string skillTimeZone)
		{
			TimeZoneGuard.SetTimeZone(TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
			UserTimeZone.Is(TimeZoneInfo.FindSystemTimeZoneById(userViewPointTimeZone));
			var date = new DateOnly(2017, 9, 7);
			var activity = new Activity{RequiresSkill = true}.WithId();
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(skillTimeZone)).WithId().IsOpen();
			var scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(6, 0, 6, 0, 15), new TimePeriodWithSegment(14, 0, 14, 0, 15), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(agentTimeZone)).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneDay(date);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date.AddDays(-1), 1, 1, 1);
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, date, agent, skillDays);

			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, date.ToDateOnlyPeriod());

			schedulerStateHolder.Schedules[agent].ScheduledDay(date).IsScheduled().Should().Be.True();
		}

		[Test]
		public void ShouldHandleNightShiftsCorrectlyInDifferentTimeZones(
			[Values("Taipei Standard Time", "UTC", "GMT Standard Time", "Mountain Standard Time")] string timeZoneForAll)
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneForAll);
			TimeZoneGuard.TimeZone = timeZone;
			UserTimeZone.Is(timeZone);
			const int numberOfAgents = 10;
			var date = new DateOnly(2015, 10, 12);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).InTimeZone(timeZone).DefaultResolution(60).WithId().IsOpen();
			var scenario = new Scenario();
			var dayRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(10, 0, 10, 0, 15), new TimePeriodWithSegment(18, 0, 18, 0, 15), new ShiftCategory("_").WithId()));
			var nightRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(17, 0, 17, 0, 15), new TimePeriodWithSegment(25, 0, 25, 0, 15), new ShiftCategory("_").WithId()));
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date.AddDays(-1), 10, 10, 10);
			var agents = Enumerable.Repeat(0, numberOfAgents).Select(i =>
				new Person().WithId().InTimeZone(timeZone)
					.WithPersonPeriod(new RuleSetBag(dayRuleSet, nightRuleSet), skill)
					.WithSchedulePeriodOneDay(date)
					.InTimeZone(timeZone)).ToList();
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date.AddDays(1)), agents, skillDays);

			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), agents, date.ToDateOnlyPeriod());

			var assesOnDate = stateHolder.Schedules.SchedulesForDay(date);
			var groupedByStartDateTime = assesOnDate.Select(x => x.PersonAssignment()).GroupBy(x => x.ShiftLayers.Single().Period.StartDateTime);
			//half of agents should get day shift the other half night shift (only two shifts available). 
			const int expected = numberOfAgents / 2;
			Assert.Multiple(() =>
			{
				Assert.That(groupedByStartDateTime.First().Count(), Is.EqualTo(expected));
				Assert.That(groupedByStartDateTime.Last().Count(), Is.EqualTo(expected));
			});
		}

		public SchedulingTimeZoneTest(SeperateWebRequest seperateWebRequest, bool resourcePlannerNoPytteIslands47500) : base(seperateWebRequest, resourcePlannerNoPytteIslands47500)
		{
		}
	}
}