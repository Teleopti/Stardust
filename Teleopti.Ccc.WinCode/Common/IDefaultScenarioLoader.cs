using System;
using System.Collections.Generic;
using System.Text;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
	public interface IDefaultScenarioLoader
	{
		IScenario Load(IScenarioRepository scenarioRepository);
	}
}
