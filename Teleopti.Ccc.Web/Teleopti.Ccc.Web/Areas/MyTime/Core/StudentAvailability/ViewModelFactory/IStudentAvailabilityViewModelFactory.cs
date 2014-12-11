using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory
{
	public interface IStudentAvailabilityViewModelFactory
	{
		StudentAvailabilityViewModel CreateViewModel(DateOnly dateInPeriod);
		StudentAvailabilityDayFeedbackViewModel CreateDayFeedbackViewModel(DateOnly date);
		StudentAvailabilityDayViewModel CreateDayViewModel(DateOnly date);
		IEnumerable<StudentAvailabilityAndScheduleDayViewModel> CreateStudentAvailabilityAndSchedulesViewModels(DateOnly @from, DateOnly to);
	}

	public class StudentAvailabilityAndScheduleDayViewModel
	{
		public string Date { get; set; }
		public StudentAvailabilityDayViewModel StudentAvailability { get; set; }
	}
}