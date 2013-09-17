using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	public interface IOvertimeAvailabilityPersister
	{
		OvertimeAvailabilityViewModel Persist(OvertimeAvailabilityInput input);
		OvertimeAvailabilityViewModel Delete(DateOnly date);
	}
}