using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[TestFixtureSource(typeof(schedulingFixtureSource))]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	[DontSendEventsAtPersist]
	public abstract class SchedulingScenario : ITestInterceptor, IExtendSystem, IConfigureToggleManager
	{
		protected readonly ResourcePlannerTestParameters ResourcePlannerTestParameters;
		public IIoCTestContext IoCTestContext;

		protected SchedulingScenario(ResourcePlannerTestParameters resourcePlannerTestParameters)
		{
			ResourcePlannerTestParameters = resourcePlannerTestParameters;
		}

		public virtual void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<ResourceCalculateWithNewContext>();
		}
		
		public virtual void OnBefore()
		{
			ResourcePlannerTestParameters.SimulateNewRequest(IoCTestContext);
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			ResourcePlannerTestParameters.EnableToggles(toggleManager);
		}
		
		private class schedulingFixtureSource : ResourcePlannerFixtureSource
		{
			protected override IEnumerable<Toggles> ToggleFlags { get; } = new[]
			{
				Toggles.ResourcePlanner_SeamlessPlanningForPreferences_76288, 
				Toggles.ResourcePlanner_FasterSeamlessPlanningForPreferences_78286
			};
			protected override bool AlsoSimulateSecondRequest { get; } = true;
		}
	}
}