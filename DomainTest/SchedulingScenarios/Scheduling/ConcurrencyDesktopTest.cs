using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;
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
	public class ConcurrencyDesktopTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public MergeIslandsSizeLimit MergeIslandsSizeLimit;

		[Test]
		public void ShouldNotCrashDueToMultipleNonExistingSchedulingRangesWhenFillingSchedulerStateHolder()
		{
			const int agentsInEachIsland = 10;
			const int numberOfIslands = 10;
			var date = new DateOnly(2017, 1, 10);
			var scenario = new Scenario("_");
			var agents = new List<IPerson>();
			var skillDays = new List<ISkillDay>();
			var activity = new Activity().WithId();
			for (var i = 0; i < numberOfIslands; i++)
			{
				var skill = new Skill().For(activity).WithId().IsOpen();
				var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 10);
				skillDays.Add(skillDay);

				agents.AddRange(Enumerable.Repeat(0,agentsInEachIsland).Select(j => new Person().WithId()
					.WithPersonPeriod(skill)
					.WithSchedulePeriodOneWeek(date)));
			}
			SchedulerStateHolderFrom.Fill(scenario, new DateOnly(2017, 1, 10), agents, skillDays);

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(new NoSchedulingCallback(),
					new SchedulingOptions(), 
					new NoSchedulingProgress(),
					agents, 
					new DateOnlyPeriod(date, date.AddDays(1))
				);
			});
		}

		[Test]
		public void ShouldNotFailToPlaceShiftsDueToRaceConditionWhenSettingDate()
		{
			const int numberOfIslands = 10;
			const int numberOfAgentsInEachIsland = 10;
			var date = new DateOnly(2017, 1, 10);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var scenario = new Scenario();
			var agents = new List<IPerson>();
			var skillDays = new List<ISkillDay>();
			var activity = new Activity().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 10, 0, 60), new TimePeriodWithSegment(15, 0, 18, 0, 60), new ShiftCategory("_").WithId())).WithId();
			for (var i = 0; i < numberOfIslands; i++)
			{
				var skill = new Skill().For(activity).WithId().IsOpen();
				skillDays.AddRange(skill.CreateSkillDayWithDemand(scenario, period, TimeSpan.FromDays(10000)));

				agents.AddRange(Enumerable.Repeat(0, numberOfAgentsInEachIsland).Select(j => new Person().WithId()
					.WithPersonPeriod(ruleSet, skill)
					.WithSchedulePeriodOneWeek(date)));
			}
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, agents, skillDays);

			Target.Execute(new NoSchedulingCallback(),
					new SchedulingOptions(), 
					new NoSchedulingProgress(),
					agents, 
					period);

			stateHolder.Schedules.SchedulesForPeriod(period, agents.ToArray()).Count(x => x.IsScheduled())
				.Should().Be.EqualTo(agents.Count * period.DayCollection().Count);
		}

		public override void OnBefore()
		{
			base.OnBefore();
			MergeIslandsSizeLimit.TurnOff_UseOnlyFromTest();
		}

		public ConcurrencyDesktopTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}