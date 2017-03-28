using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferenceDayFeedbackViewModel
	{
		public string Date { get; set; }

		public DateTime DateInternal { get; set; }

		public string PossibleStartTimes { get; set; }
		public string PossibleEndTimes { get; set; }
		public string PossibleContractTimeMinutesLower { get; set; }
		public string PossibleContractTimeMinutesUpper { get; set; }
		public string FeedbackError { get; set; }

		public bool HasNightRestViolationToPreviousDay { get; set; }
		public bool HasNightRestViolationToNextDay { get; set; }

		public string ExpectedNightRestTimeSpan { get; set; }
		public string RestTimeToNextDayTimeSpan { get; set; }
		public string RestTimeToPreviousDayTimeSpan { get; set; }
	}
}