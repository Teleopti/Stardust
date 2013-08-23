using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// class for meeting to be used in schedule
    /// </summary>
    public interface IPersonMeeting : IAggregateRoot, 
                                        IScheduleData, 
                                        ICloneableEntity<IPersonMeeting>
    {
        /// <summary>
        /// Optional
        /// </summary>
        Boolean Optional { get; }

        /// <summary>
        /// Belongs to Meeting
        /// </summary>
        IMeeting BelongsToMeeting { get; }

        /// <summary>
        /// Toes the layer.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-10-12
        /// </remarks>
        ILayer<IActivity> ToLayer();
    }
}
