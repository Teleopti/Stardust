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
	public class DayOffOptimizationTeamBlockDesktopTest : DayOffOptimizationScenario
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public IDayOffOptimizationDesktop Target;
		public FakeGroupScheduleGroupPageDataProvider GroupScheduleGroupPageDataProvider; //should not use fake here...

		public DayOffOptimizationTeamBlockDesktopTest(bool cascading) : base(true, cascading)
		{
		}

		[Test, Ignore]
		public void ShouldCareAboutGroupType()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(1));
			var activity = new Activity("_");
			var skill = new Skill("_", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = activity };
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory))) {Description = new Description("_")};
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
			GroupScheduleGroupPageDataProvider.AddRuleSetBag(ruleSetBag); //remove me later!


			Target.Execute(period, stateHolder.Schedules.SchedulesForPeriod(period, agents.ToArray()), new NoSchedulingProgress(), optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), () => new WorkShiftFinderResultHolder(), (o, args) => { });

			var dayOffs = stateHolder.Schedules.SchedulesForPeriod(period, agents.ToArray()).Where(x => x.HasDayOff()).Select(x => x.PersonAssignment());
			var dayOffsAgent1 = dayOffs.Where(x => x.Person.Equals(agents.First())).Select(x => x.Date);
			var dayOffsAgent2 = dayOffs.Where(x => x.Person.Equals(agents.Last())).Select(x => x.Date);
			dayOffsAgent1.Should().Have.SameValuesAs(dayOffsAgent2);
			dayOffsAgent1.Should().Not.Have.SameValuesAs(skillDays[5].CurrentDate, skillDays[6].CurrentDate);
		}
	}
}