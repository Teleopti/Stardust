using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;


namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferenceViewModel
	{
		public PeriodSelectionViewModel PeriodSelection { get; set; }
		public IEnumerable<WeekDayHeader> WeekDayHeaders { get; set; }		
		public IEnumerable<WeekViewModel> Weeks { get; set; }
		public PreferencePeriodViewModel PreferencePeriod { get; set; }
		public int MaxMustHave { get; set; }
		public int CurrentMustHave { get; set; }
		public bool ExtendedPreferencesPermission { get; set; }

		public PreferenceOptionsViewModel Options { get; set; }
	}

	public class PreferenceOptionsViewModel
	{
		public PreferenceOptionsViewModel(IEnumerable<PreferenceOptionGroup> preferenceOptions, PreferenceOptionGroup activityOptions)
		{
			PreferenceOptions = preferenceOptions;
			ActivityOptions = activityOptions;
		}

		public IEnumerable<PreferenceOptionGroup> PreferenceOptions { get; set; }
		public PreferenceOptionGroup ActivityOptions { get; set; }
	}

	public class PreferencePeriodViewModel
	{
		public string Period { get; set; }
		public string OpenPeriod { get; set; }		
	}

	public class WeekDayHeader
	{
		public string Title { get; set; }
		public DateOnly Date { get; set; }
	}

	public class WeekViewModel
	{
		public IEnumerable<DayViewModel> Days { get; set; }
	}

	public class DayViewModel
	{
		public DateOnly Date { get; set; }
		public bool InPeriod { get; set; }
		public bool Editable { get; set; }
		public HeaderViewModel Header { get; set; }
	}

	public class HeaderViewModel
	{
		public string DayDescription { get; set; }
		public string DayNumber { get; set; }
	}
}