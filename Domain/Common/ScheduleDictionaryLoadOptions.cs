namespace Teleopti.Ccc.Domain.Common
{
	public class ScheduleDictionaryLoadOptions
	{
		public bool LoadRestrictions { get; set; }
		public bool LoadNotes { get; set; }
		public bool LoadOnlyPreferensesAndHourlyAvailability { get; set; }
		public bool LoadDaysAfterLeft { get; set; }
		public bool LoadAgentDayScheduleTags { get; set; }
		public ScheduleDictionaryLoadOptions(bool loadRestrictions, bool loadNotes) : this(loadRestrictions, loadNotes, false)
		{ }

		public ScheduleDictionaryLoadOptions(bool loadRestrictions, bool loadNotes, bool loadOnlyPreferensesAndHourlyAvailability)
		{
			LoadRestrictions = loadRestrictions;
			LoadNotes = loadNotes;
			LoadOnlyPreferensesAndHourlyAvailability = loadOnlyPreferensesAndHourlyAvailability;
			LoadAgentDayScheduleTags = true;
		}
	}
}
