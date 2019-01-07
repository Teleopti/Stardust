using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting
{
	[Serializable]
	public class SiteBankHolidayCalendar : NonversionedAggregateRootWithBusinessUnit, ISiteBankHolidayCalendar
	{
		private ISite _site;
		private ICollection<IBankHolidayCalendar> _bankHolidayCalendarsForSite = new List<IBankHolidayCalendar>();

		public virtual ISite Site
		{
			get { return _site; }
			set { _site = value; }
		}

		public virtual ICollection<IBankHolidayCalendar> BankHolidayCalendarsForSite
		{
			get { return _bankHolidayCalendarsForSite; }
			set { _bankHolidayCalendarsForSite = value; }
		}

		public virtual object Clone()
		{
			return MemberwiseClone();
		}

		public virtual ISiteBankHolidayCalendar NoneEntityClone()
		{
			return (ISiteBankHolidayCalendar)MemberwiseClone();
		}

		public virtual ISiteBankHolidayCalendar EntityClone()
		{
			return (ISiteBankHolidayCalendar)MemberwiseClone();
		}
	}
}
