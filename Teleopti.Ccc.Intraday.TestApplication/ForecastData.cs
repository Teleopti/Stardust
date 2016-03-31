using System.Collections.Generic;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	public class ForecastData
	{
		public IList<ForecastInterval> ForecastIntervals { get; set; }
		public IList<int> Queues { get; set; }
	}
}