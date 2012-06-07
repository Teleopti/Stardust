namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferencePeriodFeedbackViewModel
	{
		public int PossibleResultDaysOff { get; set; }
		public TargetDaysOffViewModel TargetDaysOff { get; set; }
		public TargetHoursViewModel TargetHours { get; set; }
	}

	public class TargetHoursViewModel
	{
		public int Lower { get; set; }
		public int Upper { get; set; }
	}

	public class TargetDaysOffViewModel
	{
		public int Lower { get; set; }
		public int Upper { get; set; }
	}
}