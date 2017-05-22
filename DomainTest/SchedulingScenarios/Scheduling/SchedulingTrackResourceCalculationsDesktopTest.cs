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
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class SchedulingTrackResourceCalculationsDesktopTest : SchedulingScenario, ISetup
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public SchedulingOptionsProvider SchedulingOptionsProvider;
		public ResourceCalculationWithCount ResourceCalculation;

		[Test]
		public void ShouldRespectResourceCalculateFrequency()
		{
			var firstDay = new DateOnly(2017, 5, 15);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_").WithId();
			var skill = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(90), TimeSpan.FromHours(8), TimeSpan.FromHours(0))};
			var agents = new List<IPerson>();
			for (var i = 0; i < 50; i++)
			{
				agents.Add(new Person().WithId()
					.InTimeZone(TimeZoneInfo.Utc)
					.WithPersonPeriod(ruleSet, contract, skill)
					.WithSchedulePeriodOneWeek(firstDay));
			}
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1);
			SchedulerStateHolderFrom.Fill(scenario, period, agents, Enumerable.Empty<IPersonAssignment>(), skillDays);
			var schedulingOptions = SchedulingOptionsProvider.Fetch();
			schedulingOptions.ResourceCalculateFrequency = 100;
			SchedulingOptionsProvider.SetFromTest(schedulingOptions);

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), agents, period, new OptimizationPreferences(), new DaysOffPreferences());

			ResourceCalculation.NumberOfCalculationsOnSingleDay
				.Should().Be.LessThan(20); //the lowest, in theory would be 7
		}

		public SchedulingTrackResourceCalculationsDesktopTest(bool resourcePlannerMergeTeamblockClassicScheduling44289) : base(resourcePlannerMergeTeamblockClassicScheduling44289)
		{
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ResourceCalculationWithCount>().For<IResourceCalculation>();
		}
	}
}