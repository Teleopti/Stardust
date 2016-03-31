using System.Collections.Generic;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	public interface IForecastProvider
	{
		IDictionary<int, ForecastData> Provide();
	}
}