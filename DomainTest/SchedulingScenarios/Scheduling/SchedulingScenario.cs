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
		private readonly bool _resourcePlannerTeamBlockPeriod42836;

		protected SchedulingScenario(bool resourcePlannerTeamBlockPeriod42836)
		{
			_resourcePlannerTeamBlockPeriod42836 = resourcePlannerTeamBlockPeriod42836;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerTeamBlockPeriod42836)
				toggleManager.Enable(Toggles.ResourcePlanner_TeamBlockPeriod_42836);
		}
	}
}