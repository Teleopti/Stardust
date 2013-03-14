using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Class that contains under staffing and serious under staffing dates and hours information.
    /// </summary>
    /// 
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "UnderStaffing")]
    public class UnderStaffingData : IUnderStaffingData 
    {
        /// <summary>
        /// Gets and set the list of Dates which have under staffing and serious under staffing.
        /// </summary>
        /// <value>Under staffing and serious understaffing dates dictionary</value>
        public Dictionary<string, IList<string>> UnderStaffingDates { get; set; }

        /// <summary>
        /// Gets and set the list of Hours which have under staffing and serious under staffing.
        /// </summary>
        /// <value>Under staffing and Serious understaffing hours dictionary</value>
        public Dictionary<string, IList<string>> UnderStaffingHours { get; set; }
    }
}
