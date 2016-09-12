using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsIntervalRepository : IAnalyticsIntervalRepository
	{
		private readonly int _intervalsPerDay;
		private readonly int _maxIntervalId;

		public FakeAnalyticsIntervalRepository() : this(96, 95)
		{
		}

		public FakeAnalyticsIntervalRepository(int intervalsPerDay, int maxIntervalId)
		{
			_intervalsPerDay = intervalsPerDay;
			_maxIntervalId = maxIntervalId;
		}

		public int IntervalsPerDay()
		{
			return _intervalsPerDay;
		}

		public AnalyticsInterval MaxInterval()
		{
			return new AnalyticsInterval {IntervalId = _maxIntervalId };
		}

		public IList<AnalyticsInterval> GetAll()
		{
			var result = new List<AnalyticsInterval>();
			var offset = TimeSpan.FromMinutes(1440/_intervalsPerDay);
			var currentOffset = TimeSpan.FromMinutes(0);
			for (var i = 0; i <= _maxIntervalId; i++)
			{
				result.Add(new AnalyticsInterval
				{
					IntervalId = i,
					IntervalStart = new DateTime(1900, 01, 01) + currentOffset
				});
				currentOffset += offset;
			}
			return result;
		}
	}
}