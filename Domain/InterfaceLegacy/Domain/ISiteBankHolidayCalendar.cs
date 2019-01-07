using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISiteBankHolidayCalendar : IAggregateRoot, IBelongsToBusinessUnit, ICloneableEntity<ISiteBankHolidayCalendar>
	{
		ISite Site { get; set; }
		ICollection<IBankHolidayCalendar> BankHolidayCalendarsForSite { get; set; }
	}
}
