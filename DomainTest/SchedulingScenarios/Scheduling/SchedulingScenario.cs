using System.Collections.Generic;
using System.Linq;
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
		protected readonly PlanTestParameters _planTestParameters;
		public IIoCTestContext IoCTestContext;

		protected SchedulingScenario(PlanTestParameters planTestParameters)
		{
			_planTestParameters = planTestParameters;
		}

		public virtual void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<ResourceCalculateWithNewContext>();
		}
		
		public virtual void OnBefore()
		{
			_planTestParameters.SimulateNewRequest(IoCTestContext);
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			_planTestParameters.EnableToggles(toggleManager);
		}
		
		private class schedulingFixtureSource : PlanFixtureSource
		{
			protected override IEnumerable<Toggles> ToggleFlags { get; } = new[]{Toggles.ResourcePlanner_BetterFitPreferences_76289};
			protected override bool AlsoSimulateSecondRequest { get; } = true;
		}
	}
}