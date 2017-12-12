using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[TestFixture(RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, true)]
	[TestFixture(RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, true)]
	[TestFixture(RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, false)]
	[TestFixture(RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, false)]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public abstract class DayOffOptimizationScenario : ISetup, IConfigureToggleManager
	{
		private readonly RemoveImplicitResCalcContext _removeImplicitResCalcContext;
		private readonly bool _resourcePlannerDayOffOptimizationIslands47208;

		protected DayOffOptimizationScenario(RemoveImplicitResCalcContext removeImplicitResCalcContext,
					bool resourcePlannerDayOffOptimizationIslands47208)
		{
			_removeImplicitResCalcContext = removeImplicitResCalcContext;
			_resourcePlannerDayOffOptimizationIslands47208 = resourcePlannerDayOffOptimizationIslands47208;
		}
		
		public virtual void Setup(ISystem system, IIocConfiguration configuration)
		{
			var withDefaultDayOff = new FakeDayOffTemplateRepository();
			withDefaultDayOff.Has(DayOffFactory.CreateDayOff());
			system.UseTestDouble(withDefaultDayOff).For<IDayOffTemplateRepository>();
			system.AddService<ResourceCalculateWithNewContext>();
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_removeImplicitResCalcContext == RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)
				toggleManager.Enable(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680);
			else
				toggleManager.Disable(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680);
			if(_resourcePlannerDayOffOptimizationIslands47208)
				toggleManager.Enable(Toggles.ResourcePlanner_DayOffOptimizationIslands_47208);
		}
	}
}