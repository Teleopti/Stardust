namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferenceAndScheduleDayViewModel
	{
		public string Date { get; set; }
		public PreferenceDayViewModel Preference { get; set; }
		public DayOffDayViewModel DayOff { get; set; }
		public AbsenceDayViewModel Absence { get; set; }
		public PersonAssignmentDayViewModel PersonAssignment { get; set; }
		public bool? Fulfilled { get; set; }
		public bool Feedback { get; set; }
	}
}