using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	[Toggle(Toggles.ResourcePlanner_SchedulingFewerResourceCalculations_45429)]
	[Toggle(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
	public class NumberOfResourceCalculationsDesktopTest : ISetup
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public ResourceCalculationWithCount ResourceCalculation;
		public SchedulingResourceCalculationLimiter SchedulingResourceCalculationLimiter;

		[Test]
		public void ShouldRespectLargeSkillGroups()
		{
			var firstDay = new DateOnly(2017, 5, 15);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_").WithId();
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(90), TimeSpan.FromHours(8), TimeSpan.FromHours(0)) };
			var agents = new List<IPerson>();
			for (var i = 0; i < 10; i++)
			{
				agents.Add(new Person().WithId()
					.InTimeZone(TimeZoneInfo.Utc)
					.WithPersonPeriod(ruleSet, contract, skill)
					.WithSchedulePeriodOneWeek(firstDay));
			}
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1);
			SchedulerStateHolderFrom.Fill(scenario, period, agents, Enumerable.Empty<IPersonAssignment>(), skillDays);

			SchedulingResourceCalculationLimiter.SetLimits_UseOnlyFromTest(10, new Percent(0.01));
			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), agents, period);

			ResourceCalculation.NumberOfCalculationsOnSingleDay
				.Should().Be.LessThan(10);
		}

		[Test]
		public void ShouldRespectLargeSkillGroups_MakeSureResourceCalculationHappensNowAndThen()
		{
			var firstDay = new DateOnly(2017, 5, 15);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_").WithId();
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(90), TimeSpan.FromHours(8), TimeSpan.FromHours(0)) };
			var agents = new List<IPerson>();
			for (var i = 0; i < 10; i++)
			{
				agents.Add(new Person().WithId()
					.InTimeZone(TimeZoneInfo.Utc)
					.WithPersonPeriod(ruleSet, contract, skill)
					.WithSchedulePeriodOneWeek(firstDay));
			}
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1);
			SchedulerStateHolderFrom.Fill(scenario, period, agents, Enumerable.Empty<IPersonAssignment>(), skillDays);

			SchedulingResourceCalculationLimiter.SetLimits_UseOnlyFromTest(10, new Percent(0.5));
			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), agents, period);

			ResourceCalculation.NumberOfCalculationsOnSingleDay
				.Should().Be.IncludedIn(5, 65);
		}

		[Test]
		public void ShouldRespectSmallSkillGroups()
		{
			var firstDay = new DateOnly(2017, 5, 15);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_").WithId();
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(90), TimeSpan.FromHours(8), TimeSpan.FromHours(0)) };
			var agents = new List<IPerson>();
			for (var i = 0; i < 10; i++)
			{
				agents.Add(new Person().WithId()
					.InTimeZone(TimeZoneInfo.Utc)
					.WithPersonPeriod(ruleSet, contract, skill)
					.WithSchedulePeriodOneWeek(firstDay));
			}
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1);
			SchedulerStateHolderFrom.Fill(scenario, period, agents, Enumerable.Empty<IPersonAssignment>(), skillDays);

			SchedulingResourceCalculationLimiter.SetLimits_UseOnlyFromTest(100, new Percent(0.01));
			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), agents, period);

			const int dueToSomeOtherSmallOptimizations = 3;
			ResourceCalculation.NumberOfCalculationsOnSingleDay
				.Should().Be.GreaterThanOrEqualTo(10 * 7 - dueToSomeOtherSmallOptimizations);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ResourceCalculationWithCount>().For<IResourceCalculation>();
		}
	}
}