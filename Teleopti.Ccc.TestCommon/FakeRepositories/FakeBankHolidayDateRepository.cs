using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSettingWeb;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeBankHolidayDateRepository : IBankHolidayDateRepository
	{
		private IList<IBankHolidayDate> _bankHolidayDates = new List<IBankHolidayDate>();

		public void Add(IBankHolidayDate obj)
		{
			if (!obj.Id.HasValue)
			{
				obj.SetId(Guid.NewGuid());
				_bankHolidayDates.Add(obj);
			}
				
			else
			{
				var date = _bankHolidayDates.ToList().Find(_d => _d.Id.Value == obj.Id.Value);
				_bankHolidayDates.Remove(date);
				date.Date = obj.Date;
				date.Description = obj.Description;
				if (obj.IsDeleted)
					date.SetDeleted();
				_bankHolidayDates.Add(date);
			}
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
	}
}
