using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[TestFixture(true)]
	[TestFixture(false)]
	public abstract class SchedulingScenario : IConfigureToggleManager
	{
		protected readonly bool ResourcePlannerMergeTeamblockClassicScheduling44289;

		protected SchedulingScenario(bool resourcePlannerMergeTeamblockClassicScheduling44289)
		{
			ResourcePlannerMergeTeamblockClassicScheduling44289 = resourcePlannerMergeTeamblockClassicScheduling44289;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(ResourcePlannerMergeTeamblockClassicScheduling44289)
				toggleManager.Enable(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289);
		}
	}
}