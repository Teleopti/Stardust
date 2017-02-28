using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Common
{
	public class DefaultScenarioFromRepository : ICurrentScenario
	{
		private readonly IScenarioRepository _scenarioRepository;

		public DefaultScenarioFromRepository(IScenarioRepository scenarioRepository)
		{
			_scenarioRepository = scenarioRepository;
		}

		public IScenario Current()
		{
			return _scenarioRepository.LoadDefaultScenario();
		}
	}
}