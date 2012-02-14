using System;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;

namespace Teleopti.Ccc.AgentPortalCode.Common
{
    /// <summary>
    /// Represents a Event Argument for NotifyActivityChanged event
    /// </summary>
    public class ActivityChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the next activity on schedule.
        /// </summary>
        /// <value>The next activity.</value>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-07-26
        /// </remarks>
        public ICustomScheduleAppointment NextScheduleItem { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityChangedEventArgs"/> class.
        /// </summary>
        /// <param name="nextScheduleItem">The next schedule item.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-07-26
        /// </remarks>
        public ActivityChangedEventArgs(ICustomScheduleAppointment nextScheduleItem)
        {
            NextScheduleItem = nextScheduleItem;
        }
    }
}
