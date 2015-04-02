using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public interface IMissingForecastProvider
	{
		IEnumerable<MissingForecastModel> GetMissingForecast(DateOnlyPeriod range);
	}
}