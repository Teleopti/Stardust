namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferenceDayFeedbackViewModel
	{
		public string Date { get; set; }
		public string PossibleStartTimes { get; set; }
		public string PossibleEndTimes { get; set; }
		public string PossibleContractTimes { get; set; }
		public string FeedbackError { get; set; }
	}
}