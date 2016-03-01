using Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public interface IMonthScheduleDomainDataProvider
	{
		MonthScheduleDomainData Get(DateOnly date);
	}
}