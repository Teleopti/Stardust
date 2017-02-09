﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class SchedulingCascadingDesktopTest
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		[Test]
		public void ShouldBaseBestShiftOnNonShoveledResourceCalculation()
		{
			const int numberOfAgents = 100;
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var earlyInterval = new TimePeriod(7, 45, 8, 0);
			var lateInterval = new TimePeriod(15, 45, 16, 0);
			var date = new DateOnly(2017, 1, 10);
			var activity = new Activity("_").WithId();
			var scenario = new Scenario("_");
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpen(new TimePeriod(7, 45, 16, 0));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, date, 1);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpen(new TimePeriod(7, 45, 16, 0));
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
			
			Target.Execute(new OptimizerOriginalPreferences(new SchedulingOptions()),
				new NoSchedulingProgress(),
				schedulerStateHolder.Schedules.SchedulesForPeriod(date.ToDateOnlyPeriod(), schedulerStateHolder.SchedulingResultState.PersonsInOrganization.FixedStaffPeople(date.ToDateOnlyPeriod())).ToArray(),
				new OptimizationPreferences(),
				new DaysOffPreferences()
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
		public void ShouldBaseBestShiftOnNonShoveledResourceCalculation_TeamBlock()
		{
			const int numberOfAgents = 100;
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var earlyInterval = new TimePeriod(7, 45, 8, 0);
			var lateInterval = new TimePeriod(15, 45, 16, 0);
			var date = new DateOnly(2017, 1, 10);
			var activity = new Activity("_").WithId();
			var scenario = new Scenario("_");
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpen(new TimePeriod(7, 45, 16, 0));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, date, 1);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpen(new TimePeriod(7, 45, 16, 0));
			var skillDayB = skillB.CreateSkillDayWithDemandOnInterval(scenario, date, 1, new Tuple<TimePeriod, double>(lateInterval, 1000)); //should not shovel resources here when deciding what shift to choose		
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(earlyInterval, TimeSpan.FromMinutes(15)), new TimePeriodWithSegment(lateInterval, TimeSpan.FromMinutes(15)), new ShiftCategory("_").WithId()));
			var agents = Enumerable.Range(0,numberOfAgents).Select(i => new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skillA, skillB).WithSchedulePeriodOneDay(date)).ToArray();

			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), agents, Enumerable.Empty<IPersonAssignment>(), new[] { skillDayA, skillDayB });
			var options = new SchedulingOptions {UseTeam = true, UseBlock = true};

			Target.Execute(new OptimizerOriginalPreferences(options),
				new NoSchedulingProgress(),
				schedulerStateHolder.Schedules.SchedulesForPeriod(date.ToDateOnlyPeriod(), schedulerStateHolder.SchedulingResultState.PersonsInOrganization.FixedStaffPeople(date.ToDateOnlyPeriod())).ToArray(),
				new OptimizationPreferences(),
				new DaysOffPreferences()
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
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var activity = new Activity("_").WithId();
			var scenario = new Scenario("_");
			var skillA = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(1).IsOpenBetween(8, 16);
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, date, 0);
			var skillB = new Skill("B").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().CascadingIndex(2).IsOpenBetween(8, 16);
			var skillDayB = skillB.CreateSkillDayWithDemandOnInterval(scenario, date, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8,0,8,0,15), new TimePeriodWithSegment(16,0,16,0,15), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skillA, skillB).WithSchedulePeriodOneDay(date);
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), new[] { agent}, Enumerable.Empty<IPersonAssignment>(), new[] { skillDayA, skillDayB });

			Target.Execute(new OptimizerOriginalPreferences(new SchedulingOptions()),
				new NoSchedulingProgress(),
				schedulerStateHolder.Schedules.SchedulesForPeriod(date.ToDateOnlyPeriod(), schedulerStateHolder.SchedulingResultState.PersonsInOrganization.FixedStaffPeople(date.ToDateOnlyPeriod())).ToArray(),
				new OptimizationPreferences(),
				new DaysOffPreferences()
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
			var period = new DateOnlyPeriod(date, date.AddWeeks(1));
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
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

			Target.Execute(new OptimizerOriginalPreferences(new SchedulingOptions()),
				new NoSchedulingProgress(),
				stateHolder.Schedules.SchedulesForPeriod(period, agent).ToArray(),
				new OptimizationPreferences(),
				new DaysOffPreferences()
				);

			var asses = stateHolder.Schedules[agent].ScheduledDayCollection(period).Select(x => x.PersonAssignment()).Where(x => x.DayOff()==null);
			asses.Count().Should().Be.EqualTo(5);
			asses.ForEach(x =>
			{
				x.Period.StartDateTime.TimeOfDay
					.Should().Be.EqualTo(TimeSpan.FromHours(0));
			});
		}
	}
}
