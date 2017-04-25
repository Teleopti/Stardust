using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public interface IWeekScheduleDomainDataProvider
	{
		DayScheduleDomainData GetDaySchedule(DateOnly date);
		WeekScheduleDomainData GetWeekSchedule(DateOnly date);
	}
}