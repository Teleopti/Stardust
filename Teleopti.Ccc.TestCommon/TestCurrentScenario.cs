using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class TestCurrentScenario : ICurrentScenario
	{
		private readonly IScenario _scenario;

		public TestCurrentScenario()
		{
			_scenario = ScenarioFactory.CreateScenarioWithId(" ", true);
		}
		public IScenario Current()
		{
			return _scenario;
		}
	}
}