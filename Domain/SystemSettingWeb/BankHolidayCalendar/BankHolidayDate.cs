using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.SystemSettingWeb
{
	public class BankHolidayDate : NonversionedAggregateRootWithBusinessUnit, IBankHolidayDate, IAggregateRootWithEvents
	{
		private DateTime _date;

		public virtual DateTime Date
		{
			get { return _date; }
			set { _date = value; }
		}

		private string _description;

		public virtual string Description
		{
			get { return _description; }
			set { _description = value; }
		}
		

		private bool _isDeleted;

		public virtual bool IsDeleted
		{
			get { return _isDeleted; }
		}
		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}

		private IBankHolidayCalendar _calendar;
		public virtual IBankHolidayCalendar Calendar
		{
			get
			{
				return _calendar;
			}
			set
			{
				value.AddDate(this);
				_calendar = value;
			}
		}

	}
}
