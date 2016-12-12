using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[TestFixture(false)]
	[TestFixture(true)]
	[DomainTest]
	public class DayOffOptimizationDesktopTest : DayOffOptimizationScenario
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public IDayOffOptimizationDesktop Target;

		public DayOffOptimizationDesktopTest(bool teamBlockDayOffForIndividuals) : base(teamBlockDayOffForIndividuals)
		{
		}

		[Test]
		public void ShouldMoveDayOffToDayWithLessDemand()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(1));
			var activity = new Activity("_");
			var skill = new Skill("_", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) {Activity = activity};
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var scenario = new Scenario("_");
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var team = new Team {Site = new Site("_")};
			var personPeriod = new PersonPeriod(firstDay.AddWeeks(-1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team)
			{
				RuleSetBag = new RuleSetBag(ruleSet)
			};
			personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			agent.AddPersonPeriod(personPeriod);
			agent.AddSchedulePeriod(schedulePeriod);

			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				1,
				5,
				5,
				5,
				25,
				5);

			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 7; i++)
			{
				var ass = new PersonAssignment(agent, scenario, firstDay.AddDays(i));
				ass.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
				ass.SetShiftCategory(shiftCategory);
				asses.Add(ass);
				if (i == 5)
				{
					ass.SetDayOff(new DayOffTemplate()); //saturday
				}
			}
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] {agent}, asses, skillDays);
			var optPrefs = new OptimizationPreferences {General = {ScheduleTag = new ScheduleTag()}};

			Target.Execute(period, stateHolder.Schedules.SchedulesForPeriod(period, agent), new NoSchedulingProgress(), optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new GroupPageLight("_", GroupPageType.SingleAgent), () => new WorkShiftFinderResultHolder(),(o, args) => {});

			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(5)).HasDayOff()
				.Should().Be.False();//saturday
			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(1)).HasDayOff()
				.Should().Be.True();//tuesday
		}

		[Test]
		public void ShouldMoveDayOffToDayWithLessDemandAndNotConsiderBrokenMaxSeatOnOtherSite()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(1));
			var activity = new Activity("_") {RequiresSeat = true};
			var skill = new Skill("_", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = activity };
			var skillMaxSeat = new Skill("SkillMaxSeat", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.MaxSeatSkill)) { Activity = activity };
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skillMaxSeat);	
			var scenario = new Scenario("_");
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var ruleSetBag = new RuleSetBag(ruleSet);
			var site = new Site("_");
			var siteMaxSeat = new Site("siteMaxSeat"){MaxSeats = 0, MaxSeatSkill = skillMaxSeat};
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var agentMaxSeat = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var team = new Team { Site = site };
			var teamMaxSeat = new Team { Site = siteMaxSeat };
			var personPeriod = new PersonPeriod(firstDay.AddWeeks(-1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team){RuleSetBag = ruleSetBag};
			var personPeriodMaxSeat = new PersonPeriod(firstDay.AddWeeks(-1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), teamMaxSeat){RuleSetBag = ruleSetBag};
			personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			personPeriodMaxSeat.AddPersonSkill(new PersonSkill(skillMaxSeat, new Percent(1)));
			agent.AddPersonPeriod(personPeriod);
			agentMaxSeat.AddPersonPeriod(personPeriodMaxSeat);
			agent.AddSchedulePeriod(schedulePeriod);
			agentMaxSeat.AddSchedulePeriod(schedulePeriod);

			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				1,
				5,
				5,
				5,
				25,
				5);

			var skillDaysMaxSeat = skillMaxSeat.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				2,
				2,
				2,
				2,
				2,
				2,
				2);


			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < 7; i++)
			{
				var ass = new PersonAssignment(agent, scenario, firstDay.AddDays(i));
				var assMaxSeat = new PersonAssignment(agentMaxSeat, scenario, firstDay.AddDays(i));
				ass.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
				assMaxSeat.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
				ass.SetShiftCategory(shiftCategory);
				assMaxSeat.SetShiftCategory(shiftCategory);
				asses.Add(ass);
				asses.Add(assMaxSeat);
				if (i != 5) continue;
				ass.SetDayOff(new DayOffTemplate()); //saturday
				assMaxSeat.SetDayOff(new DayOffTemplate()); //saturday
			}
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, asses, skillDays.Union(skillDaysMaxSeat));
			var optPrefs = new OptimizationPreferences
			{
				General = {ScheduleTag = new ScheduleTag()},
				Advanced = {UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak}
			};

			Target.Execute(period, stateHolder.Schedules.SchedulesForPeriod(period, agent), new NoSchedulingProgress(), optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new GroupPageLight("_", GroupPageType.SingleAgent), () => new WorkShiftFinderResultHolder(), (o, args) => { });

			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(5)).HasDayOff().Should().Be.False();//saturday
			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(1)).HasDayOff().Should().Be.True();//tuesday
		}
	}
}