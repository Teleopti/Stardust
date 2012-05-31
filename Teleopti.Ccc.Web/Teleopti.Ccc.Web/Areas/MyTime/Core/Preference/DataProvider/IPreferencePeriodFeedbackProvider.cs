using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public interface IPreferencePeriodFeedbackProvider
	{
		MinMax<int> TargetDaysOff(DateOnly date);
		int PossibleResultDaysOff(DateOnly date);
	}
}