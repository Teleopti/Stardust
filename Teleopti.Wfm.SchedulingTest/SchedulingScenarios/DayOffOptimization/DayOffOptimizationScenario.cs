using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.DayOffOptimization
{
	[LoggedOnAppDomain]
	[DontSendEventsAtPersist]
	[TestFixtureSource(typeof(dayOffFixtureSource))]
	public abstract class DayOffOptimizationScenario : IIsolateSystem, IExtendSystem, ITestInterceptor, IConfigureToggleManager
	{
		protected readonly ResourcePlannerTestParameters ResourcePlannerTestParameters;
		public IIoCTestContext IoCTestContext;

		protected DayOffOptimizationScenario(ResourcePlannerTestParameters resourcePlannerTestParameters)
		{
			ResourcePlannerTestParameters = resourcePlannerTestParameters;
		}
				
		public virtual void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<ResourceCalculateWithNewContext>();
		}

		public virtual void Isolate(IIsolate isolate)
		{
			var withDefaultDayOff = new FakeDayOffTemplateRepository();
			withDefaultDayOff.Has(DayOffFactory.CreateDayOff());
			isolate.UseTestDouble(withDefaultDayOff).For<IDayOffTemplateRepository>();
		}

		public void OnBefore()
		{
			ResourcePlannerTestParameters.MightSimulateNewRequest(IoCTestContext);
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			ResourcePlannerTestParameters.EnableToggles(toggleManager);
		}

		private class dayOffFixtureSource : ResourcePlannerFixtureSource
		{
			protected override IEnumerable<Toggles> ToggleFlags { get; } = new[]
			{
				Toggles.ResourcePlanner_TeamSchedulingInPlans_79283
			};
			protected override bool AlsoSimulateSecondRequest { get; } = true;
		}
	}
}