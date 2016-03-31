using System.Collections.Generic;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	public class ForecastProviderFake : IForecastProvider
	{
		public IList<ForecastInterval> Provide(int workloadId)
		{
			return getForecastIntervals();
		}

		private IList<ForecastInterval> getForecastIntervals()
		{
			return new ForecastInterval[]
			{
				new ForecastInterval
				{
					DateId = 1,
					IntervalId = 32,
					Calls = 8.2,
					HandleTime = 1200
				},
				new ForecastInterval
				{
					DateId = 1,
					IntervalId = 33,
					Calls = 10,
					HandleTime = 900
				},
				new ForecastInterval
				{
					DateId = 1,
					IntervalId = 34,
					Calls = 11.8,
					HandleTime = 1100
				},
				new ForecastInterval
				{
					DateId = 1,
					IntervalId = 35,
					Calls = 13,
					HandleTime = 1000
				},
				new ForecastInterval
				{
					DateId = 1,
					IntervalId = 36,
					Calls = 15.1,
					HandleTime = 1100
				},
				new ForecastInterval
				{
					DateId = 1,
					IntervalId = 37,
					Calls = 17.2,
					HandleTime = 1200
				},
				new ForecastInterval
				{
					DateId = 1,
					IntervalId = 38,
					Calls = 19.3,
					HandleTime = 1200
				},
				new ForecastInterval
				{
					DateId = 1,
					IntervalId = 39,
					Calls = 20.6,
					HandleTime = 1100
				},

			};
		}
	}
}