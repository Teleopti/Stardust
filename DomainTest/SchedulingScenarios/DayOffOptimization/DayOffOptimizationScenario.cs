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
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, true, true, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, false, true, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, true, true, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, false, true, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, true, false, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, false, false, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, true, false, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, false, false, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, true, true, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, false, true, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, true, true, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, false, true, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, true, false, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, false, false, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, true, false, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, false, false, true)]
	[LoggedOnAppDomain]
	public abstract class DayOffOptimizationScenario : IIsolateSystem, IExtendSystem, IConfigureToggleManager, ITestInterceptor
	{
		private readonly SeperateWebRequest _seperateWebRequest;
		protected readonly bool _resourcePlannerDayOffOptimizationIslands47208;
		protected readonly bool ResourcePlannerMinimumAgents75339;
		private readonly bool _resourcePlannerLessResourcesXxl74915;

		public IIoCTestContext IoCTestContext;

		protected DayOffOptimizationScenario(SeperateWebRequest seperateWebRequest,
			bool resourcePlannerDayOffOptimizationIslands47208, 
			bool resourcePlannerMinimumAgents75339,
			bool resourcePlannerLessResourcesXXL74915)
		{
			_seperateWebRequest = seperateWebRequest;
			_resourcePlannerDayOffOptimizationIslands47208 = resourcePlannerDayOffOptimizationIslands47208;
			ResourcePlannerMinimumAgents75339 = resourcePlannerMinimumAgents75339;
			_resourcePlannerLessResourcesXxl74915 = resourcePlannerLessResourcesXXL74915;
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
			if(ResourcePlannerMinimumAgents75339)
				toggleManager.Enable(Toggles.ResourcePlanner_MinimumAgents_75339);
			if(_resourcePlannerLessResourcesXxl74915)
				toggleManager.Enable(Toggles.ResourcePlanner_LessResourcesXXL_74915);
		}

		public void OnBefore()
		{
			if (_seperateWebRequest == SeperateWebRequest.SimulateSecondRequestOrScheduler)
				IoCTestContext.SimulateNewRequest();
		}
	}
}