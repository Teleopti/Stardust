using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
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
		private readonly bool _teamBlockDayOffForIndividuals;

		public DayOffOptimizationDesktopTest(bool teamBlockDayOffForIndividuals) : base(teamBlockDayOffForIndividuals)
		{
			_teamBlockDayOffForIndividuals = teamBlockDayOffForIndividuals;
		}

		[Test]
		public void ShouldMoveDayOffToDayWithLessDemand()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(1));
			var activity = new Activity("_");
			var skill = new Skill().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var team = new Team { Site = new Site("_") };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				1,
				5,
				5,
				5,
				25,
				5);
		
			var asses = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
			asses[5].SetDayOff(new DayOffTemplate()); //saturday
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
			var skill = new Skill().For(activity).IsOpen();
			var skillMaxSeat = new Skill("SkillMaxSeat").For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var site = new Site("_");
			var siteMaxSeat = new Site("siteMaxSeat"){MaxSeats = 0, MaxSeatSkill = skillMaxSeat};
			var team = new Team { Site = site };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skill).WithSchedulePeriodOneWeek(firstDay);
			var teamMaxSeat = new Team { Site = siteMaxSeat };
			var agentMaxSeat = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, teamMaxSeat, skillMaxSeat).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(1);
			agentMaxSeat.SchedulePeriod(firstDay).SetDaysOff(1);

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
				var ass = new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16));
				var assMaxSeat = new PersonAssignment(agentMaxSeat, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16));
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


		[TestCase(TeamBlockType.Team)]
		[TestCase(TeamBlockType.Block)]
		[TestCase(TeamBlockType.TeamAndBlock)]
		[TestCase(TeamBlockType.Classic)]
		public void ShouldMoveDayOffToDayWithLessDemand_MarkedBlankDay(TeamBlockType teamBlockType)
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(1));
			var activity = new Activity("_");
			var skill = new Skill().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				1,
				5,
				5,
				5,
				25,
				5);
			var asses = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
			asses[5].SetDayOff(new DayOffTemplate());
			asses[0].ClearMainActivities();//blank day
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, asses, skillDays);
			IExtraPreferences extra = null;
			switch (teamBlockType)
			{
				case TeamBlockType.Classic:
					extra = new ExtraPreferences { UseTeams = false, UseTeamBlockOption = false };
					break;
				case TeamBlockType.Block:
					extra = new ExtraPreferences { UseTeams = false, UseTeamBlockOption = true };
					break;
				case TeamBlockType.Team:
					extra = new ExtraPreferences { UseTeams = true, UseTeamBlockOption = false };
					break;
				case TeamBlockType.TeamAndBlock:
					extra = new ExtraPreferences { UseTeams = true, UseTeamBlockOption = true };
					break;
			}
			var optPrefs = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag() },
				Extra = extra
			};

			Target.Execute(period, stateHolder.Schedules.SchedulesForPeriod(period, agent), new NoSchedulingProgress(), optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new GroupPageLight("_", GroupPageType.SingleAgent), () => new WorkShiftFinderResultHolder(), (o, args) => { });

			var wasModified = !stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(5)).HasDayOff();
			if (teamBlockType == TeamBlockType.Classic)
			{
				wasModified.Should().Be.False();
			}
			else if(_teamBlockDayOffForIndividuals)
			{
				wasModified.Should().Be.True();
			}
		}

		[TestCase(TeamBlockType.Team)]
		[TestCase(TeamBlockType.Block)]
		[TestCase(TeamBlockType.TeamAndBlock)]
		[TestCase(TeamBlockType.Classic)]
		public void ShouldMoveDayOffToDayWithLessDemandPerAgent_MarkedBlankDay(TeamBlockType teamBlockType)
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(1));
			var activity = new Activity("_");
			var skill = new Skill().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent1 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(firstDay);
			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(firstDay);
			agent1.SchedulePeriod(firstDay).SetDaysOff(1);
			agent2.SchedulePeriod(firstDay).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				1,
				5,
				5,
				5,
				25,
				5);
			var asses1 = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent1, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
			var asses2 = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent2, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
			asses1[5].SetDayOff(new DayOffTemplate());
			asses2[5].SetDayOff(new DayOffTemplate());
			asses1[0].ClearMainActivities();//bland day
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent1, agent2 }, asses1.Union(asses2), skillDays);
			IExtraPreferences extra = null;
			switch (teamBlockType)
			{
				case TeamBlockType.Classic:
					extra = new ExtraPreferences { UseTeams = false, UseTeamBlockOption = false };
					break;
				case TeamBlockType.Block:
					extra = new ExtraPreferences { UseTeams = false, UseTeamBlockOption = true };
					break;
				case TeamBlockType.Team:
					extra = new ExtraPreferences { UseTeams = true, UseTeamBlockOption = false };
					break;
				case TeamBlockType.TeamAndBlock:
					extra = new ExtraPreferences { UseTeams = true, UseTeamBlockOption = true };
					break;
			}
			var optPrefs = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag() },
				Extra = extra
			};

			Target.Execute(period, stateHolder.Schedules.SchedulesForPeriod(period, agent1, agent2), new NoSchedulingProgress(), optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new GroupPageLight("_", GroupPageType.SingleAgent), () => new WorkShiftFinderResultHolder(), (o, args) => { });

			var wasModified1 = !stateHolder.Schedules[agent1].ScheduledDay(firstDay.AddDays(5)).HasDayOff();
			var wasModified2 = !stateHolder.Schedules[agent2].ScheduledDay(firstDay.AddDays(5)).HasDayOff();

			if (teamBlockType == TeamBlockType.Classic)
			{
				wasModified1.Should().Be.False();
				wasModified2.Should().Be.True();
			}
			else if (_teamBlockDayOffForIndividuals)
			{
				wasModified1.Should().Be.True();
				wasModified2.Should().Be.True();
			}
		}

		[TestCase(TeamBlockType.Team)]
		[TestCase(TeamBlockType.Block)]
		[TestCase(TeamBlockType.TeamAndBlock)]
		[TestCase(TeamBlockType.Classic)]
		public void ShouldGetBackToLegalState(TeamBlockType teamBlockType)
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(1));
			var activity = new Activity("_");
			var contract = new ContractWithMaximumTolerance();
			var skill = new Skill().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				5,
				1,
				1,
				5,
				5,
				5);
			var asses = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
			asses[2].SetDayOff(new DayOffTemplate());
			asses[3].SetDayOff(new DayOffTemplate());
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, asses, skillDays);
			IExtraPreferences extra = null;
			switch (teamBlockType)
			{
				case TeamBlockType.Classic:
					extra = new ExtraPreferences { UseTeams = false, UseTeamBlockOption = false };
					break;
				case TeamBlockType.Block:
					extra = new ExtraPreferences { UseTeams = false, UseTeamBlockOption = true };
					break;
				case TeamBlockType.Team:
					extra = new ExtraPreferences { UseTeams = true, UseTeamBlockOption = false };
					break;
				case TeamBlockType.TeamAndBlock:
					extra = new ExtraPreferences { UseTeams = true, UseTeamBlockOption = true };
					break;
			}
			var optPrefs = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag() },
				Extra = extra
			};

			var dayOffsPreferences = new DaysOffPreferences
			{
				UseFullWeekendsOff = true,
				FullWeekendsOffValue = new MinMax<int>(1, 1)
			};

			Target.Execute(period, stateHolder.Schedules.SchedulesForPeriod(period, agent), new NoSchedulingProgress(), optPrefs, new FixedDayOffOptimizationPreferenceProvider(dayOffsPreferences), new GroupPageLight("_", GroupPageType.SingleAgent), () => new WorkShiftFinderResultHolder(), (o, args) => { });

			var wasModified1 = stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(5)).HasDayOff();
			var wasModified2 = stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(6)).HasDayOff();
			if (teamBlockType == TeamBlockType.Classic)
			{
				wasModified1.Should().Be.True();
				wasModified2.Should().Be.True();
			}
			else if (_teamBlockDayOffForIndividuals)
			{
				wasModified1.Should().Be.False();
				wasModified2.Should().Be.False();
			}
		}

		[Test]
		public void ShouldReScheduleWhiteSpotsAfterGetBackToLegalStateClassic()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(1));
			var activity = new Activity("_");
			var contract = new ContractWithMaximumTolerance();
			var skill = new Skill().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				5,
				1,
				1,
				5,
				5,
				5);
			var asses = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16))).ToArray();
			asses[2].SetDayOff(new DayOffTemplate());
			asses[3].SetDayOff(new DayOffTemplate());
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, asses, skillDays);
			var optPrefs = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag() },
				Extra = new ExtraPreferences { UseTeams = false, UseTeamBlockOption = false }
			};
			var dayOffsPreferences = new DaysOffPreferences
			{
				UseFullWeekendsOff = true,
				FullWeekendsOffValue = new MinMax<int>(1, 1)
			};

			Target.Execute(period, stateHolder.Schedules.SchedulesForPeriod(period, agent), new NoSchedulingProgress(), optPrefs, new FixedDayOffOptimizationPreferenceProvider(dayOffsPreferences), new GroupPageLight("_", GroupPageType.SingleAgent), () => new WorkShiftFinderResultHolder(), (o, args) => { });

			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(2)).IsScheduled().Should().Be.True();
			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(3)).IsScheduled().Should().Be.True();
		}

		[Test]
		public void ShouldGetBackToLegalStateWorkShifts()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(1));
			var activity = new Activity("_");
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(44), TimeSpan.FromHours(1), TimeSpan.FromHours(16)),
				NegativePeriodWorkTimeTolerance = TimeSpan.FromHours(4)
			};
			var skill = new Skill().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(1);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				5,
				5,
				5,
				5,
				1,
				1);
			var asses = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 17))).ToArray();
			asses[5].SetDayOff(new DayOffTemplate());
			asses[6].SetDayOff(new DayOffTemplate());
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, asses, skillDays);
			var optPrefs = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag() },
				Extra = new ExtraPreferences { UseTeams = false, UseTeamBlockOption = false }
			};

			Target.Execute(period, stateHolder.Schedules.SchedulesForPeriod(period, agent), new NoSchedulingProgress(), optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new GroupPageLight("_", GroupPageType.SingleAgent), () => new WorkShiftFinderResultHolder(), (o, args) => { });

			stateHolder.Schedules[agent].ScheduledDay(firstDay)
					.PersonAssignment()
					.ShiftLayers.First()
					.Period.ElapsedTime()
					.Should()
					.Be.EqualTo(TimeSpan.FromHours(8));
		}
	}
}