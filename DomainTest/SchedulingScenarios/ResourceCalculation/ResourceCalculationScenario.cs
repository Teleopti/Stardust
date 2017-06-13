using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[TestFixture(true)]
	[TestFixture(false)]
	public abstract class ResourceCalculationScenario : IConfigureToggleManager
	{
		protected readonly bool _resourcePlannerRespectSkillGroupShoveling44156;

		protected ResourceCalculationScenario(bool resourcePlannerRespectSkillGroupShoveling44156)
		{
			_resourcePlannerRespectSkillGroupShoveling44156 = resourcePlannerRespectSkillGroupShoveling44156;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerRespectSkillGroupShoveling44156)
				toggleManager.Enable(Toggles.ResourcePlanner_RespectSkillGroupShoveling_44156);
		}
	}
}