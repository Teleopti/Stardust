using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeBankHolidayCalendarSiteRepository : IBankHolidayCalendarSiteRepository
	{
		private IList<IBankHolidayCalendarSite> _bankHolidayCalendarSites = new List<IBankHolidayCalendarSite>();
		

		public void Add(IBankHolidayCalendarSite obj)
		{
			if (!obj.Id.HasValue)
				obj.SetId(Guid.NewGuid());
			_bankHolidayCalendarSites.Add(obj);
		}

		public void Remove(IBankHolidayCalendarSite root)
		{
			_bankHolidayCalendarSites.Remove(root);
		}

		public IBankHolidayCalendarSite Get(Guid id)
		{
			return _bankHolidayCalendarSites.FirstOrDefault(s => s.Id == id);
		}

		public IBankHolidayCalendarSite Load(Guid id)
		{
			return _bankHolidayCalendarSites.FirstOrDefault(s => s.Id == id);
		}

		public IEnumerable<IBankHolidayCalendarSite> LoadAll()
		{
			return _bankHolidayCalendarSites;
		}

		public IEnumerable<System.Guid> FindSitesByCalendar(Guid calendarId)
		{
			return _bankHolidayCalendarSites.Where(s => s.Calendar.Id.Value == calendarId).Select(c=>c.Site.Id.Value);
		}
	}
}
