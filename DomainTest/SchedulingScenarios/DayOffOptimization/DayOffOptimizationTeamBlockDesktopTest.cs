using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[TestFixture(false, true)]
	[TestFixture(true, true)]
	[TestFixture(false, false)]
	[TestFixture(true, false)]
	[DomainTest]
	public class DayOffOptimizationTeamBlockDesktopTest : DayOffOptimizationScenario
	{
		private readonly bool _teamSameDayOff;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public DayOffOptimizationDesktopTeamBlock Target;
		public FakeRuleSetBagRepository RuleSetBagRepository; //needed also in Desktop tests becaused used in grouppagecreator

		public DayOffOptimizationTeamBlockDesktopTest(bool teamBlockDayOffForIndividuals, bool teamSameDayOff) : base(teamBlockDayOffForIndividuals)
		{
			_teamSameDayOff = teamSameDayOff;
		}

		[Test]
		public void ShouldCareAboutGroupType()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
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
				Extra = {UseTeamSameDaysOff = true, UseTeams = true}
			};
			var groupPageLight = new GroupPageLight("_", GroupPageType.RuleSetBag);

			Target.Execute(period, agents, new NoSchedulingProgress(), optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), groupPageLight, () => new WorkShiftFinderResultHolder(), (o, args) => { });

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
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
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
			SchedulerStateHolder.Fill(scenario, period, agents, asses, skillDays);
			var groupPageLight = new GroupPageLight("Contract", GroupPageType.Contract, "Contract");

			var optPrefs = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag() },
				Extra = { UseTeamSameDaysOff = true, UseTeams = true, TeamGroupPage = groupPageLight}
			};

			Target.Execute(period, agents, new NoSchedulingProgress(), optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), groupPageLight, () => new WorkShiftFinderResultHolder(), (o, args) => { });
		}

		[Test]
		public void ShouldNotPlaceShiftsOnClosedDaysWhenUsingSameShift()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
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

			Target.Execute(period, new[] { agent }, new NoSchedulingProgress(), optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), groupPageLight, () => new WorkShiftFinderResultHolder(), (o, args) => { });

			stateHolder.Schedules[agent].ScheduledDay(skillDays1[5].CurrentDate).PersonAssignment().AssignedWithDayOff(dayOffTemplate).Should().Be.True();
			stateHolder.Schedules[agent].ScheduledDay(skillDays1[6].CurrentDate).PersonAssignment().AssignedWithDayOff(dayOffTemplate).Should().Be.True();
		}

		[Test]
		public void ShouldConsiderDayOffPreferences()
		{
			//hard-to-understand-test... setup is like in bugdb mentioned here: http://challenger:8080/tfs/web/UI/Pages/WorkItems/WorkItemEdit.aspx?Id=40314

			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 3);
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

			Target.Execute(period, new[] { agents[0] }, new NoSchedulingProgress(), optPrefs, new FixedDayOffOptimizationPreferenceProvider(dayOffsPreferences), groupPageLight, () => new WorkShiftFinderResultHolder(), (o, args) => { });

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

		public override void Configure(FakeToggleManager toggleManager)
		{
			base.Configure(toggleManager);
			if(_teamSameDayOff)
				toggleManager.Enable(Toggles.ResourcePlanner_TeamSameDayOff_44265);
		}
	}
}