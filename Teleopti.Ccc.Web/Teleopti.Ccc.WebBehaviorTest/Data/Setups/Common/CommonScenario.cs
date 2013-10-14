using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common
{
	public class CommonScenario : IDataSetup
	{
		public IScenario Scenario;

		public void Apply(IUnitOfWork uow)
		{
			Scenario = ScenarioFactory.CreateScenarioAggregate(RandomName.Make("Common scenario"), true);
			new ScenarioRepository(uow).Add(Scenario);
		}
	}
}