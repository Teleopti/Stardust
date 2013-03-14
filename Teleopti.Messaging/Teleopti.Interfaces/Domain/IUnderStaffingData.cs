using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface which shows under staffing data
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "UnderStaffing")]
    public interface IUnderStaffingData
    {
        /// <summary>
        /// list of under staffing and critical under staffing dates.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "UnderStaffing")]
        Dictionary<string, IList<string>> UnderStaffingDates { get; set; }

        /// <summary>
        /// list of under staffing and critical under staffing hours. (only in case if the absence request is only for one day)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "UnderStaffing")]
        Dictionary<string, IList<string>> UnderStaffingHours { get; set; } 

    }
}
