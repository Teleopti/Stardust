using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[TestFixture(true, true, false)]
	[TestFixture(true, false, false)]
	[TestFixture(false, true, false)]
	[TestFixture(false, false, false)]
	[TestFixture(true, true, true)]
	[TestFixture(true, false, true)]
	[TestFixture(false, true, true)]
	[TestFixture(false, false, true)]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public abstract class SchedulingScenario : IConfigureToggleManager
	{
		protected readonly bool ResourcePlannerMergeTeamblockClassicScheduling44289;
		protected readonly bool _resourcePlannerSchedulingIslands44757;
		private readonly bool _resourcePlannerSchedulingFewerResourceCalculations45429;

		protected SchedulingScenario(bool resourcePlannerMergeTeamblockClassicScheduling44289, bool resourcePlannerSchedulingIslands44757, bool resourcePlannerSchedulingFewerResourceCalculations45429)
		{
			ResourcePlannerMergeTeamblockClassicScheduling44289 = resourcePlannerMergeTeamblockClassicScheduling44289;
			_resourcePlannerSchedulingIslands44757 = resourcePlannerSchedulingIslands44757;
			_resourcePlannerSchedulingFewerResourceCalculations45429 = resourcePlannerSchedulingFewerResourceCalculations45429;
			if (resourcePlannerSchedulingFewerResourceCalculations45429)
				Assert.Ignore("Try to fix later -> 45429");
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(ResourcePlannerMergeTeamblockClassicScheduling44289)
				toggleManager.Enable(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289);
			if (_resourcePlannerSchedulingIslands44757)
				toggleManager.Enable(Toggles.ResourcePlanner_SchedulingIslands_44757);
			if(_resourcePlannerSchedulingFewerResourceCalculations45429)
				toggleManager.Enable(Toggles.ResourcePlanner_SchedulingFewerResourceCalculations_45429);
		}
	}
}