using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(RunInProcessEventPublisher))]
	[Toggle(Toggles.ResourcePlanner_CascadingSkills_38524)]
	public class IntradayOptimizationCascadingDesktopTest : ISetup
	{
		public OptimizeIntradayIslandsDesktop Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		public void ShouldShovelResources()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skillA = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillA.SetCascadingIndex_UseFromTestOnly(1);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 17, 0));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var skillB = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = phoneActivity, TimeZone = TimeZoneInfo.Utc }.WithId();
			skillB.SetCascadingIndex_UseFromTestOnly(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 17, 0));
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, dateOnly, 1);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkills(new PersonPeriod(dateOnly, new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), new[] { skillA, skillB });
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
			agent.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
			ass.SetShiftCategory(new ShiftCategory("_").WithId());
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new[] { ass }, new[] { skillDayA, skillDayB });

			Target.Optimize(new[] { agent }, new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), null);

			schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillA].Single().SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
			schedulerStateHolderFrom.SchedulingResultState.SkillDays[skillB].Single().SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(0);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<DesktopOptimizationContext>().For<IFillSchedulerStateHolder, ISynchronizeIntradayOptimizationResult, IOptimizationPreferencesProvider, IPeopleInOrganization>();
			// share the same current principal on all threads
			system.UseTestDouble(new FakeCurrentTeleoptiPrincipal(Thread.CurrentPrincipal as ITeleoptiPrincipal)).For<ICurrentTeleoptiPrincipal>();
		}
	}
}