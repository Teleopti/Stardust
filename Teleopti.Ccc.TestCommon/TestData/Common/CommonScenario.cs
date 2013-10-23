using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Common
{
	public class CommonScenario : IDataSetup
	{
		public IScenario Scenario;

		public void Apply(IUnitOfWork uow)
		{
			Scenario = ScenarioFactory.CreateScenarioAggregate(DefaultName.Make("Common scenario"), true);
			new ScenarioRepository(uow).Add(Scenario);
		}
	}
}