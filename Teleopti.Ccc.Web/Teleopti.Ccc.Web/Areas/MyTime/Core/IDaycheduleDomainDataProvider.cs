using Teleopti.Ccc.Web.Areas.MyTime.Core.DaySchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public interface IDayScheduleDomainDataProvider
	{
		DayScheduleDomainData GetDaySchedule(DateOnly date);
	}
}