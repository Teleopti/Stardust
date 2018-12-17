using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.InterfaceLegacy
{
	public interface IBankHolidayCalendar : IAggregateRoot, IDeleteTag, ICloneableEntity<IBankHolidayCalendar>
	{
		string Name { get; set; }
		ICollection<DateTime> Dates { get; set; }
	}
}
