namespace Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability
{
	public class StudentAvailabilityPeriodFeedbackViewModel
	{
		public TargetContractTimeViewModel TargetContractTime { get; set; }
	}

	public class TargetContractTimeViewModel
	{
		public double LowerMinutes { get; set; }
		public double UpperMinutes { get; set; }
	}
}