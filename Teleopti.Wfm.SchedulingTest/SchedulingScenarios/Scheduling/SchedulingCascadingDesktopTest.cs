using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
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

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class SchedulingCascadingDesktopTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public ResourceCalculateWithNewContext ResourceCalculation;

		[Test]
		public void ShouldBaseBestShiftOnNonShoveledResourceCalculation()
		{
			const int numberOfAgents = 50;
			var earlyInterval = new TimePeriod(7, 45, 8, 0);
			var lateInterval = new TimePeriod(15, 45, 16, 0);
			var date = new DateOnly(2017, 1, 10);
			var activity = new Activity("_").WithId();
			var scenario = new Scenario("_");
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpen(new TimePeriod(7, 45, 16, 0)).DefaultResolution(15);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, date, 1);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpen(new TimePeriod(7, 45, 16, 0)).DefaultResolution(15);
			var skillDayB = skillB.CreateSkillDayWithDemandOnInterval(scenario, date, 1, new Tuple<TimePeriod, double>(lateInterval, 1000)); //should not shovel resources here when deciding what shift to choose		
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(earlyInterval, TimeSpan.FromMinutes(15)), new TimePeriodWithSegment(lateInterval, TimeSpan.FromMinutes(15)), new ShiftCategory("_").WithId()));
			var agents =
				Enumerable.Range(0, numberOfAgents)
					.Select(
						i =>
							new Person().WithId()
								.InTimeZone(TimeZoneInfo.Utc)
								.WithPersonPeriod(ruleSet, skillA, skillB)
								.WithSchedulePeriodOneDay(date))
					.ToArray();
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), agents, Enumerable.Empty<IPersonAssignment>(), new[] { skillDayA, skillDayB });
			
			Target.Execute(new NoSchedulingCallback(), 
				new SchedulingOptions(),
				new NoSchedulingProgress(), 
				schedulerStateHolder.SchedulingResultState.LoadedAgents.FixedStaffPeople(date.ToDateOnlyPeriod()), date.ToDateOnlyPeriod()
				);
		
			var allAssignmentsStartTime = schedulerStateHolder.Schedules.Select(keyValuePair => keyValuePair.Value).
				Select(range => range.ScheduledDay(date).PersonAssignment()).
				Select(x => x.Period.StartDateTime.TimeOfDay);

			allAssignmentsStartTime.Count().Should().Be.EqualTo(numberOfAgents);
			allAssignmentsStartTime.Count(x => x == new TimeSpan(7, 45, 0))
					.Should().Be.EqualTo(numberOfAgents / 2);
			allAssignmentsStartTime.Count(x => x == new TimeSpan(8, 0, 0))
					.Should().Be.EqualTo(numberOfAgents / 2);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldBaseBestShiftOnNonShoveledResourceCalculation_TeamBlock(bool resourceCalculationHasBeenMade)
		{
			const int numberOfAgents = 50;
			var earlyInterval = new TimePeriod(7, 45, 8, 0);
			var lateInterval = new TimePeriod(15, 45, 16, 0);
			var date = new DateOnly(2017, 1, 10);
			var activity = new Activity("_").WithId();
			var scenario = new Scenario("_");
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpen(new TimePeriod(7, 45, 16, 0)).DefaultResolution(15);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, date, 1);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpen(new TimePeriod(7, 45, 16, 0)).DefaultResolution(15);
			var skillDayB = skillB.CreateSkillDayWithDemandOnInterval(scenario, date, 1, new Tuple<TimePeriod, double>(lateInterval, 1000)); //should not shovel resources here when deciding what shift to choose		
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(earlyInterval, TimeSpan.FromMinutes(15)), new TimePeriodWithSegment(lateInterval, TimeSpan.FromMinutes(15)), new ShiftCategory("_").WithId()));
			var agents = Enumerable.Range(0,numberOfAgents).Select(i => new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skillA, skillB).WithSchedulePeriodOneDay(date)).ToArray();
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), agents, Enumerable.Empty<IPersonAssignment>(), new[] { skillDayA, skillDayB });
			var options = new SchedulingOptions {UseTeam = true, UseBlock = true};
			if (resourceCalculationHasBeenMade)
				ResourceCalculation.ResourceCalculate(date.ToDateOnlyPeriod(), new ResourceCalculationData(schedulerStateHolder.SchedulingResultState, false, false));

			Target.Execute(new NoSchedulingCallback(), 
				options,
				new NoSchedulingProgress(),
				schedulerStateHolder.SchedulingResultState.LoadedAgents.FixedStaffPeople(date.ToDateOnlyPeriod()), date.ToDateOnlyPeriod()
				);

			var allAssignmentsStartTime = schedulerStateHolder.Schedules.Select(keyValuePair => keyValuePair.Value).
				Select(range => range.ScheduledDay(date).PersonAssignment()).
				Select(x => x.Period.StartDateTime.TimeOfDay);

			allAssignmentsStartTime.Count().Should().Be.EqualTo(numberOfAgents);
			allAssignmentsStartTime.Count(x => x == new TimeSpan(7, 45, 0))
					.Should().Be.EqualTo(numberOfAgents / 2);
			allAssignmentsStartTime.Count(x => x == new TimeSpan(8, 0, 0))
					.Should().Be.EqualTo(numberOfAgents / 2);
		}

		[Test]
		public void ShouldShovelWhenSchedulingHasBeenDone()
		{
			var date = new DateOnly(2017, 1, 10);
			var activity = new Activity("_").WithId();
			var scenario = new Scenario("_");
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 16);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, date, 0);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 16);
			var skillDayB = skillB.CreateSkillDayWithDemandOnInterval(scenario, date, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8,0,8,0,60), new TimePeriodWithSegment(16,0,16,0,60), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skillA, skillB).WithSchedulePeriodOneDay(date);
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), new[] { agent}, Enumerable.Empty<IPersonAssignment>(), new[] { skillDayA, skillDayB });

			Target.Execute(new NoSchedulingCallback(),
				new SchedulingOptions(),
				new NoSchedulingProgress(),
				schedulerStateHolder.SchedulingResultState.LoadedAgents.FixedStaffPeople(date.ToDateOnlyPeriod()), date.ToDateOnlyPeriod()
				);

			skillDayA.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			skillDayB.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldBaseBestShiftOnPrimarySkillOpenHoursEvenIfSubskillIsOpenWithHigherDemand()
		{
			var date = new DateOnly(2016, 9, 19); //mån
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var activity = new Activity("_").WithId();
			var scenario = new Scenario("_");
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(0, 8);
			var skillDaysA = skillA.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 1, 1, 1, 1, 1, 1, 1); //lower demand
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(16, 24);
			var skillDaysB = skillB.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 2, 2, 2, 2, 2, 2, 2); //higher demand
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(0, 0, 16, 0, 60), new TimePeriodWithSegment(8, 0, 24, 0, 60), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skillA, skillB).WithSchedulePeriodOneWeek(date);
			agent.SchedulePeriod(date).SetDaysOff(2);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, Enumerable.Empty<IScheduleData>(), skillDaysA.Union(skillDaysB));

			Target.Execute(new NoSchedulingCallback(),
				new SchedulingOptions(),
				new NoSchedulingProgress(),
				new[] {agent},
				period
				);

			var asses = stateHolder.Schedules[agent].ScheduledDayCollection(period).Select(x => x.PersonAssignment()).Where(x => x.DayOff()==null);
			asses.Count().Should().Be.EqualTo(5);
			asses.ForEach(x =>
			{
				x.Period.StartDateTime.TimeOfDay
					.Should().Be.EqualTo(TimeSpan.FromHours(0));
			});
		}

		public SchedulingCascadingDesktopTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}
