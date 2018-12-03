using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
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


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class DaysOffSchedulingDesktopTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		public void DaysOffFromContractScheduleShouldBePossibleToPlaceOnClosedDays()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_");
			var timePeriod = new TimePeriod(8, 16);
			var skill = new Skill().WithId().For(activity).IsOpen(timePeriod, timePeriod, timePeriod, timePeriod, timePeriod); //closed on saturdays and sundays
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var team = new Team { Site = new Site("_") };
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skill).WithSchedulePeriodOneWeek(firstDay);
			agent.SchedulePeriod(firstDay).SetDaysOff(2);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 5, 1, 5, 5, 5);
			
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, Enumerable.Empty<IPersonAssignment>(), skillDays);

			Target.Execute(new NoSchedulingCallback(),
				new SchedulingOptions(),
				new NoSchedulingProgress(),
				new[] { agent },
				period);

			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(5)).HasDayOff().Should().Be.True();//closed saturday
			stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(6)).HasDayOff().Should().Be.True();//closed sunday
		}

		public DaysOffSchedulingDesktopTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}