using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar
{
	public interface IBankHolidayDate : IAggregateRoot,IDeleteTag
	{
		DateOnly Date { get; set; }
		IBankHolidayCalendar Calendar { get; set; }
		string Description { get; set; }
		void Active();
	}
}
