using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[TestFixture(true, true, true)]
	[TestFixture(true, false, true)]
	[TestFixture(true, true, false)]
	[TestFixture(true, false, false)]
	[TestFixture(false, true, true)]
	[TestFixture(false, false, true)]
	[TestFixture(false, true, false)]
	[TestFixture(false, false, false)]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public abstract class SchedulingScenario : IConfigureToggleManager, ITestInterceptor
	{
		protected readonly bool RunInSeperateWebRequest;
		protected readonly bool ResourcePlannerEasierBlockScheduling46155;
		protected readonly bool ResourcePlannerRemoveClassicShiftCat46582;
		public IIoCTestContext IoCTestContext;

		protected SchedulingScenario(bool runInSeperateWebRequest, bool resourcePlannerEasierBlockScheduling46155, bool resourcePlannerRemoveClassicShiftCat46582)
		{
			RunInSeperateWebRequest = runInSeperateWebRequest;
			ResourcePlannerEasierBlockScheduling46155 = resourcePlannerEasierBlockScheduling46155;
			ResourcePlannerRemoveClassicShiftCat46582 = resourcePlannerRemoveClassicShiftCat46582;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(ResourcePlannerEasierBlockScheduling46155)
				toggleManager.Enable(Toggles.ResourcePlanner_EasierBlockScheduling_46155);
			if(ResourcePlannerRemoveClassicShiftCat46582)
				toggleManager.Enable(Toggles.ResourcePlanner_RemoveClassicShiftCat_46582);
		}

		public void OnBefore()
		{
			if (RunInSeperateWebRequest)
				IoCTestContext.SimulateNewRequest();
		}
	}
}