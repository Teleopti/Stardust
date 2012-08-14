using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class SecondScenario : IDataSetup
	{
		public IScenario Scenario;

		public void Apply(IUnitOfWork uow)
		{
			var businessUnit = GlobalDataContext.Data().Data<SecondBusinessUnit>().BusinessUnit;

			Scenario = ScenarioFactory.CreateScenarioAggregate("Second scenario", true, false);
			Scenario.SetBusinessUnit(businessUnit);
			new ScenarioRepository(uow).Add(Scenario);
		}
	}
}