using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	public interface IOvertimeAvailabilityPersister
	{
		OvertimeAvailabilityViewModel Persist(OvertimeAvailabilityInput input);
	}
}