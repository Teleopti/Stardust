using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider
{
	public interface IStudentAvailabilityPeriodFeedbackProvider
	{
		PeriodFeedback PeriodFeedback(DateOnly date);
	}
}