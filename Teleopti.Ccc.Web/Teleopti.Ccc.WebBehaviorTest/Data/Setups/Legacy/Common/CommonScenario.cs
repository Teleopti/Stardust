using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common
{
	public class CommonScenario : IDataSetup
	{
		public IScenario Scenario;

		public void Apply(IUnitOfWork uow)
		{
			Scenario = ScenarioFactory.CreateScenario(RandomName.Make("Common scenario"), true, false);
			new ScenarioRepository(uow).Add(Scenario);
		}
	}
}