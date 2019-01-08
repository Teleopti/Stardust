using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeSiteBankHolidayCalendarRepository: ISiteBankHolidayCalendarRepository
	{
		private IList<ISiteBankHolidayCalendar> _siteBankHolidayCalendars = new List<ISiteBankHolidayCalendar>();

		public void Add(ISiteBankHolidayCalendar obj)
		{
			_siteBankHolidayCalendars.Add(obj);
		}

		public void Remove(ISiteBankHolidayCalendar root)
		{
			_siteBankHolidayCalendars.Remove(root);
		}

		public ISiteBankHolidayCalendar Get(Guid id)
		{
			return _siteBankHolidayCalendars.FirstOrDefault(s => s.Id == id);
		}

		public ISiteBankHolidayCalendar Load(Guid id)
		{
			return Get(id);
		}

		public IEnumerable<ISiteBankHolidayCalendar> LoadAll()
		{
			return _siteBankHolidayCalendars;
		}

		public IEnumerable<ISiteBankHolidayCalendar> FindAllSiteBankHolidayCalendarsSortedBySite()
		{
			return LoadAll();
		}

		public ISiteBankHolidayCalendar FindSiteBankHolidayCalendar(ISite site)
		{
			return _siteBankHolidayCalendars.FirstOrDefault(siteCalendars =>
				site.Id.GetValueOrDefault() == siteCalendars.Site.Id.GetValueOrDefault());
		}
	}
}
