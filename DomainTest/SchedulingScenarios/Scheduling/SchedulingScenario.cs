using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[TestFixture(true, true)]
	[TestFixture(false, true)]
	[TestFixture(true, false)]
	[TestFixture(false, false)]
	public abstract class SchedulingScenario : IConfigureToggleManager
	{
		private readonly bool _resourcePlannerTeamBlockPeriod42836;
		protected readonly bool ResourcePlannerMergeTeamblockClassicScheduling44289;

		protected SchedulingScenario(bool resourcePlannerTeamBlockPeriod42836, bool resourcePlannerMergeTeamblockClassicScheduling44289)
		{
			_resourcePlannerTeamBlockPeriod42836 = resourcePlannerTeamBlockPeriod42836;
			ResourcePlannerMergeTeamblockClassicScheduling44289 = resourcePlannerMergeTeamblockClassicScheduling44289;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerTeamBlockPeriod42836)
				toggleManager.Enable(Toggles.ResourcePlanner_TeamBlockPeriod_42836);
			if(ResourcePlannerMergeTeamblockClassicScheduling44289)
				toggleManager.Enable(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289);
		}
	}
}