using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Default
{
	public class DefaultScenario
	{
		public static IScenario Scenario = ScenarioFactory.CreateScenario("Default scenario", true, false);

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			ScenarioRepository.DONT_USE_CTOR(currentUnitOfWork).Add(Scenario);
		}
	}
}