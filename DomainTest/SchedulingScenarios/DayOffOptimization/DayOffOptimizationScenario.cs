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
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, true)]
	[LoggedOnAppDomain]
	[DontSendEventsAtPersist]
	public abstract class DayOffOptimizationScenario : IIsolateSystem, IExtendSystem, ITestInterceptor, IConfigureToggleManager
	{
		private readonly SeperateWebRequest _seperateWebRequest;
		protected readonly bool _resourcePlannerNoWhiteSpotWhenTargetDayoffIsBroken77941;

		public IIoCTestContext IoCTestContext;

		protected DayOffOptimizationScenario(SeperateWebRequest seperateWebRequest, bool resourcePlannerNoWhiteSpotWhenTargetDayoffIsBroken77941)
		{
			_seperateWebRequest = seperateWebRequest;
			_resourcePlannerNoWhiteSpotWhenTargetDayoffIsBroken77941 = resourcePlannerNoWhiteSpotWhenTargetDayoffIsBroken77941;
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
			if (_seperateWebRequest == SeperateWebRequest.SimulateSecondRequestOrScheduler)
				IoCTestContext.SimulateNewRequest();
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerNoWhiteSpotWhenTargetDayoffIsBroken77941)
				toggleManager.Enable(Toggles.ResourcePlanner_NoWhiteSpotWhenTargetDayoffIsBroken_77941);
		}
	}
}