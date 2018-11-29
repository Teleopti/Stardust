using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.DaySchedule.Mapping;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public interface IDayScheduleDomainDataProvider
	{
		DayScheduleDomainData GetDaySchedule(DateOnly date);
	}
}