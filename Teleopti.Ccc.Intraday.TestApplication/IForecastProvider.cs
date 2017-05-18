using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	public interface IForecastProvider
	{
		IList<ForecastInterval> Provide(int workloadId, int intervalId, DateTime date);
	}
}