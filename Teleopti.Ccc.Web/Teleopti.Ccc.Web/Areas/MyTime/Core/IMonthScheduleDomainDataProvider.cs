using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public interface IMonthScheduleDomainDataProvider
	{
		MonthScheduleDomainData Get(DateOnly date, bool loadSeatBooking);
	}
}