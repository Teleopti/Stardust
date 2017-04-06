using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation
{
	public interface IMissingForecastProvider
	{
		IEnumerable<MissingForecastModel> GetMissingForecast(DateOnlyPeriod range);
		IEnumerable<MissingForecastModel> GetMissingForecast(ICollection<IPerson> people, DateOnlyPeriod range);
	}
}