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
	    public bool EnableReporting;

		public void Apply(IUnitOfWork uow)
		{
			Scenario = ScenarioFactory.CreateScenario(DefaultName.Make("Common scenario"), true, EnableReporting);
			new ScenarioRepository(uow).Add(Scenario);
		}
	}
}