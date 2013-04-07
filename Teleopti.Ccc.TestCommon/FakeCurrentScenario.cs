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
			var scenario = ScenarioFactory.CreateScenarioWithId(" ", true);
			scenario.BusinessUnit = BusinessUnitFactory.CreateWithId(" ");
			_scenario = scenario;
		}

		public IScenario Current()
		{
			return _scenario;
		}
	}
}