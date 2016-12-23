using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(RunInProcessEventPublisher))]
	[LoggedOnAppDomain]
	[TestFixture(true)]
	[TestFixture(false)]
	public class IntradayOptimizationCallbackDesktopTest : ISetup, IConfigureToggleManager
	{
		private readonly bool _resourcePlannerSplitBigIslands42049;
		public OptimizeIntradayIslandsDesktop Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public DesktopOptimizationContext DesktopOptimizationContext;

		public IntradayOptimizationCallbackDesktopTest(bool resourcePlannerSplitBigIslands42049)
		{
			_resourcePlannerSplitBigIslands42049 = resourcePlannerSplitBigIslands42049;
		}

		[Test]
		public void ShouldDoSuccesfulCallbacks()
		{
			const int numberOfAgents = 10;
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var asses = new List<IPersonAssignment>();
			for (var i = 0; i < numberOfAgents; i++)
			{
				var agent = new Person().WithId();
				agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
				agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
				var ass = new PersonAssignment(agent, scenario, dateOnly);
				ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
				ass.SetShiftCategory(new ShiftCategory("_").WithId());
				asses.Add(ass);
			}
			SchedulerStateHolderFrom.Fill(scenario, dateOnly, asses.Select(x => x.Person), asses, skillDay);

			var callbackTracker = new TrackIntradayOptimizationCallback();
			Target.Optimize(asses.Select(x => x.Person), new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), callbackTracker);
			callbackTracker.SuccessfulOptimizations().Should().Be.EqualTo(10);
		}

		[Test]
		public void ShouldDoUnsuccesfulCallbacks()
		{
			const int numberOfAgents = 10;
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var asses = new List<IPersonAssignment>();
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId();
			for (var i = 0; i < numberOfAgents; i++)
			{
				var agent = new Person().WithId();
				agent.AddPeriodWithSkill(new PersonPeriod(dateOnly, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
				agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1));
				var ass = new PersonAssignment(agent, scenario, dateOnly);
				ass.AddActivity(phoneActivity, new TimePeriod(8, 0, 17, 0));
				ass.SetShiftCategory(new ShiftCategory("_").WithId());
				asses.Add(ass);
			}
			SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), asses.Select(x => x.Person), asses, new List<ISkillDay>());

			var callbackTracker = new TrackIntradayOptimizationCallback();
			Target.Optimize(asses.Select(x => x.Person), new DateOnlyPeriod(dateOnly, dateOnly), new OptimizationPreferencesDefaultValueProvider().Fetch(), callbackTracker);
			callbackTracker.UnSuccessfulOptimizations().Should().Be.EqualTo(10);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<DesktopOptimizationContext>().For<IFillSchedulerStateHolder, ISynchronizeIntradayOptimizationResult, IOptimizationPreferencesProvider, IPeopleInOrganization, ICurrentIntradayOptimizationCallback>();
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if (_resourcePlannerSplitBigIslands42049)
				toggleManager.Enable(Toggles.ResourcePlanner_SplitBigIslands_42049);
		}
	}
}