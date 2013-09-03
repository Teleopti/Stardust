using System;

namespace Teleopti.Interfaces.Domain
{

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2009-01-21
    /// </remarks>
    public interface IScheduleDayTracker
    {
        /// <summary>
        /// Gets the moved day-off source.
        /// </summary>
        /// <value>The moved from.</value>
        DateOnly MovedDay { get; }

        /// <summary>
        /// Gets the moved day-off target.
        /// </summary>
        /// <value>The moved to.</value>
        DateOnly? MovedTo { get; }
    }
}