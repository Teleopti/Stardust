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
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[TestFixture(false)]
	[TestFixture(true)]
	[DomainTestWithStaticDependenciesAvoidUse]
	public class DayOffOptimizationTeamBlockDesktopTest : DayOffOptimizationScenario
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public DayOffOptimizationDesktopTeamBlock Target;
		public FakeRuleSetBagRepository RuleSetBagRepository; //needed also in Desktop tests becaused used in grouppagecreator

		public DayOffOptimizationTeamBlockDesktopTest(bool teamBlockDayOffForIndividuals) : base(teamBlockDayOffForIndividuals)
		{
		}

		[Test]
		public void ShouldCareAboutGroupType()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(1));
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSetBag = RuleSetBagRepository.Has(new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory))) {Description = new Description("_")});
			var team = new Team { Site = new Site("_") };
			var agents = new List<IPerson>();
			for (var i = 0; i < 2; i++)
			{
				var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithSchedulePeriodOneWeek(firstDay);
				agent.SchedulePeriod(firstDay).SetDaysOff(2);
				var personPeriod = new PersonPeriod(firstDay.AddWeeks(-1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team) { RuleSetBag = ruleSetBag };
				personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
				agent.AddPersonPeriod(personPeriod);
				agents.Add(agent);
			}
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1, 10, 10, 10, 10, 10, 100);
			var asses = new List<IPersonAssignment>();
			foreach (var agent in agents)
			{
				for (var i = 0; i < 7; i++)
				{
					var ass = new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16));
					asses.Add(ass);
					if (i == 5 || i==6)
					{
						ass.SetDayOff(new DayOffTemplate()); //saturday/sunday
					}
				}
			}
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, agents, asses, skillDays);
			var optPrefs = new OptimizationPreferences
			{
				General = {ScheduleTag = new ScheduleTag()},
				Extra = {UseTeamSameDaysOff = true}
			};
			var groupPageLight = new GroupPageLight("_", GroupPageType.RuleSetBag);

			Target.Execute(period, stateHolder.Schedules.SchedulesForPeriod(period, agents.ToArray()), new NoSchedulingProgress(), optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), groupPageLight, () => new WorkShiftFinderResultHolder(), (o, args) => { });

			var dayOffs = stateHolder.Schedules.SchedulesForPeriod(period, agents.ToArray()).Where(x => x.HasDayOff()).Select(x => x.PersonAssignment());
			var dayOffsAgent1 = dayOffs.Where(x => x.Person.Equals(agents.First())).Select(x => x.Date);
			var dayOffsAgent2 = dayOffs.Where(x => x.Person.Equals(agents.Last())).Select(x => x.Date);
			dayOffsAgent1.Should().Have.SameValuesAs(dayOffsAgent2);
			dayOffsAgent1.Should().Not.Have.SameValuesAs(skillDays[5].CurrentDate, skillDays[6].CurrentDate);
		}

		[Test]
		public void ShouldNotCrashOnAgentWithoutRuleSetBag()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(1));
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory))) { Description = new Description("_") };
			var team = new Team { Site = new Site("_").WithId() }.WithId();
			var agents = new List<IPerson>();
			var contract = new Contract("Contract").WithId();
			for (var i = 0; i < 2; i++)
			{
				var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithSchedulePeriodOneWeek(firstDay);
				agent.SchedulePeriod(firstDay).SetDaysOff(2);
				var personPeriod = new PersonPeriod(firstDay.AddWeeks(-1), new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), team);
				if (i == 1)
				{
					personPeriod.RuleSetBag = ruleSetBag;
				}
				personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
				agent.AddPersonPeriod(personPeriod);
				agents.Add(agent);
			}
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 10, 10, 10, 10, 10, 100);
			var asses = new List<IPersonAssignment>();
			foreach (var agent in agents)
			{
				for (var i = 0; i < 7; i++)
				{
					var ass = new PersonAssignment(agent, scenario, firstDay.AddDays(i));
					ass.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
					ass.SetShiftCategory(shiftCategory);
					asses.Add(ass);
					if (i == 5 || i == 6)
					{
						ass.SetDayOff(new DayOffTemplate()); //saturday/sunday
					}
				}
			}
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, agents, asses, skillDays);
			var groupPageLight = new GroupPageLight("Contract", GroupPageType.Contract, "Contract");

			var optPrefs = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag() },
				Extra = { UseTeamSameDaysOff = true, UseTeams = true, TeamGroupPage = groupPageLight}
			};

			Target.Execute(period, stateHolder.Schedules.SchedulesForPeriod(period, agents.ToArray()), new NoSchedulingProgress(), optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), groupPageLight, () => new WorkShiftFinderResultHolder(), (o, args) => { });
		}

		[Test]
		public void ShouldNotPlaceShiftsOnClosedDaysWhenUsingSameShift()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(1));
			var activity1 = new Activity("1").WithId();
			var activity2 = new Activity("2").WithId();
			activity1.RequiresSkill = true;
			activity2.RequiresSkill = true;
			var skill1 = new Skill("_").For(activity1).WithId().IsOpen();
			var skill2 = new Skill("_").For(activity2).WithId().IsOpenDuringWeekends();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity1, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			ruleSet.AddExtender(new ActivityAbsoluteStartExtender(activity2, new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(10, 0, 10, 0, 15)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill1, skill2).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(2);
			var skillDays1 = skill1.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 10, 10, 10, 10, 100, 100);
			var skillDays2 = skill2.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 10, 10, 10, 10, 10, 0, 0, 10);
			var asses = new List<IPersonAssignment>();
			var dayOffTemplate = new DayOffTemplate();
			for (var i = 0; i < 7; i++)
			{
				var ass = new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity2, new TimePeriod(8, 16));
				asses.Add(ass);
				if (i == 5 || i == 6)
				{
					ass.SetDayOff(dayOffTemplate); //saturday/sunday
				}
			}		
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new [] {agent}, asses, skillDays1.Union(skillDays2));
			var optPrefs = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag(), OptimizationStepDaysOff = true },
				Extra = { UseBlockSameShift = true, UseTeamBlockOption = true, BlockTypeValue = BlockFinderType.BetweenDayOff}
			};
			var groupPageLight = new GroupPageLight("_", GroupPageType.SingleAgent);

			Target.Execute(period, stateHolder.Schedules.SchedulesForPeriod(period, agent), new NoSchedulingProgress(), optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), groupPageLight, () => new WorkShiftFinderResultHolder(), (o, args) => { });

			stateHolder.Schedules[agent].ScheduledDay(skillDays1[5].CurrentDate).PersonAssignment().AssignedWithDayOff(dayOffTemplate).Should().Be.True();
			stateHolder.Schedules[agent].ScheduledDay(skillDays1[6].CurrentDate).PersonAssignment().AssignedWithDayOff(dayOffTemplate).Should().Be.True();
		}

		[Test]
		public void ShouldConsiderDayOffPreferences()
		{
			//hard-to-understand-test... setup is like in bugdb mentioned here: http://challenger:8080/tfs/web/UI/Pages/WorkItems/WorkItemEdit.aspx?Id=40314

			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(3));
			var activity = new Activity("_");
			var channelSales = new Skill("A").For(activity).WithId().IsOpenDuringWeekends();
			var directSales = new Skill("B").For(activity).WithId().IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15),new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 3);
			schedulePeriod.SetDaysOff(6);

			var agents = new List<IPerson>();
			var asses = new List<IPersonAssignment>();
			var dayOffTemplate = new DayOffTemplate();

			for (var i = 0; i < 10; i++)
			{
				var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, channelSales, directSales);
				agent.AddSchedulePeriod(schedulePeriod);
				agents.Add(agent);

				for (var day = 0; day < 21; day++)
				{
					var currentDay = firstDay.AddDays(day);
					var ass = new PersonAssignment(agent, scenario, currentDay).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16));
					if (i < 7)
					{
						if (day == 5 || day == 6 || day == 12 || day == 13 || day == 19 || day == 20)
						{
							ass.SetDayOff(dayOffTemplate); //saturday/sunday
						}
					}

					if (i == 7)
					{
						if (day == 3 || day == 4 || day == 11 || day == 12 || day == 19 || day == 20)
						{
							ass.SetDayOff(dayOffTemplate); 
						}
					}

					if (i == 8)
					{
						if (day == 5 || day == 6 || day == 12 || day == 13 || day == 17 || day == 18)
						{
							ass.SetDayOff(dayOffTemplate);
						}
					}

					if (i == 9)
					{
						if (day == 5 || day == 6 || day == 12 || day == 13 || day == 16 || day == 17)
						{
							ass.SetDayOff(dayOffTemplate); 
						}
					}
					asses.Add(ass);
				}
			}
			

			var skillDaysChannelSales = channelSales.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,	171.35, 135.06, 142.09, 139.48, 125.52, 0, 0,
																													171.35, 135.06, 142.09, 139.48, 125.52, 0, 0,
																													171.35, 135.06, 142.09, 139.48, 125.52, 0, 0);


			var skillDaysDirectSales = directSales.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,	707.35, 693.22, 641.34, 629.57, 649.46, 422.20, 194.16,
																													707.35, 693.22, 641.34, 629.57, 649.46, 422.20, 194.16,
																													707.35, 693.22, 641.34, 629.57, 649.46, 422.20, 194.16);


			var groupPageLight = new GroupPageLight("_", GroupPageType.RuleSetBag);	
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, agents, asses, skillDaysChannelSales.Union(skillDaysDirectSales) );
			var optPrefs = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag(), OptimizationStepDaysOff = true},
				Extra = { UseBlockSameShiftCategory = true, UseTeamBlockOption = true}
			};
			var dayOffsPreferences = new DaysOffPreferences
			{
				UseDaysOffPerWeek = true,
				UseConsecutiveDaysOff = true,
				UseConsecutiveWorkdays = true,
				DaysOffPerWeekValue = new MinMax<int>(2, 2),
				ConsecutiveDaysOffValue = new MinMax<int>(2, 2),
				ConsecutiveWorkdaysValue = new MinMax<int>(2, 6)
			};

			Target.Execute(period, stateHolder.Schedules.SchedulesForPeriod(period, agents[0]), new NoSchedulingProgress(), optPrefs, new FixedDayOffOptimizationPreferenceProvider(dayOffsPreferences), groupPageLight, () => new WorkShiftFinderResultHolder(), (o, args) => { });

			var consecutiveDaysOff = 0;
			for (var day = 0; day < 21; day ++)
			{
				if (stateHolder.Schedules[agents[0]].ScheduledDay(firstDay.AddDays(day)).HasDayOff())
				{
					consecutiveDaysOff++;
				}
				else
				{
					if (consecutiveDaysOff == 0)
							continue;
					consecutiveDaysOff.Should().Be.EqualTo(2);
					consecutiveDaysOff = 0;
				}
			}
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
			asses[5].SetDayOff(new DayOffTemplate());
			asses[0].ClearMainActivities();//bland day
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent }, asses, skillDays);
			IExtraPreferences extra = null;
			switch (teamBlockType)
			{
				case TeamBlockType.Classic:
					extra = new ExtraPreferences {UseTeams = false, UseTeamBlockOption = false};
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
			else
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
			var team = new Team { Site = new Site("_") };
			var agent1 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skill).WithSchedulePeriodOneWeek(firstDay);
			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skill).WithSchedulePeriodOneWeek(firstDay);
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
			else
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
			var skill = new Skill().For(activity).IsOpen();
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var team = new Team { Site = new Site("_") };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skill).WithSchedulePeriodOneWeek(firstDay);
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
				FullWeekendsOffValue = new MinMax<int>(1,1)
			};

			Target.Execute(period, stateHolder.Schedules.SchedulesForPeriod(period, agent), new NoSchedulingProgress(), optPrefs, new FixedDayOffOptimizationPreferenceProvider(dayOffsPreferences), new GroupPageLight("_", GroupPageType.SingleAgent), () => new WorkShiftFinderResultHolder(), (o, args) => { });

			var wasModified1 = stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(5)).HasDayOff();
			var wasModified2 = stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(6)).HasDayOff();
			if (teamBlockType == TeamBlockType.Classic)
			{
				wasModified1.Should().Be.True();
				wasModified2.Should().Be.True();
			}
			else
			{
				wasModified1.Should().Be.False();
				wasModified2.Should().Be.False();
			}
		}
	}
}