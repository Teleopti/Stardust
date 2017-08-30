using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	public abstract class DayOffOptimizationScenario : ISetup
	{
		public virtual void Setup(ISystem system, IIocConfiguration configuration)
		{
			var withDefaultDayOff = new FakeDayOffTemplateRepository();
			withDefaultDayOff.Has(DayOffFactory.CreateDayOff());
			system.UseTestDouble(withDefaultDayOff).For<IDayOffTemplateRepository>();
		}
	}
}