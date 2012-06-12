using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public interface IPreferencePeriodFeedbackProvider
	{
		PeriodFeedback PeriodFeedback(DateOnly date);
	}
}