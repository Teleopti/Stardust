using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSettingWeb;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeBankHolidayCalendarRepository : IBankHolidayCalendarRepository
	{
		private IList<IBankHolidayCalendar> _bankHolidayCalendars = new List<IBankHolidayCalendar>();
		

		public void Add(IBankHolidayCalendar obj)
		{
			if (!obj.Id.HasValue)
				obj.SetId(Guid.NewGuid());
			_bankHolidayCalendars.Add(obj);

		}

		public void Remove(IBankHolidayCalendar root)
		{
			_bankHolidayCalendars.Remove(root);
		}

		public IBankHolidayCalendar Get(Guid id)
		{
			return _bankHolidayCalendars.FirstOrDefault(s => s.Id == id);
		}

		public IBankHolidayCalendar Load(Guid id)
		{
			return _bankHolidayCalendars.FirstOrDefault(s => s.Id == id);
		}

		public IEnumerable<IBankHolidayCalendar> LoadAll()
		{
			return _bankHolidayCalendars.Where(c=>!c.IsDeleted);
		}

		public void Delete(Guid Id)
		{
			var calendar = _bankHolidayCalendars.ToList().Find(c => c.Id.Value == Id);
			calendar.IsDeleted = true;

			calendar.Dates.ToList().ForEach(d=>d.SetDeleted());
		}
	}
}
