using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public interface IWeekScheduleDomainDataProvider
	{
		WeekScheduleDomainData GetWeekSchedule(DateOnly date);
	}
}