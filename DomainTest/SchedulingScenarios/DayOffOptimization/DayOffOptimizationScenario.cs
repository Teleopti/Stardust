using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	public abstract class DayOffOptimizationScenario : IConfigureToggleManager, ISetup
	{
		private readonly bool _teamBlockDayOffForIndividuals;

		protected DayOffOptimizationScenario(bool teamBlockDayOffForIndividuals)
		{
			_teamBlockDayOffForIndividuals = teamBlockDayOffForIndividuals;
		}

		public virtual void Configure(FakeToggleManager toggleManager)
		{
			if (_teamBlockDayOffForIndividuals)
			{
				toggleManager.Enable(Toggles.ResourcePlanner_TeamBlockDayOffForIndividuals_37998);
			}
			else
			{
				toggleManager.Disable(Toggles.ResourcePlanner_TeamBlockDayOffForIndividuals_37998);
			}
		}

		public virtual void Setup(ISystem system, IIocConfiguration configuration)
		{
			var withDefaultDayOff = new FakeDayOffTemplateRepository();
			withDefaultDayOff.Has(DayOffFactory.CreateDayOff());
			system.UseTestDouble(withDefaultDayOff).For<IDayOffTemplateRepository>();
		}
	}
}