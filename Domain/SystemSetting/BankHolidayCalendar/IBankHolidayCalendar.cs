using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar
{
	public interface IBankHolidayCalendar : IAggregateRoot, IFilterOnBusinessUnit,IDeleteTag
	{
		string Name { get; set; } 
	}
}
