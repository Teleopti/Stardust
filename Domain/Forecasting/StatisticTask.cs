using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Describes the actual historical data retrieved from the Matrix
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-02-11
    /// </remarks>
    public class StatisticTask: IStatisticTask
    {
        private TimeSpan _statAverageTaskTime;
        private TimeSpan _statAverageAfterTaskTime;
        private TimeSpan _statAverageQueueTime;
        private TimeSpan _statAverageHandleTime;
        private TimeSpan _statAverageTimeToAbandon;
        private TimeSpan _statAverageTimeLongestInQueueAnswered;
        private TimeSpan _statAverageTimeLongestInQueueAbandoned;
        private double _statAverageTaskTimeSeconds;
        private double _statAverageAfterTaskTimeSeconds;

        /// <summary>
        /// Gets or sets the historical statistics of calculated tasks.
        /// </summary>
        /// <value>The stat calculated tasks.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-11
        /// </remarks>
        public virtual double StatCalculatedTasks { get; set; }

        /// <summary>
        /// Gets or sets the historical statistics of abandoned tasks.
        /// </summary>
        /// <value>The stat abandoned tasks.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-11
        /// </remarks>
        public virtual double StatAbandonedTasks { get; set; }

        /// <summary>
        /// Gets or sets the historical statistics of answered tasks.
        /// </summary>
        /// <value>The stat answered tasks.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-11
        /// </remarks>
        public virtual double StatAnsweredTasks { get; set; }

        /// <summary>
        /// Gets the historical statistics of average task time.
        /// </summary>
        /// <value>The stat average task time.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-11
        /// </remarks>
        public virtual TimeSpan StatAverageTaskTime
        {
            get { return _statAverageTaskTime; }
        }

        /// <summary>
        /// Gets the historical statistics of average after task time.
        /// </summary>
        /// <value>The stat average after task time.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-11
        /// </remarks>
        public virtual TimeSpan StatAverageAfterTaskTime
        {
            get { return _statAverageAfterTaskTime; }
        }

        /// <summary>
        /// Gets or sets the stat average task time seconds.
        /// </summary>
        /// <value>The stat average task time seconds.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-15
        /// </remarks>
        public virtual double StatAverageTaskTimeSeconds
        {
            get { return _statAverageTaskTimeSeconds; }
            set
            {
                _statAverageTaskTimeSeconds = value;
                _statAverageTaskTime = TimeSpan.FromSeconds(value);
            }
        }

        /// <summary>
        /// Gets or sets the stat average after task time seconds.
        /// </summary>
        /// <value>The stat average after task time seconds.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-15
        /// </remarks>
        public virtual double StatAverageAfterTaskTimeSeconds
        {
            get { return _statAverageAfterTaskTimeSeconds; }
            set
            {
                _statAverageAfterTaskTimeSeconds = value;
                _statAverageAfterTaskTime = TimeSpan.FromSeconds(value);
            }
        }

        /// <summary>
        /// Gets or sets the interval.
        /// </summary>
        /// <value>The interval.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-15
        /// </remarks>
        public DateTime Interval { get; set; }

        public virtual double StatAbandonedShortTasks { get; set; }

        public virtual double StatAbandonedTasksWithinSL { get; set; }

        public virtual double StatAnsweredTasksWithinSL { get; set; }

        public virtual double StatOverflowOutTasks { get; set; }

        public virtual double StatOverflowInTasks { get; set; }

        public virtual double StatOfferedTasks { get; set; }

        public virtual TimeSpan StatAverageQueueTime
        {
            get { return _statAverageQueueTime; }
        }

        public virtual TimeSpan StatAverageHandleTime
        {
            get { return _statAverageHandleTime; }
        }

        public virtual TimeSpan StatAverageTimeToAbandon
        {
            get { return _statAverageTimeToAbandon; }
        }

        public virtual TimeSpan StatAverageTimeLongestInQueueAnswered
        {
            get { return _statAverageTimeLongestInQueueAnswered; }
        }

        public virtual TimeSpan StatAverageTimeLongestInQueueAbandoned
        {
            get { return _statAverageTimeLongestInQueueAbandoned; }
        }

        public virtual double StatAverageQueueTimeSeconds
        {
            get { return _statAverageQueueTime.TotalSeconds; }
            set 
            { 
                _statAverageQueueTime = TimeSpan.FromSeconds(value);
            }
        }

        public virtual double StatAverageHandleTimeSeconds
        {
            get { return _statAverageHandleTime.TotalSeconds; }
            set
            {
                _statAverageHandleTime = TimeSpan.FromSeconds(value);
            }
        }

        public virtual double StatAverageTimeToAbandonSeconds
        {
            get { return _statAverageTimeToAbandon.TotalSeconds; }
            set
            {
                _statAverageTimeToAbandon = TimeSpan.FromSeconds(value);
            }
        }

        public virtual double StatAverageTimeLongestInQueueAnsweredSeconds
        {
            get { return _statAverageTimeLongestInQueueAnswered.TotalSeconds; }
            set
            {
                _statAverageTimeLongestInQueueAnswered = TimeSpan.FromSeconds(value);
            }
        }

        public virtual double StatAverageTimeLongestInQueueAbandonedSeconds
        {
            get { return _statAverageTimeLongestInQueueAbandoned.TotalSeconds; }
            set
            {
                _statAverageTimeLongestInQueueAbandoned = TimeSpan.FromSeconds(value);
            }
        }

        #region IEquatable<Task> Members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-16
        /// </remarks>
        public bool Equals(IStatisticTask other)
        {
            return other.StatAverageAfterTaskTime == _statAverageAfterTaskTime &&
                   other.StatAverageTaskTime == _statAverageTaskTime &&
                   other.StatCalculatedTasks == StatCalculatedTasks &&
                   other.StatOfferedTasks == StatOfferedTasks &&
                   other.StatAnsweredTasks == StatAnsweredTasks &&
                   other.StatCalculatedTasks == StatCalculatedTasks &&
                   other.Interval == Interval &&
                   other.StatAverageTaskTimeSeconds == _statAverageTaskTimeSeconds &&
                   other.StatAverageAfterTaskTimeSeconds == _statAverageAfterTaskTimeSeconds;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-16
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is StatisticTask))
            {
                return false;
            }
            return Equals((StatisticTask)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-16
        /// </remarks>
        public override int GetHashCode()
        {
            return (GetType().FullName + "|" +
               _statAverageTaskTime + "|" +
                _statAverageAfterTaskTime + "|" +
                StatCalculatedTasks + "|" +
                StatOfferedTasks + "|" +
                StatAnsweredTasks + "|" +
                StatAbandonedTasks + "|" +
                _statAverageTaskTimeSeconds + "|" +
                _statAverageAfterTaskTimeSeconds + "|" +
                Interval).GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="task1">The StatisticTask1.</param>
        /// <param name="task2">The StatisticTask2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-16
        /// </remarks>
        public static bool operator ==(StatisticTask task1, StatisticTask task2)
        {
            return task1.Equals(task2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="task1">The StatisticTask1.</param>
        /// <param name="task2">The StatisticTask2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-16
        /// </remarks>
        public static bool operator !=(StatisticTask task1, StatisticTask task2)
        {
            return !task1.Equals(task2);
        }

        #endregion
    }
}