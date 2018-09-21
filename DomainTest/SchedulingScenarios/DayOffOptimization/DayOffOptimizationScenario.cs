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
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, false)]
	[LoggedOnAppDomain]
	[DontSendEventsAtPersist]
	public abstract class DayOffOptimizationScenario : IIsolateSystem, IExtendSystem, IConfigureToggleManager, ITestInterceptor
	{
		private readonly SeperateWebRequest _seperateWebRequest;
		private readonly bool _resourcePlannerXxl76496;

		public IIoCTestContext IoCTestContext;

		protected DayOffOptimizationScenario(SeperateWebRequest seperateWebRequest, 
			bool resourcePlannerXXL76496)
		{
			_seperateWebRequest = seperateWebRequest;
			_resourcePlannerXxl76496 = resourcePlannerXXL76496;
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

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerXxl76496)
				toggleManager.Enable(Toggles.ResourcePlanner_XXL_76496);

			//toggleManager.Enable(Toggles.ResourcePlanner_RespectClosedDaysWhenDoingDOBackToLegal_76348);
		}

		public void OnBefore()
		{
			if (_seperateWebRequest == SeperateWebRequest.SimulateSecondRequestOrScheduler)
				IoCTestContext.SimulateNewRequest();
		}
	}
}