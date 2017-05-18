using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[TestFixture(true, true, true)]
	[TestFixture(false, true, true)]
	[TestFixture(true, false, true)]
	[TestFixture(false, false, true)]
	[TestFixture(true, true, false)]
	[TestFixture(false, true, false)]
	[TestFixture(true, false, false)]
	[TestFixture(false, false, false)]
	public class ShiftCategoryLimitationWithAndWithoutRandomnessTest : SchedulingScenario, ISetup
	{
		private readonly bool _fakeRandomness;
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		public ShiftCategoryLimitationWithAndWithoutRandomnessTest(bool resourcePlannerMergeTeamblockClassicScheduling44289, bool fakeRandomness) : base(resourcePlannerMergeTeamblockClassicScheduling44289)
		{
			_fakeRandomness = fakeRandomness;
		}

		[Test]
		[Ignore("#44171")]
		public void ShouldNotLeaveBlankSpotWhenAbleToSolve()
		{
			var date = new DateOnly(2017, 1, 22);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var shiftCat1 = new ShiftCategory("_").WithId();
			var shiftCat2 = new ShiftCategory("_").WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 7, 7, 7, 1, 1, 1, 1);
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCat1));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), shiftCat2));
			var ruleSetBag = new RuleSetBag(ruleSet1);
			ruleSetBag.AddRuleSet(ruleSet2);
			var agent = new Person().WithSchedulePeriodOneWeek(date).WithPersonPeriod(ruleSetBag, new ContractWithMaximumTolerance(), skill).InTimeZone(TimeZoneInfo.Utc);
			agent.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCat1) { MaxNumberOf = 3 });
			agent.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCat2) { MaxNumberOf = 3 });
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent },
				new[]
				{
					new PersonAssignment(agent, scenario, date.AddDays(1)).IsDayOff(),
					new PersonAssignment(agent, scenario, date.AddDays(3)).IsDayOff()
				},
				skillDays
			);
			var schedulingOptions = new SchedulingOptions
			{
				UseBlock = true,
				UseShiftCategoryLimitations = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
				BlockSameShiftCategory = true
			};

			Target.Execute(schedulingOptions, new NoSchedulingProgress(), new[] { agent }, period, new OptimizationPreferences(), null);

			stateholder.Schedules[agent]
				.ScheduledDayCollection(period)
				.Count(x => x.PersonAssignment(true).MainActivities().Any())
				.Should()
				.Be.EqualTo(5);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			if (_fakeRandomness)
			{
				system.UseTestDouble<TakeShiftWithLatestShiftStartIfSameShiftValue>().For<IEqualWorkShiftValueDecider>();
			}
		}
	}
}