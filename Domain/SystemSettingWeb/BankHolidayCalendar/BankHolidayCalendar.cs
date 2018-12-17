using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy;

namespace Teleopti.Ccc.Domain.SystemSettingWeb.BankHolidayCalendar
{
	public class BankHolidayCalendar: NonversionedAggregateRootWithBusinessUnit, IBankHolidayCalendar
	{
		private string _name;
		private ICollection<DateTime> _dates = new List<DateTime>();
		private bool _isDeleted;

		public virtual string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public virtual ICollection<DateTime> Dates
		{
			get { return _dates; }
			set { _dates = value; }
		}

		public virtual bool IsDeleted
		{
			get { return _isDeleted; }
		}

		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}

		public virtual object Clone()
		{
			throw new NotImplementedException();
		}

		public virtual IBankHolidayCalendar NoneEntityClone()
		{
			throw new NotImplementedException();
		}

		public virtual IBankHolidayCalendar EntityClone()
		{
			throw new NotImplementedException();
		}
	}
}
