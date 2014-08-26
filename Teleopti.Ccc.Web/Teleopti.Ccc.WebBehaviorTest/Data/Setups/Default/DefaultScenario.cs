using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default
{
	public class DefaultScenario : IHashableDataSetup
	{
		public readonly IScenario Scenario = ScenarioFactory.CreateScenario("Default scenario", true, false);

		public void Apply(IUnitOfWork uow)
		{
			new ScenarioRepository(uow).Add(Scenario);
		}

		public int HashValue()
		{
			return Scenario.Description.Name.GetHashCode();
		}
	}
}