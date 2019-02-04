using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar
{
	public class BankHolidayCalendarSite : AggregateRoot_Events_ChangeInfo_BusinessUnit, IBankHolidayCalendarSite
	{
		private ISite _site;

		public virtual ISite Site
		{
			get { return _site; }
			set { _site = value; }
		}

		private IBankHolidayCalendar _calendar;

		public virtual IBankHolidayCalendar Calendar
		{
			get { return _calendar; }
			set { _calendar = value; }
		}
	}
}
