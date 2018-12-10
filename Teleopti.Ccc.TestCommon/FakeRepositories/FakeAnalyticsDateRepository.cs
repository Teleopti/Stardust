using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsDateRepository : IAnalyticsDateRepository
	{
		private readonly List<IAnalyticsDate> _fakeDates;
		public FakeAnalyticsDateRepository() : this(new DateTime(2000, 1, 1), new DateTime(2010, 1, 1))
		{
		}

		public FakeAnalyticsDateRepository(DateTime start, DateTime end, int indexStart = 0)
		{
			_fakeDates = new List<IAnalyticsDate>();
			initDates(start, end, indexStart);
		}

		public void Clear()
		{
			_fakeDates.Clear();
		}

		public void HasDatesBetween(DateTime start, DateTime end, int indexStart = 0)
		{
			_fakeDates.Clear();
			initDates(start, end, indexStart);
		}

		private void initDates(DateTime start, DateTime end, int indexStart)
		{
			if (_fakeDates.All(a => a.DateId != AnalyticsDate.NotDefined.DateId)) _fakeDates.Add(AnalyticsDate.NotDefined);
			if (_fakeDates.All(a => a.DateId != AnalyticsDate.Eternity.DateId)) _fakeDates.Add(AnalyticsDate.Eternity);

			var d = start;
			var dIndex = indexStart;
			while (d <= end)
			{
				_fakeDates.Add(new AnalyticsDate { DateDate = d, DateId = dIndex });
				d = d.AddDays(1);
				dIndex++;
			}
		}

		public IAnalyticsDate MaxDate()
		{
			return _fakeDates.Last(a => a.DateId >= 0);
		}

		public IAnalyticsDate MinDate()
		{
			return _fakeDates.FirstOrDefault(a => a.DateId >= 0);
		}

		public IAnalyticsDate Date(DateTime dateDate)
		{
			if (MinDate() == null || dateDate < MinDate().DateDate)
				return null;
			return _fakeDates.FirstOrDefault(a => new DateOnly(a.DateDate) == new DateOnly(dateDate)) ?? createDates(dateDate);
		}

		private IAnalyticsDate createDates(DateTime dateDate)
		{
			initDates(MaxDate().DateDate.AddDays(1), dateDate, MaxDate().DateId + 1);
			return Date(dateDate);
		}

		public IList<IAnalyticsDate> GetAllPartial()
		{
			return _fakeDates.Where(ad => ad.DateId >= 0).ToList();
		}
	}
}