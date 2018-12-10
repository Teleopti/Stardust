using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public interface IWeekScheduleDomainDataProvider
	{
		WeekScheduleDomainData GetWeekSchedule(DateOnly date);
	}
}