using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class ThisCurrentScenario : ICurrentScenario
	{
		private readonly IScenario _scenario;

		public ThisCurrentScenario(IScenario scenario)
		{
			_scenario = scenario;
		}

		public IScenario Current()
		{
			return _scenario;
		}
	}
}