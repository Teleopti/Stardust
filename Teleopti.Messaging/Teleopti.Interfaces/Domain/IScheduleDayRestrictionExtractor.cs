using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Extracts schedule days with restrictions
    /// </summary>
    public interface IScheduleDayRestrictionExtractor
    {
        /// <summary>
        /// All scheduleDays with a restriction
        /// </summary>
        /// <returns></returns>
        IList<IScheduleDay> AllRestrictedDays(IList<IScheduleDay> scheduleDays);
    }
}
