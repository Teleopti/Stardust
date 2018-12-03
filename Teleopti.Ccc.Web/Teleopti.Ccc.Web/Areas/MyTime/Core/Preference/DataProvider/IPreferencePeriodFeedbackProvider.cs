

using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public interface IPreferencePeriodFeedbackProvider
	{
		PeriodFeedback PeriodFeedback(DateOnly date);
	}
}