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
	public abstract class DayOffOptimizationScenario : IIsolateSystem, IExtendSystem, IConfigureToggleManager, ITestInterceptor
	{
		private readonly SeperateWebRequest _seperateWebRequest;
		protected readonly bool _resourcePlannerDayOffOptimizationIslands47208;
		private readonly bool _resourcePlannerXxl76496;

		public IIoCTestContext IoCTestContext;

		protected DayOffOptimizationScenario(SeperateWebRequest seperateWebRequest,
			bool resourcePlannerDayOffOptimizationIslands47208, 
			bool resourcePlannerXXL76496)
		{
			_seperateWebRequest = seperateWebRequest;
			_resourcePlannerDayOffOptimizationIslands47208 = resourcePlannerDayOffOptimizationIslands47208;
			_resourcePlannerXxl76496 = resourcePlannerXXL76496;
		}
				
		public virtual void Extend(IExtend extend, IIocConfiguration configuration)
		{
			extend.AddService<ResourceCalculateWithNewContext>();
		}

		public virtual void Isolate(IIsolate isolate)
		{
			var withDefaultDayOff = new FakeDayOffTemplateRepository();
			withDefaultDayOff.Has(DayOffFactory.CreateDayOff());
			isolate.UseTestDouble(withDefaultDayOff).For<IDayOffTemplateRepository>();
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerDayOffOptimizationIslands47208)
				toggleManager.Enable(Toggles.ResourcePlanner_DayOffOptimizationIslands_47208);
			if(_resourcePlannerXxl76496)
				toggleManager.Enable(Toggles.ResourcePlanner_XXL_76496);
		}

		public void OnBefore()
		{
			if (_seperateWebRequest == SeperateWebRequest.SimulateSecondRequestOrScheduler)
				IoCTestContext.SimulateNewRequest();
		}
	}
}