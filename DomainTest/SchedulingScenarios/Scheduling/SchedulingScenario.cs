﻿using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[TestFixture(true, true)]
	[TestFixture(true, false)]
	[TestFixture(false, true)]
	[TestFixture(false, false)]
	public abstract class SchedulingScenario : IConfigureToggleManager
	{
		protected readonly bool ResourcePlannerMergeTeamblockClassicScheduling44289;
		private readonly bool _resourcePlannerSchedulingIslands44757;

		protected SchedulingScenario(bool resourcePlannerMergeTeamblockClassicScheduling44289, bool resourcePlannerSchedulingIslands44757)
		{
			ResourcePlannerMergeTeamblockClassicScheduling44289 = resourcePlannerMergeTeamblockClassicScheduling44289;
			_resourcePlannerSchedulingIslands44757 = resourcePlannerSchedulingIslands44757;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(ResourcePlannerMergeTeamblockClassicScheduling44289)
				toggleManager.Enable(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289);
			if (_resourcePlannerSchedulingIslands44757)
				toggleManager.Enable(Toggles.ResourcePlanner_SchedulingIslands_44757);
		}
	}
}