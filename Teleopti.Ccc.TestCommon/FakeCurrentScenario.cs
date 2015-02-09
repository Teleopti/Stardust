using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCurrentScenario : ICurrentScenario
	{
		private readonly IScenario _scenario;

		public FakeCurrentScenario()
		{
			_scenario = ScenarioFactory.CreateScenarioWithId("scenario", true);
		}

		public IScenario Current()
		{
			return _scenario;
		}
	}
}