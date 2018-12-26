using System;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.SystemSettingWeb
{
	public interface IBankHolidayCalendar : IAggregateRoot, IBelongsToBusinessUnit,IDeleteTag
	{
		string Name { get; set; }
		ReadOnlyCollection<IBankHolidayDate> Dates { get; }
		void AddDate(IBankHolidayDate dates);
		void DeleteDate(Guid Id);
		void UpdateDate(IBankHolidayDate date);
	}
}
