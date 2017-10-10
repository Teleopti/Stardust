using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[TestFixture(true)]
	[TestFixture(false)]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public abstract class SchedulingScenario : IConfigureToggleManager
	{
		protected readonly bool ResourcePlannerEasierBlockScheduling46155;

		protected SchedulingScenario(bool resourcePlannerEasierBlockScheduling46155)
		{
			ResourcePlannerEasierBlockScheduling46155 = resourcePlannerEasierBlockScheduling46155;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(ResourcePlannerEasierBlockScheduling46155)
				toggleManager.Enable(Toggles.ResourcePlanner_EasierBlockScheduling_46155);
		}
	}
}