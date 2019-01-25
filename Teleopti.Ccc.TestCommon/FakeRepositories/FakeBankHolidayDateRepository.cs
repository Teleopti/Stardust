using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeBankHolidayDateRepository : IBankHolidayDateRepository
	{
		private IList<IBankHolidayDate> _bankHolidayDates = new List<IBankHolidayDate>();

		public void Add(IBankHolidayDate obj)
		{
			if (!obj.Id.HasValue)
				obj.SetId(Guid.NewGuid());
			_bankHolidayDates.Add(obj);
		}

		public IBankHolidayDate Find(DateOnly date, IBankHolidayCalendar calendar)
		{
			return _bankHolidayDates.FirstOrDefault(s => s.Date == date && s.Calendar.Id.Value == calendar.Id.Value);
		}

		public IBankHolidayDate Get(Guid id)
		{
			return _bankHolidayDates.FirstOrDefault(s => s.Id == id);
		}

		public IBankHolidayDate Load(Guid id)
		{
			return _bankHolidayDates.FirstOrDefault(s => s.Id == id);
		}

		public IEnumerable<IBankHolidayDate> LoadAll()
		{
			return _bankHolidayDates.Where(d => !d.IsDeleted);
		}

		public void Remove(IBankHolidayDate root)
		{
			_bankHolidayDates.Remove(root);
		}

		public IEnumerable<IBankHolidayDate> FetchByCalendarsAndPeriod(IEnumerable<IBankHolidayCalendar> calendars, DateOnlyPeriod period)
		{
			return _bankHolidayDates.Where(d => d.Date >= period.StartDate && d.Date <= period.EndDate && calendars.Contains(d.Calendar));
		}
	}
}
