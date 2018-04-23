using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, true, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, false, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, true, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, false, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, true, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, false, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, true, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, false, false)]
	[LoggedOnAppDomain]
	public abstract class DayOffOptimizationScenario : ISetup, IConfigureToggleManager, ITestInterceptor
	{
		private readonly SeperateWebRequest _seperateWebRequest;
		protected readonly bool _resourcePlannerDayOffOptimizationIslands47208;
		private readonly bool _resourcePlannerDayOffUsePredictorEverywhere75667;

		public IIoCTestContext IoCTestContext;

		protected DayOffOptimizationScenario(SeperateWebRequest seperateWebRequest,
			bool resourcePlannerDayOffOptimizationIslands47208, bool resourcePlannerDayOffUsePredictorEverywhere75667) //TODO
		{
			_seperateWebRequest = seperateWebRequest;
			_resourcePlannerDayOffOptimizationIslands47208 = resourcePlannerDayOffOptimizationIslands47208;
			_resourcePlannerDayOffUsePredictorEverywhere75667 = resourcePlannerDayOffUsePredictorEverywhere75667;
		}
		
		public virtual void Setup(ISystem system, IIocConfiguration configuration)
		{
			var withDefaultDayOff = new FakeDayOffTemplateRepository();
			withDefaultDayOff.Has(DayOffFactory.CreateDayOff());
			system.UseTestDouble(withDefaultDayOff).For<IDayOffTemplateRepository>();
			system.AddService<ResourceCalculateWithNewContext>();
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerDayOffOptimizationIslands47208)
				toggleManager.Enable(Toggles.ResourcePlanner_DayOffOptimizationIslands_47208);
			if(_resourcePlannerDayOffUsePredictorEverywhere75667)
				toggleManager.Enable(Toggles.ResourcePlanner_DayOffUsePredictorEverywhere_75667);
		}

		public void OnBefore()
		{
			if (_seperateWebRequest == SeperateWebRequest.SimulateSecondRequestOrScheduler)
				IoCTestContext.SimulateNewRequest();
		}
	}
}