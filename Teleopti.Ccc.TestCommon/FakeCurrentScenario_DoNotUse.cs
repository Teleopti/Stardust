using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCurrentScenario_DoNotUse : ICurrentScenario
	{
		private IScenario _scenario;

		public FakeCurrentScenario_DoNotUse()
		{
			_scenario = ScenarioFactory.CreateScenarioWithId("scenario", true);
		}

		public void FakeScenario(IScenario scenario)
		{
			_scenario = scenario;
		}

		public IScenario Current()
		{
			return _scenario;
		}
	}
}