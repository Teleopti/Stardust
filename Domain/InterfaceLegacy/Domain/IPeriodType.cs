using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Aggregated statistics information for a period
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-03-11
    /// </remarks>
    public interface IPeriodType
    {
        /// <summary>
        /// Gets the index of the task.
        /// </summary>
        /// <value>The index of the task.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-12
        /// </remarks>
        double TaskIndex
        {
            get; set; }

        /// <summary>
        /// Gets the index of the talk time.
        /// </summary>
        /// <value>The index of the talk time.</value>
        /// <remarks>
        /// Created by: ZoeT
        /// Created date: 2008-03-09
        /// </remarks>
        double TalkTimeIndex
        {
            get; set; }

        /// <summary>
        /// Gets the index of the after talk time.
        /// </summary>
        /// <value>The index of the after talk time.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-10
        /// </remarks>
        double AfterTalkTimeIndex
        {
            get; set; }

        /// <summary>
        /// Gets the calculated tasks for this month.
        /// </summary>
        /// <value>The average tasks.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-10
        /// </remarks>
        double AverageTasks
        {
            get; set; }

        /// <summary>
        /// Gets the daily average tasks.
        /// </summary>
        /// <value>The daily average tasks.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-10
        /// </remarks>
        double DailyAverageTasks
        {
            get;
        }

        /// <summary>
        /// Gets the average talk time.
        /// </summary>
        /// <value>The average talk time.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-10
        /// </remarks>
        TimeSpan AverageTalkTime
        {
            //TODO:Is this the way it should be calculated?
            get; set; }

        /// <summary>
        /// Gets the average after work time.
        /// </summary>
        /// <value>The average after work time.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-10
        /// </remarks>
        TimeSpan AverageAfterWorkTime
        {
            //TODO:Is this the way it should be calculated?
            get; set; }
    }
}