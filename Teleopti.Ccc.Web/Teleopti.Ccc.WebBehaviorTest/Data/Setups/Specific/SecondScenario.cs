using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class SecondScenario : IDataSetup
	{
		public IScenario Scenario;

		public void Apply(IUnitOfWork uow)
		{
			var businessUnit = GlobalDataMaker.Data().Data<SecondBusinessUnit>().BusinessUnit;

			Scenario = ScenarioFactory.CreateScenarioAggregate("Second scenario", false);
			Scenario.SetBusinessUnit(businessUnit);
			new ScenarioRepository(uow).Add(Scenario);
		}
	}
}