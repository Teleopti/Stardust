using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IExistingForecastRepository
	{
		IEnumerable<SkillMissingForecast> ExistingForecastForAllSkills(DateOnlyPeriod range, IScenario scenario);
	}

	public class SkillMissingForecast
	{
		public string SkillName { get; set; }
		public Guid SkillId { get; set; }
		public IEnumerable<DateOnlyPeriod> Periods { get; set; }
	}
}