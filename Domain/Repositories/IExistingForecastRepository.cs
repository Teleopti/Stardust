using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IExistingForecastRepository
	{
		IEnumerable<Tuple<string,IEnumerable<DateOnlyPeriod>>> ExistingForecastForAllSkills(DateOnlyPeriod range, IScenario scenario);
	}
}