using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor
{
    /// <summary>
    /// Common interface for the viewmodel and the control
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-08-26
    /// </remarks>
    public interface IShiftEditor
    {
        /// <summary>
        /// Loads the schedule part.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-08-31
        /// </remarks>
        void LoadSchedulePart(IScheduleDay schedule);


        /// <summary>
        /// Gets the schedule part.
        /// </summary>
        /// <value>The schedule part.</value>
        IScheduleDay SchedulePart { get; }

        /// <summary>
        /// Gets or sets the interval.
        /// </summary>
        /// <value>The interval.</value>
        TimeSpan Interval { get; set; }

      
    }
}