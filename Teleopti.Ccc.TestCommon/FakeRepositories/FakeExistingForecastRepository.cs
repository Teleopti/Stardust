using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeExistingForecastRepository : IExistingForecastRepository
	{
		public IEnumerable<Tuple<string, IEnumerable<DateOnlyPeriod>>> ExistingForecastForAllSkills(DateOnlyPeriod range, IScenario scenario)
		{
			return CustomResult;
		}

		public IEnumerable<Tuple<string, IEnumerable<DateOnlyPeriod>>> CustomResult { get; set; }
	}
}