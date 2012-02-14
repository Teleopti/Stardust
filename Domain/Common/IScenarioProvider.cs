using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public interface IScenarioProvider
    {
        IScenario DefaultScenario();
        IScenario DefaultScenario(IBusinessUnit businessUnit);
    }
}