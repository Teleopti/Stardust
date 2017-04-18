using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
	public interface IDefaultScenarioLoader
	{
		IScenario Load(IScenarioRepository scenarioRepository);
	}
}
