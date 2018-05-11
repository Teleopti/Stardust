using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, true)]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public abstract class SchedulingScenario : ITestInterceptor, IIsolateSystem, IExtendSystem, IConfigureToggleManager
	{
		private readonly SeperateWebRequest _seperateWebRequest;
		private readonly bool _resourcePlannerLessResourcesXxl74915;
		public IIoCTestContext IoCTestContext;

		protected SchedulingScenario(SeperateWebRequest seperateWebRequest, bool resourcePlannerLessResourcesXXL74915)
		{
			_seperateWebRequest = seperateWebRequest;
			_resourcePlannerLessResourcesXxl74915 = resourcePlannerLessResourcesXXL74915;
		}

		public virtual void OnBefore()
		{
			if (_seperateWebRequest == SeperateWebRequest.SimulateSecondRequestOrScheduler)
				IoCTestContext.SimulateNewRequest();
		}	
		
		public virtual void Extend(IExtend extend, IIocConfiguration configuration)
		{
			extend.AddService<ResourceCalculateWithNewContext>();
		}

		public virtual void Isolate(IIsolate isolate)
		{
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerLessResourcesXxl74915)
				toggleManager.Enable(Toggles.ResourcePlanner_LessResourcesXXL_74915);
		}
	}
}