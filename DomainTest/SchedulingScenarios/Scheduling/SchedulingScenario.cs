using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[TestFixture(true, true)]
	[TestFixture(false, true)]
	[TestFixture(true, false)]
	[TestFixture(false, false)]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public abstract class SchedulingScenario : IConfigureToggleManager, ITestInterceptor
	{
		protected readonly bool RunInSeperateWebRequest;
		protected readonly bool ResourcePlannerEasierBlockScheduling46155;
		public IIoCTestContext IoCTestContext;

		protected SchedulingScenario(bool runInSeperateWebRequest, bool resourcePlannerEasierBlockScheduling46155)
		{
			RunInSeperateWebRequest = runInSeperateWebRequest;
			ResourcePlannerEasierBlockScheduling46155 = resourcePlannerEasierBlockScheduling46155;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(ResourcePlannerEasierBlockScheduling46155)
				toggleManager.Enable(Toggles.ResourcePlanner_EasierBlockScheduling_46155);
		}

		public void OnBefore()
		{
			if (RunInSeperateWebRequest)
				IoCTestContext.SimulateNewRequest();
		}
	}
}