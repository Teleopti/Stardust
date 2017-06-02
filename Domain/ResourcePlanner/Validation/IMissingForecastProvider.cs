using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public interface IMissingForecastProvider
	{
		IEnumerable<MissingForecastModel> GetMissingForecast(DateOnlyPeriod range, IEnumerable<SkillMissingForecast> existingForecast);
		IEnumerable<MissingForecastModel> GetMissingForecast(ICollection<IPerson> people, DateOnlyPeriod range, IEnumerable<SkillMissingForecast> existingForecast);
	}
}