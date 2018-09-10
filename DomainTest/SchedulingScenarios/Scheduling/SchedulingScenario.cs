using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, false, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, false, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, true, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, true, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, false, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, false, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, true, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, true, true)]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	[DontSendEventsAtPersist]
	public abstract class SchedulingScenario : ITestInterceptor, IExtendSystem, IConfigureToggleManager
	{
		private readonly SeperateWebRequest _seperateWebRequest;
		private readonly bool _resourcePlannerXxl76496;
		private readonly bool _resourcePlannerHalfHourSkillTimeZon75509;
		public IIoCTestContext IoCTestContext;

		protected SchedulingScenario(SeperateWebRequest seperateWebRequest, bool resourcePlannerXXL76496, bool resourcePlannerHalfHourSkillTimeZon75509)
		{
			_seperateWebRequest = seperateWebRequest;
			_resourcePlannerXxl76496 = resourcePlannerXXL76496;
			_resourcePlannerHalfHourSkillTimeZon75509 = resourcePlannerHalfHourSkillTimeZon75509;
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

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerXxl76496)
				toggleManager.Enable(Toggles.ResourcePlanner_XXL_76496);

			if(_resourcePlannerHalfHourSkillTimeZon75509)
				toggleManager.Enable(Toggles.ResourcePlanner_HalfHourSkillTimeZone_75509);
		}
	}
}