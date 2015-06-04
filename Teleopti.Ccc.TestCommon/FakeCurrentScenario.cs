using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCurrentScenario : ICurrentScenario
	{
		private IScenario _scenario;

		public FakeCurrentScenario()
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