using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public interface IStaffingDataAvailablePeriodProvider
	{
		DateOnlyPeriod? GetPeriod(DateOnly date, bool forThisWeek);
	}
}