using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, false, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, false, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, true, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, true, false)]

	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, false, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, false, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, true, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, true, true)]

	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public abstract class SchedulingScenario : IConfigureToggleManager, ITestInterceptor, ISetup
	{
		private readonly SeperateWebRequest _seperateWebRequest;
		private readonly bool _resourcePlannerNoPytteIslands47500;
		protected readonly bool _resourcePlannerXxl47258;

		public IIoCTestContext IoCTestContext;

		protected SchedulingScenario(SeperateWebRequest seperateWebRequest, 
			bool resourcePlannerNoPytteIslands47500,
			bool resourcePlannerXxl47258)
		{
			_seperateWebRequest = seperateWebRequest;
			_resourcePlannerNoPytteIslands47500 = resourcePlannerNoPytteIslands47500;
			_resourcePlannerXxl47258 = resourcePlannerXxl47258;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if (_resourcePlannerXxl47258)
				toggleManager.Enable(Toggles.ResourcePlanner_XXL_47258);
			if(_resourcePlannerNoPytteIslands47500)
				toggleManager.Enable(Toggles.ResourcePlanner_NoPytteIslands_47500);
		}

		public virtual void OnBefore()
		{
			if (_seperateWebRequest == SeperateWebRequest.SimulateSecondRequestOrScheduler)
				IoCTestContext.SimulateNewRequest();
		}

		public virtual void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<ResourceCalculateWithNewContext>();
		}
	}
}