
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory
{
	public interface IStudentAvailabilityPeriodFeedbackViewModelFactory
	{
		StudentAvailabilityPeriodFeedbackViewModel CreatePeriodFeedbackViewModel(DateOnly date);
	}
}