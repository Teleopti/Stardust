using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
	public class DefaultScenarioLoader : IDefaultScenarioLoader
	{
		public IScenario Load(IScenarioRepository scenarioRepository)
		{
			if(scenarioRepository == null)
				throw new ArgumentNullException("scenarioRepository");

			return scenarioRepository.LoadDefaultScenario();
		}
	}
}
