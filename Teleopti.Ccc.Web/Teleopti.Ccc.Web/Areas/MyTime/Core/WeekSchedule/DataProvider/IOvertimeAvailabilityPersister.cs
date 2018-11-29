using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.WeekSchedule;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	public interface IOvertimeAvailabilityPersister
	{
		OvertimeAvailabilityViewModel Persist(OvertimeAvailabilityInput input);
		OvertimeAvailabilityViewModel Delete(DateOnly date);
	}
}