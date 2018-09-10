using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.TimeBetweenDaysOptimization
{
	[TestFixture(true)]
	[TestFixture(false)]
	[DontSendEventsAtPersist]
	public abstract class TimeBetweenDaysOptimizationScenario : IConfigureToggleManager
	{
		private readonly bool _resourcePlannerXxl76496;

		protected TimeBetweenDaysOptimizationScenario(bool resourcePlannerXXL76496)
		{
			_resourcePlannerXxl76496 = resourcePlannerXXL76496;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerXxl76496)
				toggleManager.Enable(Toggles.ResourcePlanner_XXL_76496);
		}
	}
}