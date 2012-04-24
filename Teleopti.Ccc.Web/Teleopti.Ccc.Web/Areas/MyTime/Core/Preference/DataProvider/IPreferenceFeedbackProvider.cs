using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public interface IPreferenceFeedbackProvider
	{
		IWorkTimeMinMax WorkTimeMinMaxForDate(DateOnly date, IScheduleDay scheduleDay);
		IWorkTimeMinMax WorkTimeMinMaxForDate(DateOnly date);
	}
}