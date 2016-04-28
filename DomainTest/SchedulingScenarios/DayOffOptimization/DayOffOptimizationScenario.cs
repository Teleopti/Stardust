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
		protected DayOffOptimizationScenario(bool teamBlockDayOffForIndividuals)
		{
			TeamBlockDayOffForIndividuals = teamBlockDayOffForIndividuals;
		}

		protected bool TeamBlockDayOffForIndividuals { get; private set; }

		public void Configure(FakeToggleManager toggleManager)
		{
			if (TeamBlockDayOffForIndividuals)
			{
				toggleManager.Enable(Toggles.ResourcePlanner_TeamBlockDayOffForIndividuals_37998);
			}
			else
			{
				toggleManager.Disable(Toggles.ResourcePlanner_IntradayIslands_36939);
			}
		}

		public virtual void Setup(ISystem system, IIocConfiguration configuration)
		{
			var withDefaultDayOff = new FakeDayOffTemplateRepository();
			withDefaultDayOff.Has(DayOffFactory.CreateDayOff());
			system.UseTestDouble(withDefaultDayOff).For<IDayOffTemplateRepository>(); ;
		}
	}
}