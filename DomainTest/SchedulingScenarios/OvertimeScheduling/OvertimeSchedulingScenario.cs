using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.OvertimeScheduling
{
	[TestFixture(RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)]
	[TestFixture(RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse)]
	public abstract  class OvertimeSchedulingScenario : IConfigureToggleManager
	{
		private readonly RemoveImplicitResCalcContext _removeImplicitResCalcContext;
		
		protected OvertimeSchedulingScenario(RemoveImplicitResCalcContext removeImplicitResCalcContext)
		{
			_removeImplicitResCalcContext = removeImplicitResCalcContext;
			
			if(_removeImplicitResCalcContext == RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)
				Assert.Ignore("46680 - Toggle On");
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_removeImplicitResCalcContext == RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)
				toggleManager.Enable(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680);
			else
				toggleManager.Disable(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680);
		}
	}
}