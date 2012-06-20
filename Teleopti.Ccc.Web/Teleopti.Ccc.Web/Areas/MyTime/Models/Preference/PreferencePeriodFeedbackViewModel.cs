namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferencePeriodFeedbackViewModel
	{
		public int PossibleResultDaysOff { get; set; }
		public TargetDaysOffViewModel TargetDaysOff { get; set; }
		public TargetContractTimeViewModel TargetContractTime { get; set; }
	}

	public class TargetContractTimeViewModel
	{
		public string Lower { get; set; }
		public string Upper { get; set; }
	}

	public class TargetDaysOffViewModel
	{
		public int Lower { get; set; }
		public int Upper { get; set; }
	}
}