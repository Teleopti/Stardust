namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferencePeriodFeedbackViewModel
	{
		public int PossibleResultDaysOff { get; set; }
		public TargetDaysOffViewModel TargetDaysOff { get; set; }
		public TargetContractTimeViewModel TargetContractTime { get; set; }
		public string PreferencePeriodStart { get; set; }
		public string PreferenceOpenPeriodStart { get; set; }
		public string PreferencePeriodEnd { get; set; }
		public string PreferenceOpenPeriodEnd { get; set; }
	}

	public class TargetContractTimeViewModel
	{
		public double LowerMinutes { get; set; }
		public double UpperMinutes { get; set; }
	}

	public class TargetDaysOffViewModel
	{
		public int Lower { get; set; }
		public int Upper { get; set; }
	}
}