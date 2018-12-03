using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Data in a schedule
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2009-09-30
    /// </remarks>
    public interface IScheduleData : IScheduleParameters, ICloneable
    {
        /// <summary>
        /// Determines whether [is part of period] [the specified period].
        /// </summary>
        /// <param name="dateAndPeriod">The date and period.</param>
        /// <returns>
        /// 	<c>true</c> if [is part of period] [the specified period]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-09-16
        /// </remarks>
        bool BelongsToPeriod(IDateOnlyAsDateTimePeriod dateAndPeriod);

        /// <summary>
        /// Belongses to period.
        /// </summary>
        /// <param name="dateOnlyPeriod">The date only period.</param>
        /// <returns></returns>
        bool BelongsToPeriod(DateOnlyPeriod dateOnlyPeriod);

        /// <summary>
        /// Determines whether this schedule data belongs to a specific scenario
        /// </summary>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-12-10
        /// </remarks>
        bool BelongsToScenario(IScenario scenario);
    }
}
