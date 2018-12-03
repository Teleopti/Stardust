
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory
{
	public interface IStudentAvailabilityPeriodFeedbackViewModelFactory
	{
		StudentAvailabilityPeriodFeedbackViewModel CreatePeriodFeedbackViewModel(DateOnly date);
	}
}