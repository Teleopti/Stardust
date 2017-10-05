using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[TestFixture(true, true)]
	[TestFixture(false, false)]
	[TestFixture(true, false)]
	[TestFixture(false, true)]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public abstract class SchedulingScenarioForTeamBlock : IConfigureToggleManager
	{
		protected readonly bool ResourcePlannerMergeTeamblockClassicScheduling44289;
		protected readonly bool ResourcePlannerEasierBlockScheduling46155;

		protected SchedulingScenarioForTeamBlock(bool resourcePlannerMergeTeamblockClassicScheduling44289, bool resourcePlannerEasierBlockScheduling46155)
		{
			ResourcePlannerMergeTeamblockClassicScheduling44289 = resourcePlannerMergeTeamblockClassicScheduling44289;
			ResourcePlannerEasierBlockScheduling46155 = resourcePlannerEasierBlockScheduling46155;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if (ResourcePlannerMergeTeamblockClassicScheduling44289)
				toggleManager.Enable(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289);

			if (ResourcePlannerEasierBlockScheduling46155)
				toggleManager.Enable(Toggles.ResourcePlanner_EasierBlockScheduling_46155);
		}
	}
}