using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class TestCurrentScenario : ICurrentScenario
	{
		private readonly IScenario _scenario = new Scenario(" ");

		public IScenario Current()
		{
			return _scenario;
		}
	}
}