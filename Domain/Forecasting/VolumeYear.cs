using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public interface IVolumeYear
    {
        /// <summary>
        /// Gets the after talk time.
        /// </summary>
        /// <value>The after talk time.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-12
        /// </remarks>
        TimeSpan AfterTalkTime { get; }

        /// <summary>
        /// Gets the talk time.
        /// </summary>
        /// <value>The talk time.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-12
        /// </remarks>
        TimeSpan TalkTime { get; }

        /// <summary>
        /// Gets the avg calls per day.
        /// </summary>
        /// <value>The avg calls per day.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-06
        /// </remarks>
        double AverageTasksPerDay { get; }

        /// <summary>
        /// Gets the number of days.
        /// </summary>
        /// <value>The number of days.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-06
        /// </remarks>
        int NumberOfDays { get; }

        /// <summary>
        /// Gets the total tasks.
        /// </summary>
        /// <value>The total tasks.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-07
        /// </remarks>
        double TotalTasks { get; }

        /// <summary>
        /// Gets the month of year collection.
        /// </summary>
        /// <value>The month of year collection.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-07
        /// </remarks>
        IDictionary<int, IPeriodType> PeriodTypeCollection { get; }

        /// <summary>
        /// Gets the task owner days.
        /// </summary>
        /// <value>The task owner days.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        IList<ITaskOwner> TaskOwnerDays { get; }

        /// <summary>
        /// Reloads the historical data depth.
        /// </summary>
        /// <param name="taskOwnerPeriod">The owner period containing the workload days.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-12
        /// </remarks>
        void ReloadHistoricalDataDepth(ITaskOwnerPeriod taskOwnerPeriod);

        /// <summary>
        /// Gets the task index for a date
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-31
        /// </remarks>
        double TaskIndex(DateOnly dateTime);

        /// <summary>
        /// Gets the index of the Talktime.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-31
        /// </remarks>
        double TaskTimeIndex(DateOnly dateTime);

        /// <summary>
        /// Gets the index of the aftertalktime.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-31
        /// </remarks>
        double AfterTaskTimeIndex(DateOnly dateTime);
    }

    /// <summary>
    /// Holds a list of MonthOfYears and calculates averages
    /// of calls talktime and after work time
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-03-06
    /// </remarks>
    public abstract class VolumeYear : IVolumeYear
    {
        private ITaskOwnerPeriod _taskOwnerPeriod;
        private readonly IDictionary<int, IPeriodType> _periodTypeCollection = new Dictionary<int, IPeriodType>();
        private double _totalTasks;
        private TimeSpan _talkTime;
        private TimeSpan _afterTalkTime;

        protected VolumeYear(ITaskOwnerPeriod taskOwnerPeriod)
        {
            _taskOwnerPeriod = taskOwnerPeriod;
        }

        /// <summary>
        /// Initializes the period to compare with.
        /// </summary>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-12
        /// </remarks>
        protected void CreateComparisonPeriod()
        {
            _talkTime = _taskOwnerPeriod.TotalStatisticAverageTaskTime;
            _afterTalkTime = _taskOwnerPeriod.TotalStatisticAverageAfterTaskTime;
            _totalTasks = _taskOwnerPeriod.TotalStatisticCalculatedTasks;
        }

        /// <summary>
        /// Gets the after talk time.
        /// </summary>
        /// <value>The after talk time.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-12
        /// </remarks>
        public TimeSpan AfterTalkTime
        {
            get { return _afterTalkTime; }
        }

        /// <summary>
        /// Gets the talk time.
        /// </summary>
        /// <value>The talk time.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-12
        /// </remarks>
        public TimeSpan TalkTime
        {
            get { return _talkTime; }
        }

        #region Properties

        /// <summary>
        /// Gets the avg calls per day.
        /// </summary>
        /// <value>The avg calls per day.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-06
        /// </remarks>
        public double AverageTasksPerDay
        {
            get { return TotalTasks/NumberOfDays; }
        }

        /// <summary>
        /// Gets the number of days.
        /// </summary>
        /// <value>The number of days.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-06
        /// </remarks>
        public int NumberOfDays
        {
            get{return TaskOwnerDays.Count;}
        }

        /// <summary>
        /// Gets the total tasks.
        /// </summary>
        /// <value>The total tasks.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-07
        /// </remarks>
        public double TotalTasks
        {
            get { return _totalTasks; }
        }

        /// <summary>
        /// Gets the month of year collection.
        /// </summary>
        /// <value>The month of year collection.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-07
        /// </remarks>
        public IDictionary<int, IPeriodType> PeriodTypeCollection
        {
            //OrderBy?
            get { return _periodTypeCollection; }
        }

        /// <summary>
        /// Resets the period type collection.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-13
        /// </remarks>
        protected void ResetPeriodTypeCollection()
        {
            _periodTypeCollection.Clear();
        }

        /// <summary>
        /// Gets the task owner days.
        /// </summary>
        /// <value>The task owner days.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public IList<ITaskOwner> TaskOwnerDays
        {
            get { return _taskOwnerPeriod.TaskOwnerDayCollection; }
        }

        /// <summary>
        /// Sets the skill days collection.
        /// </summary>
        /// <param name="taskOwnerDays">The task owner days.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-12
        /// </remarks>
        protected void SetTaskOwnerDaysCollection(ITaskOwnerPeriod taskOwnerDays)
        {
            _taskOwnerPeriod = taskOwnerDays;
        }

        #endregion

        /// <summary>
        /// Reloads the historical data depth.
        /// </summary>
        /// <param name="taskOwnerPeriod">The owner period containing the workload days.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-12
        /// </remarks>
        public abstract void ReloadHistoricalDataDepth(ITaskOwnerPeriod taskOwnerPeriod);

        /// <summary>
        /// Gets the task index for a date
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-31
        /// </remarks>
        public abstract double TaskIndex(DateOnly dateTime);

        /// <summary>
        /// Gets the index of the Talktime.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-31
        /// </remarks>
        public abstract double TaskTimeIndex(DateOnly dateTime);

        /// <summary>
        /// Gets the index of the aftertalktime.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-31
        /// </remarks>
        public abstract double AfterTaskTimeIndex(DateOnly dateTime);
    }
}
