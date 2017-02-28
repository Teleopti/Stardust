﻿namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Load options for ScheduleDictionary
    /// </summary>
    public interface IScheduleDictionaryLoadOptions
    {
        /// <summary>
        /// LoadRestrictions
        /// </summary>
        bool LoadRestrictions { get; set; }
        /// <summary>
        /// LoadNotes
        /// </summary>
        bool LoadNotes { get; set; }

		bool LoadOnlyPreferensesAndHourlyAvailability { get; set; }

	    bool LoadDaysAfterLeft { get; set; }
		bool LoadAgentDayScheduleTags { get; set; }
	}
}
