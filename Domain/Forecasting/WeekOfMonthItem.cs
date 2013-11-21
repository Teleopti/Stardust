using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Represents a calculated specified week of a month 
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-03-10
    /// </remarks>
    public class WeekOfMonthItem : IPeriodType
    {
        private readonly IVolumeYear _owner;
        private double _taskIndex;
        private double _talkTimeIndex;
        private double _afterTalkTimeIndex;
        private readonly ITaskOwnerPeriod _taskOwnerDays;
        private double _totalTasks;
        private double _averageTasks;
        private TimeSpan _averageTalkTime;
        private TimeSpan _averageAfterWorkTime;

        private readonly double _comparisonTasks;
        private TimeSpan _comparisonAverageTalkTime;
        private TimeSpan _comparisonAverageAfterWorkTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeekOfMonth"/> class.
        /// </summary>
        /// <param name="taskOwnerDays">The task owner days.</param>
        /// <param name="owner">The owner.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-10
        /// </remarks>
        public WeekOfMonthItem(ITaskOwnerPeriod taskOwnerDays, IVolumeYear owner)
        {
            _owner = owner;
            _taskOwnerDays = taskOwnerDays;

            _totalTasks = _taskOwnerDays.TotalStatisticCalculatedTasks;
            _averageTalkTime = _taskOwnerDays.TotalStatisticAverageTaskTime;
            _averageAfterWorkTime = _taskOwnerDays.TotalStatisticAverageAfterTaskTime;
          
            CalculateTaskIndex();
            CalculateTalkTimeIndex(_averageTalkTime, _owner.TalkTime);
            CalculateAfterTalkTimeIndex(_averageAfterWorkTime, _owner.AfterTalkTime);

            //Calculate these from index 1, I mean what are the values calculated from index 1
            if (_taskIndex != 0)
                _comparisonTasks = _totalTasks / _taskIndex * 1;
            if (_talkTimeIndex != 0)
                _comparisonAverageTalkTime = new TimeSpan((long)(_averageTalkTime.Ticks / _talkTimeIndex) * 1);
            if (_afterTalkTimeIndex != 0)
                _comparisonAverageAfterWorkTime = new TimeSpan((long)(_averageAfterWorkTime.Ticks / _afterTalkTimeIndex) * 1); 
        }

        #region Private methods


        /// <summary>
        /// Returns the week index of the month.
        /// </summary>
        /// <param name="dayOfMonth">The day of month.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-10
        /// </remarks>
        public static int WeekIndex(int dayOfMonth)
        {
            if (dayOfMonth < int.MinValue) throw new ArgumentOutOfRangeException("dayOfMonth");
            int remainder;
            return Math.DivRem(dayOfMonth - 1, 7, out remainder) + 1;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the index of the calls.
        /// </summary>
        /// <value>The index of the call.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-06
        /// </remarks>
        public double TaskIndex
        {
            get { return _taskIndex; }
            set
            {
                _taskIndex = value;
                if (_comparisonTasks != 0)
                    _totalTasks = (_comparisonTasks * _taskIndex);
            }
        }

        /// <summary>
        /// Gets the index of the talk time.
        /// </summary>
        /// <value>The index of the talk time.</value>
        /// <remarks>
        /// Created by: ZoeT
        /// Created date: 2008-03-09
        /// </remarks>
        public double TalkTimeIndex
        {
            get { return _talkTimeIndex; }
            set
            {
                _talkTimeIndex = value;
                if (_comparisonAverageTalkTime.TotalSeconds > 1)
                    _averageTalkTime = new TimeSpan((long)(_comparisonAverageTalkTime.Ticks * _talkTimeIndex));
            }
        }

        /// <summary>
        /// Gets the index of the after talk time.
        /// </summary>
        /// <value>The index of the after talk time.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-10
        /// </remarks>
        public double AfterTalkTimeIndex
        {
            get { return _afterTalkTimeIndex; }
            set
            {
                _afterTalkTimeIndex = value;
                if (_comparisonAverageAfterWorkTime.TotalSeconds > 1)
                    _averageAfterWorkTime = new TimeSpan((long)(_comparisonAverageAfterWorkTime.Ticks * _afterTalkTimeIndex));
            }
        }

        /// <summary>
        /// Gets the calculated average tasks for this week.
        /// </summary>
        /// <value>The average tasks.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-10
        /// </remarks>
        public double AverageTasks
        {
            get
            {
                if (_taskOwnerDays.TaskOwnerDayCollection.Count != 0)
                    _averageTasks = _totalTasks / _taskOwnerDays.TaskOwnerDayCollection.Count;
                return _averageTasks * 7;
            }
            set
            {
	            if (!(value >= 0)) return;
	            if (_comparisonTasks != 0 && _averageTasks != 0)
		            _taskIndex = (value/(_averageTasks*7))*_taskIndex;
	            _averageTasks = value/7;
	            _totalTasks = _averageTasks*_taskOwnerDays.TaskOwnerDayCollection.Count;
	            if (_comparisonTasks != 0)
		            _taskIndex = _totalTasks/_comparisonTasks;
            }
        }

        /// <summary>
        /// Gets the daily average tasks.
        /// </summary>
        /// <value>The daily average tasks.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-10
        /// </remarks>
        public double DailyAverageTasks
        {
            get { return _totalTasks / _taskOwnerDays.TaskOwnerDayCollection.Count; }
        }

        /// <summary>
        /// Gets the average talk time.
        /// </summary>
        /// <value>The average talk time.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-10
        /// </remarks>
        public TimeSpan AverageTalkTime
        {
            //TODO:Is this the way it should be calculated?
            get { return _averageTalkTime; }
            set
            {
                if (_comparisonAverageTalkTime.TotalSeconds > 1 && _averageTalkTime.Ticks !=0)
                    _talkTimeIndex = (((double)value.Ticks / _averageTalkTime.Ticks) * _talkTimeIndex);
                _averageTalkTime = value;
                if (_comparisonAverageTalkTime.TotalSeconds > 1)
                    _talkTimeIndex = (double)_averageTalkTime.Ticks / _comparisonAverageTalkTime.Ticks;
            }
        }

        /// <summary>
        /// Gets the average after work time.
        /// </summary>
        /// <value>The average after work time.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-10
        /// </remarks>
        public TimeSpan AverageAfterWorkTime
        {
            //TODO:Is this the way it should be calculated?
            get { return _averageAfterWorkTime; }
            set
            {
                if (_comparisonAverageAfterWorkTime.TotalSeconds > 1 && _averageAfterWorkTime.Ticks!=0)
                    _afterTalkTimeIndex = (((double)value.Ticks / _averageAfterWorkTime.Ticks) * _afterTalkTimeIndex);
                _averageAfterWorkTime = value;
                if (_comparisonAverageAfterWorkTime.TotalSeconds > 1)
                    _afterTalkTimeIndex = (double)_averageAfterWorkTime.Ticks / _comparisonAverageAfterWorkTime.Ticks;
            }
        }
#endregion

        #region Calculate indexes

        private void CalculateTaskIndex()
        {
            double averageTasks = 0d;

            if (_taskOwnerDays.TaskOwnerDayCollection.Count != 0)
                averageTasks = _totalTasks / _taskOwnerDays.TaskOwnerDayCollection.Count;

            double shouldHaveBeenTasks = averageTasks * 7;
            double avg = _owner.AverageTasksPerDay * 7;
            _taskIndex = 1d; //hmm

            if (shouldHaveBeenTasks != 0)
                _taskIndex = shouldHaveBeenTasks / avg;
        }

        private void CalculateTalkTimeIndex(TimeSpan talktTime, TimeSpan talkTimeMonth)
        {
            if (talktTime.TotalSeconds == 0 || talkTimeMonth.TotalSeconds == 0)
                _talkTimeIndex = 1;
            else
                _talkTimeIndex = (talktTime.TotalSeconds / talkTimeMonth.TotalSeconds);
        }

        private void CalculateAfterTalkTimeIndex(TimeSpan afterTalktTime, TimeSpan afterTalkTimeMonth)
        {
            //Index Should be 1 if the aftertalktime is 0 
            if (afterTalktTime.TotalSeconds == 0 || afterTalkTimeMonth.TotalSeconds == 0)
                _afterTalkTimeIndex = 1;
            else
                _afterTalkTimeIndex = (afterTalktTime.TotalSeconds / afterTalkTimeMonth.TotalSeconds);
        }
        #endregion
    }
}
