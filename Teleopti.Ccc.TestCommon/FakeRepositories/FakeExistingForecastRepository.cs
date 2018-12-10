using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeExistingForecastRepository : IExistingForecastRepository
	{
		public IEnumerable<SkillMissingForecast> ExistingForecastForAllSkills(DateOnlyPeriod range, IScenario scenario)
		{
			return CustomResult;
		}

		public IEnumerable<SkillMissingForecast> CustomResult { get; set; }
	}
}