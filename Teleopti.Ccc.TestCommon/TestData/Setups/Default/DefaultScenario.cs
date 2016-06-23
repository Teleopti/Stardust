using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Default
{
	public class DefaultScenario
	{
		public static IScenario Scenario = ScenarioFactory.CreateScenario("Default scenario", true, false);

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			new ScenarioRepository(currentUnitOfWork).Add(Scenario);
		}
	}
}