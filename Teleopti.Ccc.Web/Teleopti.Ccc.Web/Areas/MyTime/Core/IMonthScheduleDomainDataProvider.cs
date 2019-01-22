using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public interface IMonthScheduleDomainDataProvider
	{
		MonthScheduleDomainData GetMonthData(DateOnly date);
		MonthScheduleDomainData GetMobileMonthData(DateOnly date);
	}
}