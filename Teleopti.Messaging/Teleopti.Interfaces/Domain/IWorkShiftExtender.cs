using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// An object of this types creates new shifts based on the template.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-18
    /// </remarks>
    public interface IWorkShiftExtender : IAggregateEntity, ICloneableEntity<IWorkShiftExtender>
    {
        /// <summary>
        /// Gets the activity to extend.
        /// </summary>
        /// <value>The extend with activity.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-18
        /// </remarks>
        IActivity ExtendWithActivity { get; set; }

        /// <summary>
        /// Gets or sets the activity length with segment.
        /// </summary>
        /// <value>The activity length with segment.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-27
        /// </remarks>
        TimePeriodWithSegment ActivityLengthWithSegment { get; set; }

        /// <summary>
        /// Priorities this instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-28
        /// </remarks>
        int? Priority();

        /// <summary>
        /// Gets the maximum, possible length of the activity.
        /// </summary>
        /// <value>The length of the activity.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-18
        /// </remarks>
        TimeSpan ExtendMaximum();

        /// <summary>
        /// Replaces workshifts
        /// </summary>
        /// <param name="shift"></param>
        /// <returns></returns>
        IList<IWorkShift> ReplaceWithNewShifts(IWorkShift shift);
    }
}