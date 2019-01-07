using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting
{
	public class BankHolidayDate : AggregateRoot, IBankHolidayDate, IAggregateRoot
	{
		private DateOnly _date;
		private string _description;
		private IBankHolidayCalendar _calendar;
		private bool _isDeleted;

		public virtual DateOnly Date
		{
			get { return _date; }
			set { _date = value; }
		}

		public virtual string Description
		{
			get { return _description; }
			set { _description = value; }
		}
		
		public virtual bool IsDeleted => _isDeleted;

		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}
		
		public virtual IBankHolidayCalendar Calendar
		{
			get
			{
				return _calendar;
			}
			set
			{
				_calendar = value;
			}
		}

	}
}
