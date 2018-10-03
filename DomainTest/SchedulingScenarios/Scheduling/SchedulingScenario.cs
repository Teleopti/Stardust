using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest)]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	[DontSendEventsAtPersist]
	public abstract class SchedulingScenario : ITestInterceptor, IExtendSystem, IConfigureToggleManager
	{
		private readonly SeperateWebRequest _seperateWebRequest;
		public IIoCTestContext IoCTestContext;

		protected SchedulingScenario(SeperateWebRequest seperateWebRequest)
		{
			_seperateWebRequest = seperateWebRequest;
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
		}
	}
}