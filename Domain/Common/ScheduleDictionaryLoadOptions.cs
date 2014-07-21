using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class ScheduleDictionaryLoadOptions : IScheduleDictionaryLoadOptions
    {
        public bool LoadRestrictions { get; set; }
        public bool LoadNotes { get; set; }
        public bool LoadOnlyPreferensesAndHourlyAvailability { get; set; }
	    public bool LoadDaysAfterLeft { get; set; }

        public ScheduleDictionaryLoadOptions(bool loadRestrictions, bool loadNotes):this(loadRestrictions,loadNotes, false)
        {}

		public ScheduleDictionaryLoadOptions(bool loadRestrictions, bool loadNotes, bool loadOnlyPreferensesAndHourlyAvailabilit)
		{
			LoadRestrictions = loadRestrictions;
			LoadNotes = loadNotes;
			LoadOnlyPreferensesAndHourlyAvailability = loadOnlyPreferensesAndHourlyAvailabilit;
		} 
    }
}
