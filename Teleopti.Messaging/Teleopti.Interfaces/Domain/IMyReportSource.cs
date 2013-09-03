using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// IMyReportSource interface
    /// </summary>
    /// <remarks>
    /// Created by:VirajS
    /// Created date: 11/24/2008
    /// </remarks>
    public interface IMyReportSource
    {
        /// <summary>
        /// Gets or sets the agent.
        /// </summary>
        /// <value>The agent.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        IPerson Agent { get; set; }

        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        DateTimePeriod Period { get; set; }

        /// <summary>
        /// Gets or sets the schedules.
        /// </summary>
        /// <value>The schedules.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        ReadOnlyCollection<IMyReportScheduleInfo> Schedules { get; }

        /// <summary>
        /// Gets or sets the log on info.
        /// </summary>
        /// <value>The log on info.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        ReadOnlyCollection<IMyReportLogOnInfo> LogOnInfo { get; }


        /// <summary>
        /// Adds the schedule.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/25/2008
        /// </remarks>
        void AddSchedule(IMyReportScheduleInfo schedule);

        /// <summary>
        /// Adds the schedule.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/25/2008
        /// </remarks>
        void AddSchedule(IEnumerable<IMyReportScheduleInfo> collection);

        /// <summary>
        /// Adds the log on info.
        /// </summary>
        /// <param name="logOnInfo">The log on info.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/25/2008
        /// </remarks>
        void AddLogOnInfo(IMyReportLogOnInfo logOnInfo);

        /// <summary>
        /// Adds the log on info.
        /// </summary>
        /// <param name="logOnInfoCollection">The log on info collection.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/25/2008
        /// </remarks>
        void AddLogOnInfo(IEnumerable<IMyReportLogOnInfo> logOnInfoCollection);

    }
}
