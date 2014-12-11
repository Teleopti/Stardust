using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory
{
	public class StudentAvailabilityPeriodFeedbackViewModelFactory : IStudentAvailabilityPeriodFeedbackViewModelFactory
	{
		private readonly IStudentAvailabilityPeriodFeedbackProvider _studentAvailabilityPeriodFeedbackProvider;

		public StudentAvailabilityPeriodFeedbackViewModelFactory(IStudentAvailabilityPeriodFeedbackProvider studentAvailabilityPeriodFeedbackProvider)
		{
			_studentAvailabilityPeriodFeedbackProvider = studentAvailabilityPeriodFeedbackProvider;
		}

		public StudentAvailabilityPeriodFeedbackViewModel CreatePeriodFeedbackViewModel(DateOnly date)
		{
			var feedback = _studentAvailabilityPeriodFeedbackProvider.PeriodFeedback(date);
			return new StudentAvailabilityPeriodFeedbackViewModel
				{
					PossibleResultDaysOff = feedback.PossibleResultDaysOff,
					TargetDaysOff = new TargetDaysOffViewModel
						{
							Lower = feedback.TargetDaysOff.Minimum,
							Upper = feedback.TargetDaysOff.Maximum
						},
					TargetContractTime = new TargetContractTimeViewModel
						{
							LowerMinutes = feedback.TargetTime.Minimum.TotalMinutes,
							UpperMinutes = feedback.TargetTime.Maximum.TotalMinutes
						},
				};
		}
	}
}