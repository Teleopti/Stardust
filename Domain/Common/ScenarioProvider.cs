using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class ScenarioProvider : IScenarioProvider
    {
        private readonly IScenarioRepository _scenarioRepository;

        public ScenarioProvider(IScenarioRepository scenarioRepository)
        {
            _scenarioRepository = scenarioRepository;
        }

        public IScenario DefaultScenario()
        {
            return _scenarioRepository.LoadDefaultScenario();
        }

        public IScenario DefaultScenario(IBusinessUnit businessUnit)
        {
            return _scenarioRepository.LoadDefaultScenario(businessUnit);
        }
    }
}