using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    

    /// <summary>
    /// Interface for tracker
    /// Tracker-classes uses different behaviors for calculating IPayloads
    /// </summary>
    /// <remarks>
    /// 2008-11-3 Replaces generic ITracker
    /// </remarks>
    public interface ITracker
    {
        /// <summary>
        /// Gets the name of the Tracker.
        /// </summary>
        /// <value>The name.</value>
        Description Description { get; }

        /// <summary>
        /// Tracks the specified target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="absence">The absence.</param>
        /// <param name="scheduleDays">The schedule days.</param>
        void Track(ITraceable target, IAbsence absence,  IList<IScheduleDay> scheduleDays);

        /// <summary>
        /// Tracks for reset.
        /// </summary>
        /// <param name="absence">The absence.</param>
        /// <param name="scheduleDays">The schedule days.</param>
        /// <returns></returns>
        TimeSpan TrackForReset(IAbsence absence, IList<IScheduleDay> scheduleDays);

        /// <summary>
        /// Creates the person account.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-03-17
        /// </remarks>
        IAccount CreatePersonAccount(DateOnly dateTime);

    }

    /// <summary>
    /// Interface for classes that can be tracked by ITracker
    /// </summary>
    /// <remarks>
    /// NeedsReferesh is used for caching
    /// Created by: henrika
    /// Created date: 2008-11-24
    /// </remarks>
    public interface ITraceable
    {
        /// <summary>
        /// Tracks the number of occasions
        /// </summary>
        /// <param name="timeOrDaysFromLoadedSchedule">The time or days from loaded schedule.</param>
        void Track(TimeSpan timeOrDaysFromLoadedSchedule);
    }
}
