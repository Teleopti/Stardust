using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Merges the validators and process for a list of absence request open periods
    /// </summary>
    /// <remarks>
    /// Created by: henryg
    /// Created date: 2010-04-22
    /// </remarks>
    public interface IAbsenceRequestOpenPeriodMerger
    {
        /// <summary>
        /// Merges the specified absence request open periods.
        /// </summary>
        /// <param name="absenceRequestOpenPeriods">The absence request open periods.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2010-04-22
        /// </remarks>
        IAbsenceRequestOpenPeriod Merge(IEnumerable<IAbsenceRequestOpenPeriod> absenceRequestOpenPeriods);
    }
}