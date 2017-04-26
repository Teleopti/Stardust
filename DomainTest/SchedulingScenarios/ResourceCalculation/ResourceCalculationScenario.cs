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
		private readonly bool _resourcePlannerEvenRelativeDiff44091;

		protected ResourceCalculationScenario(bool resourcePlannerEvenRelativeDiff44091)
		{
			_resourcePlannerEvenRelativeDiff44091 = resourcePlannerEvenRelativeDiff44091;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerEvenRelativeDiff44091)
				toggleManager.Enable(Toggles.ResourcePlanner_EvenRelativeDiff_44091);
		}
	}
}