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
		protected readonly bool ResourcePlannerEvenRelativeDiff44091;

		protected ResourceCalculationScenario(bool resourcePlannerEvenRelativeDiff44091)
		{
			ResourcePlannerEvenRelativeDiff44091 = resourcePlannerEvenRelativeDiff44091;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(ResourcePlannerEvenRelativeDiff44091)
				toggleManager.Enable(Toggles.ResourcePlanner_EvenRelativeDiff_44091);
		}
	}
}