using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Holds a SkillDay with indices
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2008-04-01
    /// </remarks>
    public class TotalDayItem
    {
        private double _taskIndex;
        private double _talkTimeIndex;
        private double _afterTalkTimeIndex;
        private double _comparisonTasks;
        private TimeSpan _comparisonAverageTalkTime;
        private TimeSpan _comparisonAverageAfterWorkTime;
		private IForecastingTarget _taskOwner;
        private double _dayTrendFactor;
        

        /// <summary>
        /// Sets the comparison values.
        /// </summary>
        /// <param name="taskOwner">The task owner.</param>
        /// <param name="taskIndex">Index of the task.</param>
        /// <param name="talkTimeIndex">Index of the talk time.</param>
        /// <param name="afterTalkTimeIndex">Index of the after talk time.</param>
        /// <param name="dayTrendFactor">The day trend factor.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-11-03
        /// </remarks>
        public void SetComparisonValues(IForecastingTarget taskOwner, double taskIndex, double talkTimeIndex, double afterTalkTimeIndex, double dayTrendFactor)
        {
            //Count comparison values to index 1
            if (taskIndex != 0 && taskOwner.Tasks != 0)
                _comparisonTasks = (taskOwner.Tasks / taskIndex);

            if (talkTimeIndex != 0 && taskOwner.AverageTaskTime.Ticks!=0)
                _comparisonAverageTalkTime = new TimeSpan((long)(taskOwner.AverageTaskTime.Ticks / talkTimeIndex));

            if (afterTalkTimeIndex != 0 && taskOwner.AverageAfterTaskTime.Ticks!=0)
                _comparisonAverageAfterWorkTime = new TimeSpan((long)(taskOwner.AverageAfterTaskTime.Ticks / afterTalkTimeIndex));

            
            _dayTrendFactor = dayTrendFactor;
            _taskOwner = taskOwner;
            TaskIndex = taskIndex * dayTrendFactor;
            _talkTimeIndex = talkTimeIndex;
            _afterTalkTimeIndex = afterTalkTimeIndex;
            
        }

        /// <summary>
        /// Gets the index of the task.
        /// </summary>
        /// <value>The index of the task.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-31
        /// </remarks>
        public double TaskIndex
        {
            get { return _taskIndex; }
            set
            {
	            if (value < 0) return;
	            if (!_taskOwner.OpenForWork.IsOpenForIncomingWork) return;
	            _taskIndex = value;
	            _taskOwner.Tasks = _taskIndex*_comparisonTasks;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [work load day is closed].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [work load day is closed]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-04-10
        /// </remarks>
        public bool WorkloadDayIsClosed
        {
            //get { return _taskOwner.IsClosed.IsOpenForIncomingWork; }
            get { return !_taskOwner.OpenForWork.IsOpen; }  // need to test it .
        }

        /// <summary>
        /// Gets the current date.
        /// </summary>
        /// <value>The current date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-21
        /// </remarks>
        public DateOnly CurrentDate
        {
            get { return _taskOwner.CurrentDate; }
        }

        /// <summary>
        /// Gets the index of the talk time.
        /// </summary>
        /// <value>The index of the talk time.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-31
        /// </remarks>
        public double TalkTimeIndex
        {
            get { return _talkTimeIndex; }
            set
            {
	            if (value < 0) return;
	            if (!_taskOwner.OpenForWork.IsOpenForIncomingWork) return;
	            _talkTimeIndex = value;
	            _taskOwner.AverageTaskTime =
		            new TimeSpan((long) (_talkTimeIndex*_comparisonAverageTalkTime.Ticks));
            }
        }

        /// <summary>
        /// Gets the index of the after talk time.
        /// </summary>
        /// <value>The index of the after talk time.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-31
        /// </remarks>
        public double AfterTalkTimeIndex
        {
            get { return _afterTalkTimeIndex; }
            set
            {
	            if (value < 0) return;
	            if (!_taskOwner.OpenForWork.IsOpenForIncomingWork) return;
	            _afterTalkTimeIndex = value;
	            _taskOwner.AverageAfterTaskTime =
		            new TimeSpan((long) (_afterTalkTimeIndex*_comparisonAverageAfterWorkTime.Ticks));
            }
        }

        /// <summary>
        /// Gets or sets the tasks.
        /// </summary>
        /// <value>The tasks.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-04-01
        /// </remarks>
        public double Tasks
        {
            get { return _taskOwner.Tasks; }
            set
            {
                if (_taskOwner.OpenForWork.IsOpenForIncomingWork)
                {
                    _taskOwner.Tasks = value;
                    if (_comparisonTasks != 0)
                        _taskIndex = value/_comparisonTasks;
                }
            }
        }

        /// <summary>
        /// Gets or sets the talk time.
        /// </summary>
        /// <value>The talk time.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-04-01
        /// </remarks>
        public TimeSpan TalkTime
        {
            get { return _taskOwner.AverageTaskTime; }
            set
            {
                if (_taskOwner.OpenForWork.IsOpenForIncomingWork)
                {
                    _taskOwner.AverageTaskTime = value;
                    if (_comparisonAverageTalkTime.Ticks != 0)
                        _talkTimeIndex = ((double) value.Ticks/_comparisonAverageTalkTime.Ticks);
                }
            }
        }

        /// <summary>
        /// Gets or sets the after talk time.
        /// </summary>
        /// <value>The after talk time.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-04-01
        /// </remarks>
        public TimeSpan AfterTalkTime
        {
            get { return _taskOwner.AverageAfterTaskTime; }
            set
            {
                if (_taskOwner.OpenForWork.IsOpenForIncomingWork)
                {
                    _taskOwner.AverageAfterTaskTime = value;
                    if (_comparisonAverageAfterWorkTime.Ticks != 0)
                        _afterTalkTimeIndex = ((double) value.Ticks/_comparisonAverageAfterWorkTime.Ticks);
                }
            }
        }

        public IForecastingTarget TaskOwner
        {
            get { return _taskOwner; }
        }

        public double DayTrendFactor
        {
            get { return _dayTrendFactor; }
        }
    }
}
