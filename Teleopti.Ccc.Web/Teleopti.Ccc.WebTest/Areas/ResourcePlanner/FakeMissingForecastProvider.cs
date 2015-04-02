using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class FakeMissingForecastProvider : IMissingForecastProvider
	{
		private readonly MissingForecastModel[] _missingForecast;

		public FakeMissingForecastProvider(params MissingForecastModel[] missingForecast)
		{
			_missingForecast = missingForecast;
		}

		public IEnumerable<MissingForecastModel> GetMissingForecast(DateOnlyPeriod range)
		{
			return _missingForecast;
		}
	}
}