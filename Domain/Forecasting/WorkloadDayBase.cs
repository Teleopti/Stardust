using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using TimePeriod = Teleopti.Ccc.Domain.InterfaceLegacy.Domain.TimePeriod;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Class for holding one day of workload data
    /// </summary>
    public abstract class WorkloadDayBase : AggregateEntity, IWorkloadDayBase
    {
        private IWorkload _workload;
        private double _tasks;
        private double _totalTasks;
        private double _totalStatisticCalculatedTasks;
        private double _totalStatisticAnsweredTasks;
        private double _totalStatisticAbandonedTasks;
        private TimeSpan _averageTaskTime;
        private TimeSpan _averageAfterTaskTime;
        private TimeSpan _totalAverageTaskTime;
        private TimeSpan _totalAverageAfterTaskTime;
        private TimeSpan _totalStatisticAverageTaskTime;
        private TimeSpan _totalStatisticAverageAfterTaskTime;
        private Percent _campaignTasks;
        private Percent _campaignTaskTime;
        private Percent _campaignAfterTaskTime;
		private ISet<TimePeriod> _openHourList = new HashSet<TimePeriod>();
		private ISet<ITemplateTaskPeriod> _taskPeriodList = new HashSet<ITemplateTaskPeriod>();
        private bool _turnOffInternalRecalc;
        private bool _initialized;
        private bool _isDirty;
        private IList<ITaskOwner> _parents = new List<ITaskOwner>();
        private DateOnly _currentDate;
        private IQueueStatisticsProvider _queueStatisticsProvider;
	    private bool _useSkewedDistribution;

	    /// <summary>
        /// Creates the specified workload day
        /// </summary>
        /// <param name="workloadDate">The workload date.</param>
        /// <param name="workload">The workload.</param>
        /// <param name="openHourList">The open hour list.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-13
        /// </remarks>
        public virtual void Create(DateOnly workloadDate, IWorkload workload, IList<TimePeriod> openHourList)
        {
            InParameter.NotNull(nameof(workload), workload);
            InParameter.NotNull(nameof(openHourList), openHourList);

            _workload = workload;
            _currentDate = workloadDate;

            //Reset the day
            Close();
            ChangeOpenHours(openHourList);
		}

		public virtual void SetUseSkewedDistribution(bool value)
	    {
		    _useSkewedDistribution = value;
	    }

        protected bool IsEmailWorkload => _workload.Skill.SkillType.ForecastSource != ForecastSource.InboundTelephony &&
										  _workload.Skill.SkillType.ForecastSource != ForecastSource.Retail &&
										  _workload.Skill.SkillType.ForecastSource != ForecastSource.Chat;

		protected void EntityCloneTaskPeriodList(WorkloadDayBase targetClone)
        {
            targetClone._taskPeriodList = new HashSet<ITemplateTaskPeriod>(_taskPeriodList.Select(templateTaskPeriod => {
				var clonedTaskPeriod = templateTaskPeriod.EntityClone();
				clonedTaskPeriod.SetParent(targetClone);
				return clonedTaskPeriod;
			}));
			targetClone._openHourList = new HashSet<TimePeriod>(_openHourList.ToArray());
        }

        public virtual void CloneTaskPeriodListFrom(WorkloadDayBase targetClone)
        {
            if (targetClone == null)
                return;
            _taskPeriodList.Clear();
            var timeZone = Workload.Skill.TimeZone;
            var localDate = CurrentDate;
            foreach (ITemplateTaskPeriod templateTaskPeriod in targetClone._taskPeriodList)
            {
                ITask task = templateTaskPeriod.Task;
                DateTime startDateTime = localDate.Date.Add(templateTaskPeriod.Period.TimePeriod(timeZone).StartTime);
                DateTime endDateTime = localDate.Date.Add(templateTaskPeriod.Period.TimePeriod(timeZone).EndTime);
                DateTimePeriod dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                                                    startDateTime,
                                                    endDateTime,
                                                    timeZone);
                ITemplateTaskPeriod clonedTaskPeriod = new TemplateTaskPeriod(task, dateTimePeriod);
                clonedTaskPeriod.SetParent(this);
                _taskPeriodList.Add(clonedTaskPeriod);
            }
        }

        protected void NoneEntityCloneTaskPeriodList(WorkloadDayBase targetClone)
        {
            targetClone._taskPeriodList = new HashSet<ITemplateTaskPeriod>(_taskPeriodList.Select(templateTaskPeriod =>
			{
				ITemplateTaskPeriod clonedTaskPeriod = templateTaskPeriod.NoneEntityClone();
				clonedTaskPeriod.SetParent(targetClone);
				return clonedTaskPeriod;
			}));
            
			targetClone._openHourList = new HashSet<TimePeriod>(_openHourList.ToArray());
        }

        /// <summary>
        /// Adds an OpenHourPeriod spanning the complete date.
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 11/28/2007
        /// </remarks>
        public virtual void MakeOpen24Hours()
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            TimeSpan startTimeSpan = _workload.Skill.MidnightBreakOffset;
            TimeSpan endTimeSpan = startTimeSpan.Add(TimeSpan.FromDays(1));
            openHours.Add(new TimePeriod(startTimeSpan, endTimeSpan));
            ChangeOpenHours(openHours);
        }

        private OpenForWork _isClosed
        {
            get
            {
	            var isOpen =
		            !(_openHourList.Count == 0 ||
		              (_openHourList.Count == 1 && _openHourList.First().SpanningTime() == TimeSpan.Zero));

				return new OpenForWork(isOpen,isOpen || IsEmailWorkload);
            }
        }

        /// <summary>
        /// Closes this workload day.
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 11/28/2007
        /// </remarks>
        public virtual void Close()
        {
            if (!IsEmailWorkload)
            {
                _taskPeriodList.Clear();
            }
            _openHourList.Clear();

            _recalculateDailyTasks();
            _recalculateDailyAverageTimes();

            foreach (ITaskOwner parent in _parents)
            {
                parent.RecalculateDailyTasks();
                parent.RecalculateDailyAverageTimes();
            }
        }

        /// <summary>
        /// Sets the open hour list.
        /// </summary>
        /// <param name="openHourList">The open hour list.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 11/27/2007
        /// </remarks>
        public virtual void ChangeOpenHours(IList<TimePeriod> openHourList)
        {
            if (openHourList.Any(o => o.StartTime < _workload.Skill.MidnightBreakOffset ||
                o.EndTime > _workload.Skill.MidnightBreakOffset.Add(TimeSpan.FromDays(1))))
            {
                throw new ArgumentOutOfRangeException(nameof(openHourList), "The open hour list contains open hours outside the defined midnight break plus 24 hours.");
            }

            //Split day
            SplitTemplateTaskPeriods(new List<ITemplateTaskPeriod>(_taskPeriodList));
            //Save task values for current list start date time
            SortedList<DateTime, ITemplateTaskPeriod> oldValues = new SortedList<DateTime, ITemplateTaskPeriod>();
            foreach (var taskPeriod in OpenTaskPeriodList)
            {
                oldValues.Add(taskPeriod.Period.StartDateTime, taskPeriod);
            }
            SetOpenHours(openHourList);
            SplitTemplateTaskPeriods(new List<ITemplateTaskPeriod>(_taskPeriodList));
            Lock();
            foreach (ITemplateTaskPeriod taskPeriod in _taskPeriodList)
            {
                ITemplateTaskPeriod templateTaskPeriod;
                if (oldValues.TryGetValue(taskPeriod.Period.StartDateTime, out templateTaskPeriod))
                {
                    taskPeriod.Tasks = templateTaskPeriod.Tasks;
                    taskPeriod.AverageTaskTime = templateTaskPeriod.AverageTaskTime;
                    taskPeriod.AverageAfterTaskTime = templateTaskPeriod.AverageAfterTaskTime;
                    taskPeriod.CampaignTasks = templateTaskPeriod.CampaignTasks;
                    taskPeriod.CampaignTaskTime = templateTaskPeriod.CampaignTaskTime;
                    taskPeriod.CampaignAfterTaskTime = templateTaskPeriod.CampaignAfterTaskTime;
                }
            }
            Release();

            if (!_isClosed.IsOpen) //Checks the open hours
                Close();

            ResetStatistics();
            Initialize();
        }

        protected void SetOpenHours(IList<TimePeriod> openHourList)
        {
            _taskPeriodList.Clear();
            _openHourList.Clear();
            //Set new open hours and create list of task periods (merged) for day
            openHourList.ForEach(o => _openHourList.Add(o));

            DateTime localDate = CurrentDate.Date;
            foreach (TimePeriod openHour in IncomingOpenHoursList)
            {
                DateTimePeriod thePeriod =
                    TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(localDate.Add(openHour.StartTime),
                                                                         localDate.Add(openHour.EndTime),
                                                                         _workload.Skill.TimeZone);
                ITemplateTaskPeriod period = new TemplateTaskPeriod(new Task(), thePeriod);
                _addTaskPeriod(period);
            }
        }

        /// <summary>
        /// Checks for intersection.
        /// </summary>
        /// <param name="newPeriod">The new period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-01-23
        /// </remarks>
        protected bool CheckForIntersection(IPeriodized newPeriod)
        {
            foreach (ITemplateTaskPeriod period in _taskPeriodList)
            {
                if (period.Period.Intersect(newPeriod.Period))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// _adds the task period.
        /// </summary>
        /// <param name="taskPeriod">The task period.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-01-23
        /// </remarks>
        private void _addTaskPeriod(ITemplateTaskPeriod taskPeriod)
        {
            if (!IsEmailWorkload && !IsWithinOpenHours(taskPeriod))
                throw new InvalidOperationException("A task period must be contained by open hours.");

            if (CheckForIntersection(taskPeriod))
                throw new InvalidOperationException("Task period may not intersect another task period.");

            taskPeriod.SetParent(this);
            _taskPeriodList.Add(taskPeriod);
        }

        /// <summary>
        /// Clears the task list.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-01-31
        /// </remarks>
        protected virtual void ClearTaskList()
        {
            _taskPeriodList.Clear();
        }

        #region Public Properties

        /// <summary>
        /// Gets or sets the total tasks.
        /// </summary>
        /// <value>The total tasks.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 11/27/2007
        /// </remarks>
        public virtual double TotalTasks
        {
            get
            {
                //if (!_initialized) Initialize();
                return _totalTasks;
            }
        }

	    /// <summary>
        /// Gets or sets the average task time.
        /// </summary>
        /// <value>The average task time.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 10.12.2007
        /// </remarks>
        public virtual TimeSpan AverageTaskTime
        {
            get
            {
                return _averageTaskTime;
            }
            set
            {
                checkOpen();

                bool currentState = _turnOffInternalRecalc;
                _turnOffInternalRecalc = true;

                //end fix
                long averageTaskTimeTicks = AverageTaskTime.Ticks;
                if (averageTaskTimeTicks == 0) averageTaskTimeTicks = TimeSpan.FromSeconds(1).Ticks;
                ValueDistributor.DistributeAverageTaskTime(
                    ((double)value.Ticks / averageTaskTimeTicks),
                    value, 
					_taskPeriodList,
                    _workload.Skill.SkillType.TaskTimeDistributionService.DistributionType);
                _turnOffInternalRecalc = currentState;

                _averageTaskTime = value;

                _recalculateDailyAverageTimes();
                _recalculateDailyAverageCampaignTimes();

                OnAverageTaskTimesChanged();
            }
        }

        /// <summary>
        /// Gets or sets the average after task time.
        /// </summary>
        /// <value>The average after task time.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 10.12.2007
        /// </remarks>
        public virtual TimeSpan AverageAfterTaskTime
        {
            get
            {
                return _averageAfterTaskTime;
            }
            set
            {
                checkOpen();

                bool currentState = _turnOffInternalRecalc;
                _turnOffInternalRecalc = true;

                long averageAfterTaskTimeTicks = AverageAfterTaskTime.Ticks;
                if (averageAfterTaskTimeTicks == 0) averageAfterTaskTimeTicks = TimeSpan.FromSeconds(1).Ticks;
                ValueDistributor.DistributeAverageAfterTaskTime(
                    ((double)value.Ticks / averageAfterTaskTimeTicks),
                    value, _taskPeriodList,
                    _workload.Skill.SkillType.TaskTimeDistributionService.DistributionType);
                _turnOffInternalRecalc = currentState;

                _averageAfterTaskTime = value;

                _recalculateDailyAverageTimes();
                _recalculateDailyAverageCampaignTimes();

                OnAverageTaskTimesChanged();
            }
        }

        public virtual OpenForWork OpenForWork
        {
            get { return _isClosed; }
        }

        /// <summary>
        /// Gets the workload.
        /// </summary>
        /// <value>The workload.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 11/27/2007
        /// </remarks>
        public virtual IWorkload Workload
        {
            get { return _workload; }
        }

        /// <summary>
        /// Gets the open hour list.
        /// </summary>
        /// <value>The open hour list.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 11/27/2007
        /// </remarks>
        public virtual ReadOnlyCollection<TimePeriod> OpenHourList
        {
            get
            {
                return new ReadOnlyCollection<TimePeriod>(_openHourList.ToArray());
            }
        }

        /// <summary>
        /// Gets the incoming open hours list.
        /// </summary>
        /// <value>The incoming open hours list.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-28
        /// </remarks>
        protected virtual ICollection<TimePeriod> IncomingOpenHoursList
        {
            get
            {
                if (IsEmailWorkload)
                {
                    TimeSpan startSpan = _workload.Skill.MidnightBreakOffset;
                    TimeSpan endSpan = startSpan.Add(TimeSpan.FromDays(1));
                    TimePeriod timePeriod = new TimePeriod(startSpan, endSpan);
                    return new List<TimePeriod> { timePeriod };
                }
                return _openHourList;
            }
        }

        /// <summary>
        /// Gets the task period list.
        /// </summary>
        /// <value>The task period list.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 11/27/2007
        /// </remarks>
        public virtual ReadOnlyCollection<ITemplateTaskPeriod> TaskPeriodList
        {
            get { return new ReadOnlyCollection<ITemplateTaskPeriod>(_taskPeriodList.ToList()); }
        }

        /// <summary>
        /// Gets the task period list sorted by start times.
        /// </summary>
        /// <value>The sorted task period list.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-14
        /// </remarks>
        public virtual ReadOnlyCollection<ITemplateTaskPeriod> SortedTaskPeriodList
        {
            get
            {
                IList<ITemplateTaskPeriod> list = _taskPeriodList
                    .OrderBy(tp => tp.Period.StartDateTime)
                    .ToList();
                return new ReadOnlyCollection<ITemplateTaskPeriod>(list);
            }
        }


        /// <summary>
        /// Gets the TaskPeriodList period.
        /// </summary>
        /// <value>The task period list period.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-02-26
        /// </remarks>
        public virtual DateTimePeriod TaskPeriodListPeriod
        {
            get
            {
                var currentUtcDate = TimeZoneHelper.ConvertToUtc(_currentDate.Date, _workload.Skill.TimeZone);
                DateTime start = currentUtcDate;
                DateTime end = currentUtcDate;
                if (_taskPeriodList.Count > 0)
                {
                    start = _taskPeriodList.Min(tp => tp.Period.StartDateTime);
                    end = _taskPeriodList.Max(tp => tp.Period.EndDateTime);
                }
                DateTimePeriod dateTimePeriod = new DateTimePeriod(start, end);
                return dateTimePeriod;
            }
        }

        #endregion

        #region Recalculations

        /// <summary>
        /// Recalcs the dayly tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        public virtual void RecalculateDailyTasks()
        {
            _recalculateDailyTasks();
        }
        /// <summary>
        /// Recalculates the daily statistic tasks.
        /// </summary>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-18
        /// </remarks>
        public virtual void RecalculateDailyStatisticTasks()
        {
            _recalculateDailyStatisticTasks();
        }

        private void _recalculateDailyTasks()
        {
            if (!_turnOffInternalRecalc)
            {
                _turnOffInternalRecalc = true;
                _tasks = _taskPeriodList.Sum(t => t.Tasks);
                _totalTasks = _taskPeriodList.Sum(t => t.TotalTasks);
                _turnOffInternalRecalc = false;
                //Inform parent about my changed value!
                if (_initialized) OnTasksChanged();
            }
            else
            {
                if (!_isDirty)
                {
                    _isDirty = true;
                    Parents.ForEach(p => p.SetDirty());
                }
            }
        }

        private void _recalculateDailyStatisticTasks()
        {
            if (!_turnOffInternalRecalc)
            {
                _turnOffInternalRecalc = true;
                _totalStatisticCalculatedTasks = _taskPeriodList.Sum(t => t.StatisticTask.StatCalculatedTasks);
                _totalStatisticAnsweredTasks = _taskPeriodList.Sum(t => t.StatisticTask.StatAnsweredTasks);
                _totalStatisticAbandonedTasks = _taskPeriodList.Sum(t => t.StatisticTask.StatAbandonedTasks);
                _turnOffInternalRecalc = false;
            }
            else
            {
                if (!_isDirty)
                {
                    _isDirty = true;
                    Parents.ForEach(p => p.SetDirty());
                }
            }
        }

        /// <summary>
        /// Recalcs the dayly average times.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        public virtual void RecalculateDailyAverageTimes()
        {
            _recalculateDailyAverageTimes();
        }

        /// <summary>
        /// Recalculates the daily statistic average times.
        /// </summary>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-19
        /// </remarks>
        public virtual void RecalculateDailyAverageStatisticTimes()
        {
            _recalculateDailyAverageStatisticTimes();
        }

		private void _recalculateDailyAverageTimes()
		{
			if (!_turnOffInternalRecalc)
			{
				_turnOffInternalRecalc = true;

				if (_totalTasks > 0d)
				{
					if (_tasks > 0d)
					{
						_averageTaskTime = TimeSpan.FromTicks((long)
							(_taskPeriodList.Sum(t => t.AverageTaskTime.Ticks*t.Tasks)/_tasks));
						_averageAfterTaskTime = TimeSpan.FromTicks((long)
							(_taskPeriodList.Sum(t => t.AverageAfterTaskTime.Ticks*t.Tasks)/_tasks));
					}
					else
					{
						recalculateDailyAverageTimesWhenZeroTasks();
					}

					_totalAverageTaskTime = TimeSpan.FromTicks((long)
							(_taskPeriodList.Sum(t => t.TotalAverageTaskTime.Ticks * t.TotalTasks) / _totalTasks));
					_totalAverageAfterTaskTime = TimeSpan.FromTicks((long)
						(_taskPeriodList.Sum(t => t.TotalAverageAfterTaskTime.Ticks * t.TotalTasks) / _totalTasks));
					
				}
				else
				{
					recalculateDailyAverageTimesWhenZeroTotalTasks();
					recalculateDailyAverageTimesWhenZeroTasks();
				}

				_turnOffInternalRecalc = false;

				//Inform parent about my changed values!
				if (_initialized) OnAverageTaskTimesChanged();
			}
			else
			{
				if (!_isDirty)
				{
					_isDirty = true;
					Parents.ForEach(p => p.SetDirty());
				}
			}
		}

		private void recalculateDailyAverageTimesWhenZeroTotalTasks()
		{
			if (_isClosed.IsOpenForIncomingWork && _taskPeriodList.Count > 0)
			{
				_totalAverageTaskTime = TimeSpan.FromTicks((long)
						(_taskPeriodList.Average(t => t.TotalAverageTaskTime.Ticks)));
				_totalAverageAfterTaskTime = TimeSpan.FromTicks((long)
					(_taskPeriodList.Average(t => t.TotalAverageAfterTaskTime.Ticks)));
			}
			else
			{
				_totalAverageTaskTime = TimeSpan.FromSeconds(0);
				_totalAverageAfterTaskTime = TimeSpan.FromSeconds(0);
			}
		}

		private void recalculateDailyAverageTimesWhenZeroTasks()
		{
			if (_isClosed.IsOpenForIncomingWork && _taskPeriodList.Count > 0)
			{
				_averageTaskTime = TimeSpan.FromTicks((long)
						(_taskPeriodList.Average(t => t.AverageTaskTime.Ticks)));
				_averageAfterTaskTime = TimeSpan.FromTicks((long)
					(_taskPeriodList.Average(t => t.AverageAfterTaskTime.Ticks)));
			}
			else
			{
				_averageTaskTime = TimeSpan.FromSeconds(0);
				_averageAfterTaskTime = TimeSpan.FromSeconds(0);
			}
		}

        private void _recalculateDailyAverageStatisticTimes()
        {
            if (!_turnOffInternalRecalc)
            {
                _turnOffInternalRecalc = true;

                double sumTasks = _taskPeriodList.Sum(t => t.StatisticTask.StatAnsweredTasks);
                if (sumTasks > 0d)
                {
	                _totalStatisticAverageTaskTime = TimeSpan.FromTicks((long)
		                (_taskPeriodList.Sum(
			                t =>
				                t.StatisticTask.StatAverageTaskTime.Ticks * handleZeroCallsForInterval(t))/sumTasks));
	                _totalStatisticAverageAfterTaskTime = TimeSpan.FromTicks((long)
		                (_taskPeriodList.Sum(
			                t =>
												t.StatisticTask.StatAverageAfterTaskTime.Ticks * handleZeroCallsForInterval(t)) / sumTasks));
                }
                else
                {
                    if (_isClosed.IsOpenForIncomingWork && _taskPeriodList.Count > 0)
                    {
                        _totalStatisticAverageTaskTime = TimeSpan.FromTicks((long)
                                (_taskPeriodList.Average(t => t.StatisticTask.StatAverageTaskTime.Ticks)));
                        _totalStatisticAverageAfterTaskTime = TimeSpan.FromTicks((long)
                            (_taskPeriodList.Average(t => t.StatisticTask.StatAverageAfterTaskTime.Ticks)));
                    }
                    else
                    {
                        _totalStatisticAverageTaskTime = TimeSpan.FromSeconds(0);
                        _totalStatisticAverageAfterTaskTime = TimeSpan.FromSeconds(0);
                    }
                }

                _turnOffInternalRecalc = false;
            }
            else
            {
                if (!_isDirty)
                {
                    _isDirty = true;
                    Parents.ForEach(p => p.SetDirty());
                }
            }
        }

	    private static double handleZeroCallsForInterval(ITemplateTaskPeriod t)
	    {
		    return Math.Max(t.StatisticTask.StatAnsweredTasks, 1);
	    }

	    #endregion

        /// <summary>
        /// Called when [tasks changed].
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 10.12.2007
        /// </remarks>
        public virtual void OnTasksChanged()
        {
            OnValuesChanged();
        }

        private void OnValuesChanged()
        {
            if (!_turnOffInternalRecalc)
            {
                _isDirty = false;
                foreach (ITaskOwner parent in Parents)
                {
                    parent.RecalculateDailyTasks();
                    parent.RecalculateDailyAverageTimes();
                    parent.RecalculateDailyCampaignTasks();
                    parent.RecalculateDailyAverageCampaignTimes();
                }
            }
            else
            {
                if (!_isDirty)
                {
                    _isDirty = true;
                    Parents.ForEach(p => p.SetDirty());
                }
            }
        }

        /// <summary>
        /// Called when [average task time changed].
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 10.12.2007
        /// </remarks>
        public virtual void OnAverageTaskTimesChanged()
        {
            OnValuesChanged();
        }

        /// <summary>
        /// Called when [campaign tasks changed].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public virtual void OnCampaignTasksChanged()
        {
            OnValuesChanged();
        }

        /// <summary>
        /// Called when [campaign average times changed].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public virtual void OnCampaignAverageTimesChanged()
        {
            OnValuesChanged();
        }

        /// <summary>
        /// Gets a value indicating whether this instance is locked.
        /// </summary>
        /// <value><c>true</c> if this instance is locked; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-23
        /// </remarks>
        public virtual bool IsLocked
        {
            get { return _turnOffInternalRecalc; }
        }

        /// <summary>
        /// Gets the template reference.
        /// </summary>
        /// <value>The template reference.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-09
        /// </remarks>
        public abstract ITemplateReference TemplateReference { get; protected set; }

        /// <summary>
        /// Updates the name of the template.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-14
        /// </remarks>
        public abstract void ClearTemplateName();

        /// <summary>
        /// Locks this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-22
        /// </remarks>
        public virtual void Lock()
        {
            _turnOffInternalRecalc = true;
            foreach (ITaskOwner parent in Parents)
            {
                parent.Lock();
            }
        }

        /// <summary>
        /// Releases this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-22
        /// </remarks>
        public virtual void Release()
        {
            _turnOffInternalRecalc = false;
            if (_isDirty)
            {
                Initialize();
                _isDirty = false;
            }
            foreach (ITaskOwner parent in Parents)
            {
                parent.Release();
            }
        }

        /// <summary>
        /// Removes the parent.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        public virtual void RemoveParent(ITaskOwner parent)
        {
            if (Parents.Contains(parent))
                Parents.Remove(parent);
        }

        /// <summary>
        /// Sets the entity as dirty.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-23
        /// </remarks>
        public virtual void SetDirty()
        {
            _isDirty = true;
        }

        /// <summary>
        /// Gets the workload date.
        /// </summary>
        /// <value>The workload date.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-01-23
        /// </remarks>
        public virtual DateOnly CurrentDate
        {
            get { return _currentDate; }
        }

        /// <summary>
        /// Gets the total statistic calculated tasks.
        /// </summary>
        /// <value>The total statistic calculated tasks.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-18
        /// </remarks>
        public virtual double TotalStatisticCalculatedTasks
        {
            get
            {
                //if (!_initialized) Initialize();
                return _totalStatisticCalculatedTasks;
            }
            set { _totalStatisticCalculatedTasks = value; }
        }

        /// <summary>
        /// Gets the total statistic answered tasks.
        /// </summary>
        /// <value>The total statistic answered tasks.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-18
        /// </remarks>
        public virtual double TotalStatisticAnsweredTasks
        {
            get
            {
                //if (!_initialized) Initialize();
                return _totalStatisticAnsweredTasks;
            }
            set { _totalStatisticAnsweredTasks = value; }
        }

        /// <summary>
        /// Gets the total statistic abandoned tasks.
        /// </summary>
        /// <value>The total statistic abandoned tasks.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-18
        /// </remarks>
        public virtual double TotalStatisticAbandonedTasks
        {
            get
            {
                //if (!_initialized) Initialize();
                return _totalStatisticAbandonedTasks;
            }
            set { _totalStatisticAbandonedTasks = value; }
        }

        /// <summary>
        /// Gets the total statistic average task time.
        /// </summary>
        /// <value>The total statistic average task time.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-18
        /// </remarks>
        public virtual TimeSpan TotalStatisticAverageTaskTime
        {
            get
            {
                //if (!_initialized) Initialize();
                return _totalStatisticAverageTaskTime;
            }
            set { _totalStatisticAverageTaskTime = value; }
        }

        /// <summary>
        /// Gets the total statistic average after task time.
        /// </summary>
        /// <value>The total statistic average after task time.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-18
        /// </remarks>
        public virtual TimeSpan TotalStatisticAverageAfterTaskTime
        {
            get
            {
                //if (!_initialized) Initialize();
                return _totalStatisticAverageAfterTaskTime;
            }
            set { _totalStatisticAverageAfterTaskTime = value; }
        }

        /// <summary>
        /// Gets the total average after task time.
        /// </summary>
        /// <value>The total average after task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public virtual TimeSpan TotalAverageAfterTaskTime
        {
            get
            {
                //if (!_initialized) Initialize();
                return _totalAverageAfterTaskTime;
            }
        }

        /// <summary>
        /// Gets the total average task time.
        /// </summary>
        /// <value>The total average task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public virtual TimeSpan TotalAverageTaskTime
        {
            get
            {
                //if (!_initialized) Initialize();
                return _totalAverageTaskTime;
            }
        }

        /// <summary>
        /// Gets or sets the tasks.
        /// </summary>
        /// <value>The tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public virtual double Tasks
        {
            get
            {
                //if (!_initialized) Initialize();
                return _tasks;
            }
            set
            {
                checkOpen();

                bool currentState = _turnOffInternalRecalc;
                _turnOffInternalRecalc = true;
                if (value < 0)
                    value = 0;

				_distributeTasks(value);

                _tasks = value;

                _turnOffInternalRecalc = currentState;

                _recalculateDailyTasks();
                _recalculateDailyAverageTimes();
                _recalculateDailyCampaignTasks();
                _recalculateDailyAverageCampaignTimes();

                OnTasksChanged();
            }
        }

	    /// <summary>
	    /// Gets or sets the campaign tasks.
	    /// </summary>
	    /// <value>The campaign tasks.</value>
	    /// <remarks>
	    /// Created by: robink
	    /// Created date: 2008-03-04
	    /// </remarks>
	    public virtual Percent CampaignTasks
        {
            get
            {
                //if (!_initialized) Initialize();
                return _campaignTasks;
            }
            set
            {
                checkOpen();

                bool currentState = _turnOffInternalRecalc;
                _turnOffInternalRecalc = true;
                _taskPeriodList.ForEach(tp =>
                {
                    tp.CampaignTasks = value;
                });
                _turnOffInternalRecalc = currentState;

                _campaignTasks = value;

                _recalculateDailyTasks();
                _recalculateDailyAverageTimes();
                _recalculateDailyAverageCampaignTimes();

                OnCampaignTasksChanged();
            }
        }

        /// <summary>
        /// Gets or sets the campaign task time.
        /// </summary>
        /// <value>The campaign task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public virtual Percent CampaignTaskTime
        {
            get
            {
                return _campaignTaskTime;
            }
            set
            {
                checkOpen();

                bool currentState = _turnOffInternalRecalc;
                _turnOffInternalRecalc = true;
                _taskPeriodList.ForEach(t =>
                {
                    t.CampaignTaskTime = value;
                });
                _turnOffInternalRecalc = currentState;

                _campaignTaskTime = value;

                _recalculateDailyAverageTimes();

                OnCampaignAverageTimesChanged();
            }
        }

        private void checkOpen()
        {
            if (!_isClosed.IsOpen && !IsEmailWorkload)
                throw new InvalidOperationException("Workload day must be open.");
        }

        /// <summary>
        /// Gets or sets the campaign after task time.
        /// </summary>
        /// <value>The campaign after task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public virtual Percent CampaignAfterTaskTime
        {
            get
            {
                //if (!_initialized) Initialize();
                return _campaignAfterTaskTime;
            }
            set
            {
                checkOpen();

                bool currentState = _turnOffInternalRecalc;
                _turnOffInternalRecalc = true;
                _taskPeriodList.ForEach(t =>
                {
                    t.CampaignAfterTaskTime = value;
                });
                _turnOffInternalRecalc = currentState;

                _campaignAfterTaskTime = value;

                _recalculateDailyAverageTimes();

                OnCampaignAverageTimesChanged();
            }
        }

        /// <summary>
        /// Recalcs the daily campaign tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        public virtual void RecalculateDailyCampaignTasks()
        {
            _recalculateDailyCampaignTasks();
        }

	    private void _distributeTasks(double value)
	    {
		    if (_useSkewedDistribution)
		    {
				ValueDistributor.DistributeToFirstOpenPeriod(value, _taskPeriodList, _openHourList, _workload.Skill.TimeZone);
		    }
		    else
		    {
				ValueDistributor.Distribute(value, _taskPeriodList, DistributionType.ByPercent);
		    }			
	    }

        private void _recalculateDailyCampaignTasks()
        {
            if (!_turnOffInternalRecalc)
            {
                _turnOffInternalRecalc = true;
                if (_tasks > 0)
                {
					double totalCamapaignTasks = _taskPeriodList.Sum(t => t.Task.Tasks * t.CampaignTasks.Value);
					_campaignTasks = new Percent(totalCamapaignTasks / _tasks);
                }
                _turnOffInternalRecalc = false;

                //Inform parent about my changed value!
                if (_initialized) OnCampaignTasksChanged();
            }
            else
            {
                _isDirty = true;
            }
        }

        /// <summary>
        /// Recalcs the daily average campaign times.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        public virtual void RecalculateDailyAverageCampaignTimes()
        {
            _recalculateDailyAverageCampaignTimes();
        }

        private void _recalculateDailyAverageCampaignTimes()
        {
            if (!_turnOffInternalRecalc)
            {
                _turnOffInternalRecalc = true;
	            if (_averageTaskTime != TimeSpan.Zero)
	            {
					double totalCampaignTaskTime = _taskPeriodList.Sum(t => t.AverageTaskTime.Ticks * (1 + t.CampaignTaskTime.Value) * t.Tasks);
		            double sumOfTaskTime = _tasks*_averageTaskTime.Ticks;
		            _campaignTaskTime = Math.Abs(sumOfTaskTime) < 0.000001 ? new Percent(0) : new Percent((totalCampaignTaskTime / sumOfTaskTime) - 1d);
	            }
	            if (_averageAfterTaskTime != TimeSpan.Zero)
	            {
					double totalCampaignAfterTaskTime = _taskPeriodList.Sum(t => t.AverageAfterTaskTime.Ticks * (1 + t.CampaignAfterTaskTime.Value) * t.Tasks);
					double sumOfAfterTaskTime = _tasks * _averageAfterTaskTime.Ticks;
		            _campaignAfterTaskTime = Math.Abs(sumOfAfterTaskTime) < 0.000001 ? new Percent(0) : new Percent((totalCampaignAfterTaskTime / sumOfAfterTaskTime) - 1d);
	            }
	            _turnOffInternalRecalc = false;

                //Inform parent about my changed value!
                if (_initialized) OnCampaignAverageTimesChanged();
            }
            else
            {
                _isDirty = true;
            }
        }

        /// <summary>
        /// Sets the parent.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        public virtual void AddParent(ITaskOwner parent)
        {
            if (!Parents.Contains(parent))
                Parents.Add(parent);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-08
        /// </remarks>
        public virtual void Initialize()
        {
            _initialized = false;
            _recalculateDailyTasks();
            _recalculateDailyAverageTimes();
            _recalculateDailyCampaignTasks();
            _recalculateDailyAverageCampaignTimes();
            _recalculateDailyStatisticTasks();
            _recalculateDailyAverageStatisticTimes();
            _initialized = true;
        }

        public virtual void ClearParents()
        {
            _parents.Clear();
        }

        /// <summary>
        /// Sets the task period collection.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-22
        /// </remarks>
        public virtual void SetTaskPeriodCollection(IList<ITemplateTaskPeriod> list)
        {
            if (list != null &&
                list.Count > 0 &&
                _taskPeriodList.Count > 0)
            {
                //if (list.Count == _taskPeriodList.Count)

                var matchingItems = from tp in _taskPeriodList
                                    from i in list
                                    where tp.Period.StartDateTime == i.Period.StartDateTime
                                    select new { OldItem = tp, NewItem = i };

                Lock();
                foreach (var item in matchingItems)
                {
                    item.OldItem.Tasks = item.NewItem.Tasks;
                    item.OldItem.AverageTaskTime = item.NewItem.AverageTaskTime;
                    item.OldItem.AverageAfterTaskTime = item.NewItem.AverageAfterTaskTime;
                    item.OldItem.CampaignTasks = item.NewItem.CampaignTasks;
                    item.OldItem.CampaignTaskTime = item.NewItem.CampaignTaskTime;
                    item.OldItem.CampaignAfterTaskTime = item.NewItem.CampaignAfterTaskTime;
                }
                Release();
            }
        }

        public virtual void SetTaskPeriodCollectionWithStatistics(IList<ITemplateTaskPeriod> list)
        {
            if (list != null &&
                list.Count > 0 &&
                _taskPeriodList.Count > 0)
            {
                var matchingItems = from tp in _taskPeriodList
                                    from i in list
                                    where tp.Period.StartDateTime == i.Period.StartDateTime
                                    select new { OldItem = tp, NewItem = i };

                Lock();
                foreach (var item in matchingItems)
                {
                    item.OldItem.Tasks = item.NewItem.Tasks;
                    item.OldItem.AverageTaskTime = item.NewItem.AverageTaskTime;
                    item.OldItem.AverageAfterTaskTime = item.NewItem.AverageAfterTaskTime;
                    item.OldItem.CampaignTasks = item.NewItem.CampaignTasks;
                    item.OldItem.CampaignTaskTime = item.NewItem.CampaignTaskTime;
                    item.OldItem.CampaignAfterTaskTime = item.NewItem.CampaignAfterTaskTime;
                    item.OldItem.StatisticTask = item.NewItem.StatisticTask;
                }
                Release();
            }
        }

        /// <summary>
        /// Merges the template task periods.
        /// </summary>
        /// <param name="templateTaskPeriodList">The template task period list.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-22
        /// </remarks>
        public virtual void MergeTemplateTaskPeriods(IList<ITemplateTaskPeriod> templateTaskPeriodList)
        {
            innerMergeTemplateTaskPeriods(templateTaskPeriodList, day => day.Lock(), day => day.Release());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        protected void innerMergeTemplateTaskPeriods(IList<ITemplateTaskPeriod> templateTaskPeriodList, Action<IWorkloadDayBase> lockAction, Action<IWorkloadDayBase> releaseAction)
        {
            if (templateTaskPeriodList.Count == 0) return;
            if (!templateTaskPeriodList.All(i => Equals(i.Parent)))
            {
                throw new ArgumentException("All items in supplied list must have this entity as parent.", nameof(templateTaskPeriodList));
            }

            //Create a list of valid date time periods
            var validPeriods = from o in _openHourList
                               select new DateTimePeriod(
								TimeZoneHelper.ConvertToUtc(CurrentDate.Date.Add(o.StartTime), _workload.Skill.TimeZone),
								TimeZoneHelper.ConvertToUtc(CurrentDate.Date.Add(o.EndTime), _workload.Skill.TimeZone));

            //Loop through list and find containing items from list
            foreach (var period in validPeriods)
            {
                var currentTaskPeriodList = (from t in templateTaskPeriodList
                                             where period.Contains(t.Period.StartDateTime)
                                             orderby t.Period.StartDateTime ascending
                                             select t).ToArray();

                if (currentTaskPeriodList.Length == 0) continue;

                TaskOwnerPeriod taskOwnerPeriod = new TaskOwnerPeriod(
                    _currentDate,
                    currentTaskPeriodList.OfType<ITaskOwner>().ToList(),
                    TaskOwnerPeriodType.Other);

                Task newTask = new Task(
                    taskOwnerPeriod.Tasks,
                    taskOwnerPeriod.AverageTaskTime,
                    taskOwnerPeriod.AverageAfterTaskTime);

                Campaign newCampaign = new Campaign(
                    taskOwnerPeriod.CampaignTasks,
                    taskOwnerPeriod.CampaignTaskTime,
                    taskOwnerPeriod.CampaignAfterTaskTime);

                TemplateTaskPeriod newTaskPeriod = new TemplateTaskPeriod(
                    newTask,
                    newCampaign,
                    new DateTimePeriod(
	                    currentTaskPeriodList[0].Period.StartDateTime,
		                    currentTaskPeriodList[currentTaskPeriodList.Length-1].Period.EndDateTime));

                lockAction(this);
                newTaskPeriod.SetParent(this);
                currentTaskPeriodList.ForEach(i => _taskPeriodList.Remove(i));
                _addTaskPeriod(newTaskPeriod);
                ResetStatistics();
                releaseAction(this);
            }
        }

        /// <summary>
        /// Splits the template task periods.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-22
        /// </remarks>
        public virtual void SplitTemplateTaskPeriods(IList<ITemplateTaskPeriod> list)
        {
            innerSplitTemplateTaskPeriods(list, d => d.Lock(), d => d.Release());
        }

        protected void innerSplitTemplateTaskPeriods(IList<ITemplateTaskPeriod> list, Action<IWorkloadDayBase> lockAction, Action<IWorkloadDayBase> releaseAction)
        {
            if (list.Count == 0) return;
            if (!list.All(i => Equals(i.Parent)))
            {
                throw new ArgumentException("All items in supplied list must have this entity as parent.", nameof(list));
            }

            lockAction(this);
            TimeSpan resolutionAsTimeSpan = TimeSpan.FromMinutes(_workload.Skill.DefaultResolution);
            foreach (ITemplateTaskPeriod templateTaskPeriod in list)
            {
                if (_taskPeriodList.Contains(templateTaskPeriod))
                {
                    IList<ITaskOwner> baseCollection = new List<ITaskOwner>();

                    _taskPeriodList.Remove(templateTaskPeriod);
                    for (DateTime t = templateTaskPeriod.Period.StartDateTime;
                         t < templateTaskPeriod.Period.EndDateTime;
                         t = t.Add(resolutionAsTimeSpan))
                    {
                        ITemplateTaskPeriod newTemplateTaskPeriod = new TemplateTaskPeriod(
                            new Task(),
                            new DateTimePeriod(t, t.Add(resolutionAsTimeSpan)));
                       if (IsEmailWorkload || IsWithinOpenHours(newTemplateTaskPeriod))
                        {
                            baseCollection.Add(newTemplateTaskPeriod);
                            newTemplateTaskPeriod.SetParent(this);
                            _addTaskPeriod(newTemplateTaskPeriod);
                        }
                    }

	                if (baseCollection.Count > 0)
	                {
		                TaskOwnerPeriod splittedTaskOwnerPeriod = new TaskOwnerPeriod(
			                _currentDate,
				                baseCollection,
				                TaskOwnerPeriodType.Other);
		                splittedTaskOwnerPeriod.Tasks = templateTaskPeriod.Tasks;
		                splittedTaskOwnerPeriod.AverageTaskTime = templateTaskPeriod.AverageTaskTime;
		                splittedTaskOwnerPeriod.AverageAfterTaskTime = templateTaskPeriod.AverageAfterTaskTime;
		                splittedTaskOwnerPeriod.CampaignTasks = templateTaskPeriod.CampaignTasks;
		                splittedTaskOwnerPeriod.CampaignTaskTime = templateTaskPeriod.CampaignTaskTime;
		                splittedTaskOwnerPeriod.CampaignAfterTaskTime = templateTaskPeriod.CampaignAfterTaskTime;
	                }
                }
            }
            ResetStatistics();
            releaseAction(this);
        }

        /// <summary>
        /// Resets the task owner.
        /// </summary>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-05-26
        /// </remarks>
        public virtual void ResetTaskOwner()
        {
            Tasks = 0;
            AverageAfterTaskTime = TimeSpan.Zero;
            AverageTaskTime = TimeSpan.Zero;
            CampaignAfterTaskTime = new Percent(0);
            CampaignTasks = new Percent(0);
            CampaignTaskTime = new Percent(0);
        }

        /// <summary>
        /// Determines whether [is only incoming] [the specified template task period].
        /// </summary>
        /// <param name="templateTaskPeriod">The template task period.</param>
        /// <returns>
        /// 	<c>true</c> if [is only incoming] [the specified template task period]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-28
        /// </remarks>
        public virtual bool IsOnlyIncoming(ITemplateTaskPeriod templateTaskPeriod)
        {
            return !IsWithinOpenHours(templateTaskPeriod);
        }

        public virtual ReadOnlyCollection<ITemplateTaskPeriod> OpenTaskPeriodList => new ReadOnlyCollection<ITemplateTaskPeriod>(
			_taskPeriodList.Where(IsWithinOpenHours).ToList());

		/// <summary>
        /// Gets the parents. (Mostly for test reasons)
        /// </summary>
        /// <value>The parents.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-10-13
        /// </remarks>
		public virtual IList<ITaskOwner> Parents
		{
			get => _parents;
			protected set => _parents = value;
		}

		public virtual ReadOnlyCollection<ITemplateTaskPeriodView> TemplateTaskPeriodViewCollection(TimeSpan periodLength)
        {
            var myPeriodLength = TimeSpan.Zero;
            if (SortedTaskPeriodList.Count > 0)
            {
                myPeriodLength = SortedTaskPeriodList[0].Period.ElapsedTime();
            }

			var views = myPeriodLength >= periodLength
				? SortedTaskPeriodList.SelectMany(period => period.Split(periodLength)).ToList()
				: new List<ITemplateTaskPeriodView>();

            return new ReadOnlyCollection<ITemplateTaskPeriodView>(views);
        }

        public virtual void SetQueueStatistics(IQueueStatisticsProvider queueStatisticsProvider)
        {
            _queueStatisticsProvider = queueStatisticsProvider;
            ResetStatistics();
        }

		public virtual void DistributeTasks(IEnumerable<ITemplateTaskPeriod> sortedTemplateTaskPeriods)
	    {
		    throw new NotImplementedException();
	    }

	    protected void ResetStatistics()
        {
            if (_queueStatisticsProvider == null) return;
            
			foreach (var taskPeriod in _taskPeriodList)
            {
                _queueStatisticsProvider.GetStatisticsForPeriod(taskPeriod.Period).ApplyStatisticsTo(taskPeriod);
            }
            RecalculateDailyStatisticTasks();
            RecalculateDailyAverageStatisticTimes();
        }

        private bool IsWithinOpenHours(ITemplateTaskPeriod periodized)
        {
            DateTime localDate = CurrentDate.Date;
				var timeZone = Workload.Skill.TimeZone;
            TemplateTaskPeriod taskPeriod = (TemplateTaskPeriod)periodized;
            if (taskPeriod.LocalPeriodCache == null)
            {
                DateTime localStart = periodized.Period.StartDateTimeLocal(timeZone);
                DateTime localEnd = periodized.Period.EndDateTimeLocal(timeZone);
                if (timeZone.IsAmbiguousTime(localStart) &&
                    localDate != localStart.Date)
                {
                    localStart = localStart.AddHours(-1);
                    localEnd = localEnd.AddHours(-1);
                }

                if (timeZone.IsInvalidTime(localEnd.AddMinutes(-1)) &&
                    !timeZone.IsInvalidTime(localStart))
                {
                    localEnd = localEnd.AddHours(-1);
                }
                taskPeriod.LocalPeriodCache = new LocalPeriodCache(localStart, localEnd);
            }

			var openHoursLocal = new List<MinMax<DateTime>>();
			foreach (var timePeriod in OpenHourList)
			{
				var startTimeLocal = TimeZoneHelper.ConvertFromUtc(TimeZoneHelper.ConvertToUtc(localDate.Add(timePeriod.StartTime), timeZone), timeZone);
				var endTimeLocal = TimeZoneHelper.ConvertFromUtc(TimeZoneHelper.ConvertToUtc(localDate.Add(timePeriod.EndTime), timeZone), timeZone);
				openHoursLocal.Add(new MinMax<DateTime>(startTimeLocal, endTimeLocal));
			}
			
			return openHoursLocal.Any(o => taskPeriod.LocalPeriodCache.LocalStart >= o.Minimum &&
					taskPeriod.LocalPeriodCache.LocalEnd <= o.Maximum);
        }

        public virtual void SetWorkloadInstance(IWorkload workload)
        {
            if (!_workload.Equals(workload)) throw new ArgumentException("It must be an other instance of the same workload.");
            _workload = workload;
        }
	}

    public class LocalPeriodCache
    {
        public DateTime LocalStart { get; }
        public DateTime LocalEnd { get; }

        public LocalPeriodCache(DateTime localStart, DateTime localEnd)
        {
            LocalStart = localStart;
            LocalEnd = localEnd;
        }
    }
}

