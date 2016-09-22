using System;
using System.Collections.Generic;
using System.Drawing;
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
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[TestFixture(false, false)]
	[TestFixture(false, true)]
	[TestFixture(true, false)]
	[TestFixture(true, true)]
	[DomainTest]
	public class DayOffOptimizationTeamBlockDesktopTest : DayOffOptimizationScenario
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public DayOffOptimizationDesktopTeamBlock Target;
		public FakeRuleSetBagRepository RuleSetBagRepository; //needed also in Desktop tests becaused used in grouppagecreator

		public DayOffOptimizationTeamBlockDesktopTest(bool teamBlockDayOffForIndividuals, bool cascading) : base(teamBlockDayOffForIndividuals, cascading)
		{
		}

		[Test]
		public void ShouldCareAboutGroupType()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(1));
			var activity = new Activity("_");
			var skill = new Skill("_", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = activity };
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSetBag = RuleSetBagRepository.Has(new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory))) {Description = new Description("_")});
			var team = new Team { Site = new Site("_") };
			var agents = new List<IPerson>();
			for (var i = 0; i < 2; i++)
			{
				var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
				var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
				schedulePeriod.SetDaysOff(2);
				var personPeriod = new PersonPeriod(firstDay.AddWeeks(-1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team) { RuleSetBag = ruleSetBag };
				agent.AddPeriodWithSkill(personPeriod, skill);
				agent.AddSchedulePeriod(schedulePeriod);
				agents.Add(agent);
			}
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1, 10, 10, 10, 10, 10, 100);
			var asses = new List<IPersonAssignment>();
			foreach (var agent in agents)
			{
				for (var i = 0; i < 7; i++)
				{
					var ass = new PersonAssignment(agent, scenario, firstDay.AddDays(i));
					ass.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
					ass.SetShiftCategory(shiftCategory);
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
			var skill = new Skill("_", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = activity };
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory))) { Description = new Description("_") };
			var team = new Team { Site = new Site("_").WithId() }.WithId();
			var agents = new List<IPerson>();
			var contract = new Contract("Contract").WithId();
			for (var i = 0; i < 2; i++)
			{
				var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
				var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
				schedulePeriod.SetDaysOff(2);
				var personPeriod = new PersonPeriod(firstDay.AddWeeks(-1), new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), team);
				if (i == 1) personPeriod.RuleSetBag = ruleSetBag;	
				agent.AddPeriodWithSkill(personPeriod, skill);
				agent.AddSchedulePeriod(schedulePeriod);
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
		public void ShouldConsiderDayOffPreferences()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(3));
			var activity = new Activity("_");

			var channelSales = new Skill("A", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = activity }.WithId();
			var directSales = new Skill("B", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = activity }.WithId();

			var workloadChannelSales = WorkloadFactory.CreateWorkload(channelSales);
			for (var i = 0; i < workloadChannelSales.TemplateWeekCollection.Count; i++)
			{
				if (i < 5)
				{
					workloadChannelSales.TemplateWeekCollection[i].MakeOpen24Hours();
				}
				else
				{
					workloadChannelSales.TemplateWeekCollection[i].Close();
				}

			}
			WorkloadFactory.CreateWorkloadWithFullOpenHours(directSales);

			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSetBag = RuleSetBagRepository.Has(new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory))) { Description = new Description("_") });
			var team = new Team { Site = new Site("_") };	
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 3);
			schedulePeriod.SetDaysOff(6);

			var agents = new List<IPerson>();
			var asses = new List<IPersonAssignment>();
			var dayOffTemplate = new DayOffTemplate();

			for (int i = 0; i < 10; i++)
			{
				var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
				var personPeriod = new PersonPeriod(firstDay.AddWeeks(-1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team) { RuleSetBag = ruleSetBag };
				agent.AddPeriodWithSkills(personPeriod, new[] {channelSales, directSales});
				agent.AddSchedulePeriod(schedulePeriod);
				agents.Add(agent);

				for (var j = 0; j < 21; j++)
				{
					if (i < 7)
					{
						var ass = new PersonAssignment(agent, scenario, firstDay.AddDays(j));	
						if (j == 5 || j == 6 || j == 12 || j == 13 || j == 19 || j == 20)
						{
							ass.SetDayOff(dayOffTemplate); //saturday/sunday
						}
						else
						{
							ass.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
							ass.SetShiftCategory(shiftCategory);
						}

						asses.Add(ass);
					}

					if (i == 7)
					{
						var ass = new PersonAssignment(agent, scenario, firstDay.AddDays(j));
						if (j == 3 || j == 4 || j == 11 || j == 12 || j == 19 || j == 20)
						{
							ass.SetDayOff(dayOffTemplate); 
						}
						else
						{
							ass.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
							ass.SetShiftCategory(shiftCategory);
						}

						asses.Add(ass);
					}

					if (i == 8)
					{
						var ass = new PersonAssignment(agent, scenario, firstDay.AddDays(j));
						if (j == 5 || j == 6 || j == 12 || j == 13 || j == 17 || j == 18)
						{
							ass.SetDayOff(dayOffTemplate);
						}
						else
						{
							ass.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
							ass.SetShiftCategory(shiftCategory);
						}

						asses.Add(ass);
					}

					if (i == 9)
					{
						var ass = new PersonAssignment(agent, scenario, firstDay.AddDays(j));
						if (j == 5 || j == 6 || j == 12 || j == 13 || j == 16 || j == 17)
						{
							ass.SetDayOff(dayOffTemplate); 
						}
						else
						{
							ass.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
							ass.SetShiftCategory(shiftCategory);
						}

						asses.Add(ass);
					}
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
			var dayOffsPreferences = new DaysOffPreferences()
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
			for (var i = 0; i < 21; i ++)
			{
				if (stateHolder.Schedules[agents[0]].ScheduledDay(firstDay.AddDays(i)).PersonAssignment().DayOff() != null)
				{
					consecutiveDaysOff++;
				}
				else
				{
					if (consecutiveDaysOff > 0)
					{
						consecutiveDaysOff.Should().Be.EqualTo(2);
					}
					consecutiveDaysOff = 0;
				}

				consecutiveDaysOff.Should().Not.Be.GreaterThan(2);
			}
		}
	}
}