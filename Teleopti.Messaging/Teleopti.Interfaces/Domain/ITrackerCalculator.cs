using System;
using System.Collections.Generic;


namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Handles the calculating for the tracker
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2008-11-12
    /// </remarks>
    public interface ITrackerCalculator
    {

        /// <summary>
        /// Calculates the total time of a payload for a period
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="scheduleDays"></param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-11-27
        /// </remarks>
        TimeSpan CalculateTotalTimeOnScheduleDays(IPayload payload, IList<IScheduleDay> scheduleDays);

        /// <summary>
        /// Calculates the occurances of payload days on a part (where the definition of day is CalculateIfCountsAsDay)
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="scheduleDays"></param>
        /// <returns></returns>
        TimeSpan CalculateNumberOfDaysOnScheduleDays(IPayload payload, IList<IScheduleDay> scheduleDays);

    }
}
