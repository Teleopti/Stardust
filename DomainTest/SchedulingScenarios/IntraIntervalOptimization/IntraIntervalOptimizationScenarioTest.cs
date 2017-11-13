using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntraIntervalOptimization
{
	[DomainTest]
	[TestFixture(RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse)]
	[TestFixture(RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)]
	public class IntraIntervalOptimizationScenarioTest : IConfigureToggleManager
	{
		private readonly RemoveImplicitResCalcContext _removeImplicitResCalcContext;

		public IntraIntervalOptimizationScenarioTest(RemoveImplicitResCalcContext removeImplicitResCalcContext)
		{
			_removeImplicitResCalcContext = removeImplicitResCalcContext;
		}
		
		public void Configure(FakeToggleManager toggleManager)
		{
			if(_removeImplicitResCalcContext==RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)
				toggleManager.Enable(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680);
			else
				toggleManager.Disable(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680);
		}
	}
}