using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, true)]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	[DontSendEventsAtPersist]
	public abstract class SchedulingScenario : ITestInterceptor, IExtendSystem, IConfigureToggleManager
	{
		private readonly SeperateWebRequest _seperateWebRequest;
		private readonly bool _resourcePlannerHalfHourSkillTimeZon75509;
		public IIoCTestContext IoCTestContext;

		protected SchedulingScenario(SeperateWebRequest seperateWebRequest, bool resourcePlannerHalfHourSkillTimeZon75509)
		{
			_seperateWebRequest = seperateWebRequest;
			_resourcePlannerHalfHourSkillTimeZon75509 = resourcePlannerHalfHourSkillTimeZon75509;
		}

		public virtual void OnBefore()
		{
			if (_seperateWebRequest == SeperateWebRequest.SimulateSecondRequestOrScheduler)
				IoCTestContext.SimulateNewRequest();
		}	
		
		public virtual void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<ResourceCalculateWithNewContext>();
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerHalfHourSkillTimeZon75509)
				toggleManager.Enable(Toggles.ResourcePlanner_HalfHourSkillTimeZone_75509);
		}
	}
}