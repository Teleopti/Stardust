using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsDateRepository : IAnalyticsDateRepository
	{
		private readonly List<KeyValuePair<DateOnly, int>> _fakeDates;
		public FakeAnalyticsDateRepository()
		{
			_fakeDates = new List<KeyValuePair<DateOnly, int>>();
			initDates(new DateTime(2000, 1, 1), new DateTime(2010, 1, 1));
		}

		private void initDates(DateTime start, DateTime end)
		{
			_fakeDates.Add(new KeyValuePair<DateOnly, int>(new DateOnly(new DateTime(1900, 01, 01)),-1));
			_fakeDates.Add(new KeyValuePair<DateOnly, int>(new DateOnly(new DateTime(2059, 12, 31)), -2));

			var d = start;
			var dIndex = 0;
			while (d <= end)
			{
				_fakeDates.Add(new KeyValuePair<DateOnly, int>(new DateOnly(d), dIndex));
				d = d.AddDays(1);
				dIndex++;
			}
		}

		public IList<KeyValuePair<DateOnly, int>> Dates()
		{
			return _fakeDates;
		}

		public KeyValuePair<DateOnly, int> Date(DateTime date)
		{
			return _fakeDates.First(a => a.Key == new DateOnly(date));
		}
	}
}