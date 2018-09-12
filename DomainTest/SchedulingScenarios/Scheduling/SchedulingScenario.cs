using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, false, false, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, false, true, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, true, false, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, true, true, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, false, false, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, false, true, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, true, false, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, true, true, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, false, false, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, false, true, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, true, false, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, true, true, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, false, false, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, false, true, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, true, false, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, true, true, true)]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	[DontSendEventsAtPersist]
	public abstract class SchedulingScenario : ITestInterceptor, IExtendSystem, IConfigureToggleManager
	{
		private readonly SeperateWebRequest _seperateWebRequest;
		private readonly bool _resourcePlannerXxl76496;
		private readonly bool _resourcePlannerHalfHourSkillTimeZon75509;
		private readonly bool _resourcePlannerReducingSkillsDifferentOpeningHours76176;
		public IIoCTestContext IoCTestContext;

		protected SchedulingScenario(SeperateWebRequest seperateWebRequest, bool resourcePlannerXXL76496, bool resourcePlannerHalfHourSkillTimeZon75509, bool resourcePlannerReducingSkillsDifferentOpeningHours76176)
		{
			_seperateWebRequest = seperateWebRequest;
			_resourcePlannerXxl76496 = resourcePlannerXXL76496;
			_resourcePlannerHalfHourSkillTimeZon75509 = resourcePlannerHalfHourSkillTimeZon75509;
			_resourcePlannerReducingSkillsDifferentOpeningHours76176 = resourcePlannerReducingSkillsDifferentOpeningHours76176;
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
			
			if(_resourcePlannerReducingSkillsDifferentOpeningHours76176)
				toggleManager.Enable(Toggles.ResourcePlanner_ReducingSkillsDifferentOpeningHours_76176);
		}
	}
}