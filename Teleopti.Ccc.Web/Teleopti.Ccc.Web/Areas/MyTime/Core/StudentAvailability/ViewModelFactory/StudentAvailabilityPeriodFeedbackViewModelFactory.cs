using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;


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
					TargetContractTime = new TargetContractTimeViewModel
						{
							LowerMinutes = feedback.TargetTime.StartTime.TotalMinutes,
							UpperMinutes = feedback.TargetTime.EndTime.TotalMinutes
						},
				};
		}
	}
}