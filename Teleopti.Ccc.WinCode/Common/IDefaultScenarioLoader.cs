using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
	public interface IDefaultScenarioLoader
	{
		IScenario Load(IScenarioRepository scenarioRepository);
	}
}
