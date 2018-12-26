using System;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.SystemSettingWeb
{
	public interface IBankHolidayCalendar : IAggregateRoot, IBelongsToBusinessUnit
	{
		string Name { get; set; }
		bool IsDeleted { get; set; }
		ReadOnlyCollection<IBankHolidayDate> Dates { get; }
		void AddDate(IBankHolidayDate dates);
		void DeleteDate(Guid Id);
		void UpdateDate(IBankHolidayDate date);
	}
}
