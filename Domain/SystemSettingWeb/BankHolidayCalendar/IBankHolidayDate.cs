using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.SystemSettingWeb
{
	public interface IBankHolidayDate : IAggregateRoot,IDeleteTag
	{
		DateTime Date { get; set; }
		IBankHolidayCalendar Calendar { get; set; }
		string Description { get; set; }
	}
}
