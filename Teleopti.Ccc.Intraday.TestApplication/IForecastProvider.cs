using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	public interface IForecastProvider
	{
		IList<ForecastInterval> Provide(int workloadId, DateTime fromDate, int fromIntervalId, DateTime toDate, int toIntervalId);
	}
}