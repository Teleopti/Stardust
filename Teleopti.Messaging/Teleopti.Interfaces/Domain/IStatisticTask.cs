using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Describes the actual historical data retrieved from the Matrix
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-02-11
    /// </remarks>
    public interface IStatisticTask : IEquatable<IStatisticTask>
    {
        /// <summary>
        /// Gets or sets the historical statistics of calculated tasks.
        /// </summary>
        /// <value>The stat calculated tasks.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-11
        /// </remarks>
        double StatCalculatedTasks { get; set; }

        /// <summary>
        /// Gets or sets the historical statistics of abandoned tasks.
        /// </summary>
        /// <value>The stat abandoned tasks.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-11
        /// </remarks>
        double StatAbandonedTasks { get; set; }

        /// <summary>
        /// Gets or sets the historical statistics of answered tasks.
        /// </summary>
        /// <value>The stat answered tasks.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-11
        /// </remarks>
        double StatAnsweredTasks { get; set; }

        /// <summary>
        /// Gets or sets the historical statistics of offered tasks.
        /// </summary>
        /// <value>The stat offered tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-07-02
        /// </remarks>
        double StatOfferedTasks { get; set; }

        /// <summary>
        /// Gets the historical statistics of average task time.
        /// </summary>
        /// <value>The stat average task time.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-11
        /// </remarks>
        TimeSpan StatAverageTaskTime
        {
            get;
        }

        /// <summary>
        /// Gets the historical statistics of average after task time.
        /// </summary>
        /// <value>The stat average after task time.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-11
        /// </remarks>
        TimeSpan StatAverageAfterTaskTime
        {
            get;
        }

        /// <summary>
        /// Gets or sets the stat average task time seconds.
        /// </summary>
        /// <value>The stat average task time seconds.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-15
        /// </remarks>
        double StatAverageTaskTimeSeconds { get; set; }

        /// <summary>
        /// Gets or sets the stat average after task time seconds.
        /// </summary>
        /// <value>The stat average after task time seconds.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-15
        /// </remarks>
        double StatAverageAfterTaskTimeSeconds { get; set; }

        /// <summary>
        /// Gets or sets the interval.
        /// </summary>
        /// <value>The interval.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-15
        /// </remarks>
        DateTime Interval { get; set; }

        /// <summary>
        /// Gets or sets the stat abandoned short tasks.
        /// </summary>
        /// <value>The stat abandoned short tasks.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-09-16
        /// </remarks>
        double StatAbandonedShortTasks { get; set; }

        /// <summary>
        /// Gets or sets the stat abandoned tasks within sl.
        /// </summary>
        /// <value>The stat abandoned tasks within sl.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-09-16
        /// </remarks>
        double StatAbandonedTasksWithinSL { get; set; }

        /// <summary>
        /// Gets or sets the stat answered tasks within sl.
        /// </summary>
        /// <value>The stat answered tasks within sl.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-09-16
        /// </remarks>
        double StatAnsweredTasksWithinSL { get; set; }

        /// <summary>
        /// Gets or sets the stat overflow out tasks.
        /// </summary>
        /// <value>The stat overflow out tasks.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-09-16
        /// </remarks>
        double StatOverflowOutTasks { get; set; }

        /// <summary>
        /// Gets or sets the stat overflow in tasks.
        /// </summary>
        /// <value>The stat overflow in tasks.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-09-16
        /// </remarks>
        double StatOverflowInTasks { get; set; }

        /// <summary>
        /// Gets the stat average queue time.
        /// </summary>
        /// <value>The stat average queue time.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-09-16
        /// </remarks>
        TimeSpan StatAverageQueueTime { get; }

        /// <summary>
        /// Gets the stat average handle time.
        /// </summary>
        /// <value>The stat average handle time.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-09-16
        /// </remarks>
        TimeSpan StatAverageHandleTime { get; }

        /// <summary>
        /// Gets the stat average time to abandon.
        /// </summary>
        /// <value>The stat average time to abandon.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-09-16
        /// </remarks>
        TimeSpan StatAverageTimeToAbandon { get; }

        /// <summary>
        /// Gets the stat average time longest in queue answered.
        /// </summary>
        /// <value>The stat average time longest in queue answered.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-09-16
        /// </remarks>
        TimeSpan StatAverageTimeLongestInQueueAnswered { get; }

        /// <summary>
        /// Gets the stat average time longest in queue abandoned.
        /// </summary>
        /// <value>The stat average time longest in queue abandoned.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-09-16
        /// </remarks>
        TimeSpan StatAverageTimeLongestInQueueAbandoned { get; }

        /// <summary>
        /// Gets or sets the stat average queue time seconds.
        /// </summary>
        /// <value>The stat average queue time seconds.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-09-16
        /// </remarks>
        double StatAverageQueueTimeSeconds { get; set; }

        /// <summary>
        /// Gets or sets the stat average handle time seconds.
        /// </summary>
        /// <value>The stat average handle time seconds.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-09-16
        /// </remarks>
        double StatAverageHandleTimeSeconds { get; set; }

        /// <summary>
        /// Gets or sets the stat average time to abandon seconds.
        /// </summary>
        /// <value>The stat average time to abandon seconds.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-09-16
        /// </remarks>
        double StatAverageTimeToAbandonSeconds { get; set; }

        /// <summary>
        /// Gets or sets the stat average time longest in queue answered seconds.
        /// </summary>
        /// <value>The stat average time longest in queue answered seconds.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-09-16
        /// </remarks>
        double StatAverageTimeLongestInQueueAnsweredSeconds { get; set; }

        /// <summary>
        /// Gets or sets the stat average time longest in queue abandoned seconds.
        /// </summary>
        /// <value>The stat average time longest in queue abandoned seconds.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-09-16
        /// </remarks>
        double StatAverageTimeLongestInQueueAbandonedSeconds { get; set; }
    }
}
