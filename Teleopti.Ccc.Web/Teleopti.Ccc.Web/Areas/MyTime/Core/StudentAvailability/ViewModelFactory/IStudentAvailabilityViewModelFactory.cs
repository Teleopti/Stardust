using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory
{
	public interface IStudentAvailabilityViewModelFactory
	{
		StudentAvailabilityViewModel CreateViewModel(DateOnly dateInPeriod);
		StudentAvailabilityDayFeedbackViewModel CreateDayFeedbackViewModel(DateOnly date);
		StudentAvailabilityDayViewModel CreateDayViewModel(DateOnly date);
		IEnumerable<StudentAvailabilityDayViewModel> CreateStudentAvailabilityAndSchedulesViewModels(DateOnly @from, DateOnly to);
	}
}