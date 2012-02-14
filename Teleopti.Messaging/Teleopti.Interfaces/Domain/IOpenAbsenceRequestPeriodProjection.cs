using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Creates projection of multiple intersecting IOpenAbsenceRequestPeriod's
    /// </summary>
    /// <remarks>
    /// Created by: HenryG
    /// Created date: 2010-04-16
    /// </remarks>
    public interface IOpenAbsenceRequestPeriodProjection
    {
        /// <summary>
        /// Gets the projected periods.
        /// </summary>
        /// <param name="limitToDateOnlyPeriod">The limit to date only period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-16
        /// </remarks>
        IList<IAbsenceRequestOpenPeriod> GetProjectedPeriods(DateOnlyPeriod limitToDateOnlyPeriod);
    }
}