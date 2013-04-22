using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

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