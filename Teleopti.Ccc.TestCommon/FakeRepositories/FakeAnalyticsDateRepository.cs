using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsDateRepository : IAnalyticsDateRepository
	{
		private readonly List<IAnalyticsDate> _fakeDates;
		public FakeAnalyticsDateRepository() : this(new DateTime(2000, 1, 1), new DateTime(2010, 1, 1))
		{
		}

		public FakeAnalyticsDateRepository(DateTime start, DateTime end)
		{
			_fakeDates = new List<IAnalyticsDate>();
			initDates(start, end);
		}

		public void Clear()
		{
			_fakeDates.Clear();
		}

		private void initDates(DateTime start, DateTime end)
		{
			_fakeDates.Add(AnalyticsDate.NotDefined);
			_fakeDates.Add(AnalyticsDate.Eternity);

			var d = start;
			var dIndex = 0;
			while (d <= end)
			{
				_fakeDates.Add(new AnalyticsDate { DateDate = d, DateId = dIndex});
				d = d.AddDays(1);
				dIndex++;
			}
		}

		public IAnalyticsDate MaxDate()
		{
			return _fakeDates.Last();
		}

		public IAnalyticsDate MinDate()
		{
			return _fakeDates.First(a => a.DateId >= 0);
		}

		public IAnalyticsDate Date(DateTime dateDate)
		{
			return _fakeDates.FirstOrDefault(a => new DateOnly(a.DateDate) == new DateOnly(dateDate));
		}

		public IList<IAnalyticsDate> GetAllPartial()
		{
			return _fakeDates.Where(ad => ad.DateId >= 0).ToList();
		}
	}
}