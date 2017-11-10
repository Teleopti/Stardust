using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[TestFixture(RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)]
	[TestFixture(RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse)]
	public abstract class DayOffOptimizationScenario : ISetup, IConfigureToggleManager
	{
		private readonly RemoveImplicitResCalcContext _removeImplicitResCalcContext;

		protected DayOffOptimizationScenario(RemoveImplicitResCalcContext removeImplicitResCalcContext)
		{
			_removeImplicitResCalcContext = removeImplicitResCalcContext;
		}
		
		public virtual void Setup(ISystem system, IIocConfiguration configuration)
		{
			var withDefaultDayOff = new FakeDayOffTemplateRepository();
			withDefaultDayOff.Has(DayOffFactory.CreateDayOff());
			system.UseTestDouble(withDefaultDayOff).For<IDayOffTemplateRepository>();
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