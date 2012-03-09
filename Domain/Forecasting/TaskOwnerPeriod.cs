﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public interface ITaskOwnerPeriod : ITaskOwner
    {
        /// <summary>
        /// Gets the end date of the current month for the thread culture.
        /// </summary>
        /// <value>The end date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        DateOnly EndDate { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is loaded.
        /// </summary>
        /// <value><c>true</c> if this instance is loaded; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        bool IsLoaded { get; }

        /// <summary>
        /// Gets the start date of the month of the current date for the thread culture.
        /// </summary>
        /// <value>The start date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        DateOnly StartDate { get; }

        /// <summary>
        /// Gets the task owner day collection.
        /// </summary>
        /// <value>The task owner day collection.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        ReadOnlyCollection<ITaskOwner> TaskOwnerDayCollection { get; }

        /// <summary>
        /// Gets or sets the type of task owner period.
        /// </summary>
        /// <value>The type of task owner period.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        TaskOwnerPeriodType TypeOfTaskOwnerPeriod { get; set; }

        /// <summary>
        /// Gets the forecasted incoming demand.
        /// </summary>
        /// <value>The forecasted incoming demand.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-05
        /// </remarks>
        TimeSpan ForecastedIncomingDemand { get; }

        /// <summary>
        /// Gets the forecasted incoming demand with shrinkage.
        /// </summary>
        /// <value>The forecasted incoming demand with shrinkage.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-05
        /// </remarks>
        TimeSpan ForecastedIncomingDemandWithShrinkage { get; }

        /// <summary>
        /// Adds the specified task owner day.
        /// </summary>
        /// <param name="taskOwnerDay">The task owner day.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        void Add(ITaskOwner taskOwnerDay);

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="taskOwnerDays">The task owner days.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        void AddRange(IEnumerable<ITaskOwner> taskOwnerDays);

        /// <summary>
        /// Clears this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        void Clear();
    }

    /// <summary>
    /// Aggregates the information for a workload to a defined level
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-12-17
    /// </remarks>
    public class TaskOwnerPeriod : ITaskOwnerPeriod
    {
        private TimeSpan _averageAfterTaskTime;
        private TimeSpan _averageTaskTime;
        private Percent _campaignAfterTaskTime;
        private Percent _campaignTasks;
        private Percent _campaignTaskTime;
        private DateOnly _currentDate;
        private bool _initialized;
        private bool _isDirty;
        private readonly IList<ITaskOwner> _parents = new List<ITaskOwner>();
        private readonly IList<ITaskOwner> _taskOwnerDayCollection;
        private double _tasks;
        private TimeSpan _totalAverageAfterTaskTime;
        private TimeSpan _totalAverageTaskTime;
        private double _totalStatisticAbandonedTasks;
        private double _totalStatisticAnsweredTasks;
        private TimeSpan _totalStatisticAverageAfterTaskTime;
        private TimeSpan _totalStatisticAverageTaskTime;
        private double _totalStatisticCalculatedTasks;
        private double _totalTasks;
        private bool _turnOffInternalRecalc;
        private TaskOwnerPeriodType _typeOfTaskOwnerPeriod;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskOwnerPeriod"/> class.
        /// </summary>
        /// <param name="currentDate">The current date.</param>
        /// <param name="taskOwnerDays">The task owner days.</param>
        /// <param name="taskOwnerPeriodType">Type of the task owner period.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public TaskOwnerPeriod(DateOnly currentDate, IEnumerable<ITaskOwner> taskOwnerDays, TaskOwnerPeriodType taskOwnerPeriodType)
        {
            _currentDate = currentDate;
            _typeOfTaskOwnerPeriod = taskOwnerPeriodType;
            _taskOwnerDayCollection = new List<ITaskOwner>();
            AddRange(taskOwnerDays);
        }

        /// <summary>
        /// Gets the end date of the current month for the thread culture.
        /// </summary>
        /// <value>The end date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        public DateOnly EndDate
        {
            get
            {
                return GetEndDate();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is loaded.
        /// </summary>
        /// <value><c>true</c> if this instance is loaded; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        public bool IsLoaded
        {
            get
            {
                return (_taskOwnerDayCollection != null &&
                        _taskOwnerDayCollection.Count > 0);
            }
        }

        /// <summary>
        /// Gets the start date of the month of the current date for the thread culture.
        /// </summary>
        /// <value>The start date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        public DateOnly StartDate
        {
            get
            {
                return GetStartDate();
            }
        }

        /// <summary>
        /// Gets the task owner day collection.
        /// </summary>
        /// <value>The task owner day collection.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public ReadOnlyCollection<ITaskOwner> TaskOwnerDayCollection
        {
            get { return new ReadOnlyCollection<ITaskOwner>(_taskOwnerDayCollection); }
        }

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
            get {
                if (!_initialized) Initialize();
                return _totalTasks; }
        }

        /// <summary>
        /// Gets or sets the type of task owner period.
        /// </summary>
        /// <value>The type of task owner period.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public TaskOwnerPeriodType TypeOfTaskOwnerPeriod
        {
            get { return _typeOfTaskOwnerPeriod; }
            set
            {
                _typeOfTaskOwnerPeriod = value;
            }
        }

        /// <summary>
        /// Adds the specified task owner day.
        /// </summary>
        /// <param name="taskOwnerDay">The task owner day.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public void Add(ITaskOwner taskOwnerDay)
        {
            taskOwnerDay.AddParent(this);
            _taskOwnerDayCollection.Add(taskOwnerDay);

            Initialize();
        }

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="taskOwnerDays">The task owner days.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public void AddRange(IEnumerable<ITaskOwner> taskOwnerDays)
        {
            if (taskOwnerDays == null) return;
            Lock();
            taskOwnerDays.ForEach(Add);
            Release();
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public void Clear()
        {
            IList<ITaskOwner> temporaryList = new List<ITaskOwner>(_taskOwnerDayCollection);
            Lock();
            temporaryList.ForEach(Remove);
            Release();
        }

        /// <summary>
        /// Called when [average task time changed].
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 10.12.2007
        /// </remarks>
        private void OnAverageTaskTimeChanged()
        {
            if (!_turnOffInternalRecalc)
            {
                _isDirty = false;
                foreach (ITaskOwner parent in _parents)
                {
                    parent.RecalculateDailyTasks();
                    parent.RecalculateDailyAverageTimes();
                    parent.RecalculateDailyCampaignTasks();
                    parent.RecalculateDailyAverageCampaignTimes();
                }
            }
            else
            {
                foreach (ITaskOwner taskOwner in _parents)
                {
                    taskOwner.SetDirty();
                }
                _isDirty = true;
            }
        }

        /// <summary>
        /// Called when [campaign average times changed].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        private void OnCampaignAverageTimesChanged()
        {
            if (!_turnOffInternalRecalc)
            {
                _isDirty = false;
                foreach (ITaskOwner parent in _parents)
                {
                    parent.RecalculateDailyTasks();
                    parent.RecalculateDailyAverageTimes();
                    parent.RecalculateDailyCampaignTasks();
                    parent.RecalculateDailyAverageCampaignTimes();
                }
            }
            else
            {
                foreach (ITaskOwner taskOwner in _parents)
                {
                    taskOwner.SetDirty();
                }
                _isDirty = true;
            }
        }

        /// <summary>
        /// Called when [campaign tasks changed].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        private void OnCampaignTasksChanged()
        {
            if (!_turnOffInternalRecalc)
            {
                _isDirty = false;
                foreach (ITaskOwner parent in _parents)
                {
                    parent.RecalculateDailyTasks();
                    parent.RecalculateDailyAverageTimes();
                    parent.RecalculateDailyCampaignTasks();
                    parent.RecalculateDailyAverageCampaignTimes();
                }
            }
            else
            {
                foreach (ITaskOwner taskOwner in _parents)
                {
                    taskOwner.SetDirty();
                }
                _isDirty = true;
            }
        }

        /// <summary>
        /// Called when [tasks changed].
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 10.12.2007
        /// </remarks>
        private void OnTasksChanged()
        {
            if (!_turnOffInternalRecalc)
            {
                _isDirty = false;
                foreach (ITaskOwner parent in _parents)
                {
                    parent.RecalculateDailyTasks();
                    parent.RecalculateDailyAverageTimes();
                    parent.RecalculateDailyCampaignTasks();
                    parent.RecalculateDailyAverageCampaignTimes();
                }
            }
            else
            {
                foreach (ITaskOwner taskOwner in _parents)
                {
                    taskOwner.SetDirty();
                }
                _isDirty = true;
            }
        }

        /// <summary>
        /// Recalcs the dayly average times.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        public void RecalculateDailyAverageTimes()
        {
            if (!_turnOffInternalRecalc)
            {
                _isDirty = false;
                _turnOffInternalRecalc = true;

                IList<ITaskOwner> taskOwnerDays = TaskOwnerDayOpenCollection();
                if (_totalTasks > 0d)
                {
                    _averageTaskTime = TimeSpan.FromTicks((long)
                        (taskOwnerDays.Sum(t => t.AverageTaskTime.Ticks * t.Tasks) / _tasks));
                    _averageAfterTaskTime = TimeSpan.FromTicks((long)
                        (taskOwnerDays.Sum(t => t.AverageAfterTaskTime.Ticks * t.Tasks) / _tasks));
                    _totalAverageTaskTime = TimeSpan.FromTicks((long)
                        (taskOwnerDays.Sum(t => t.TotalAverageTaskTime.Ticks * t.TotalTasks) / _totalTasks));
                    _totalAverageAfterTaskTime = TimeSpan.FromTicks((long)
                        (taskOwnerDays.Sum(t => t.TotalAverageAfterTaskTime.Ticks * t.TotalTasks) / _totalTasks));
                }
                else if (taskOwnerDays.Count > 0)
                {
                    _averageTaskTime = TimeSpan.FromTicks((long)
                        (taskOwnerDays.Average(t => t.AverageTaskTime.Ticks)));
                    _averageAfterTaskTime = TimeSpan.FromTicks((long)
                        (taskOwnerDays.Average(t => t.AverageAfterTaskTime.Ticks)));
                    _totalAverageTaskTime = TimeSpan.FromTicks((long)
                        (taskOwnerDays.Average(t => t.TotalAverageTaskTime.Ticks)));
                    _totalAverageAfterTaskTime = TimeSpan.FromTicks((long)
                        (taskOwnerDays.Average(t => t.TotalAverageAfterTaskTime.Ticks)));
                }
                else
                {
                    _averageTaskTime = TimeSpan.FromSeconds(0);
                    _averageAfterTaskTime = TimeSpan.FromSeconds(0);
                    _totalAverageTaskTime = TimeSpan.FromSeconds(0);
                    _totalAverageAfterTaskTime = TimeSpan.FromSeconds(0);
                }

                _turnOffInternalRecalc = false;
            }
            else
            {
                _isDirty = true;
            }
        }

        /// <summary>
        /// Recalcs the dayly tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        public void RecalculateDailyTasks()
        {
            if (!_turnOffInternalRecalc)
            {
                _isDirty = false;
                _turnOffInternalRecalc = true;

                IList<ITaskOwner> taskOwnerDays = TaskOwnerDayOpenCollection();
                if (taskOwnerDays.Count > 0)
                {
                    double sumValue = taskOwnerDays.Sum(t => t.TotalTasks);
                    _totalTasks = Math.Round(sumValue, 0);

                    sumValue = taskOwnerDays.Sum(t => t.Tasks);
                    _tasks = Math.Round(sumValue, 0);
                }
                else
                {
                    _tasks = 0;
                    _totalTasks = 0;
                }

                _turnOffInternalRecalc = false;
            }
            else
            {
                _isDirty = true;
            }
        }

        /// <summary>
        /// Removes the specified task owner day.
        /// </summary>
        /// <param name="taskOwnerDay">The task owner day.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public virtual void Remove(ITaskOwner taskOwnerDay)
        {
            if (!_taskOwnerDayCollection.Contains(taskOwnerDay)) return;
            _taskOwnerDayCollection.Remove(taskOwnerDay);

            Initialize();

            taskOwnerDay.RemoveParent(this);
        }

        /// <summary>
        /// Gets the end date depending on type of workload period.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-19
        /// </remarks>
        private DateOnly GetEndDate()
        {
            var returnDate = CurrentDate;

            switch (_typeOfTaskOwnerPeriod)
            {
                case TaskOwnerPeriodType.Month:
                    returnDate = new DateOnly(DateHelper.GetLastDateInMonth(CurrentDate, CultureInfo.CurrentCulture));
                    break;
                case TaskOwnerPeriodType.Other:
                    if (_taskOwnerDayCollection.Count > 0)
                        returnDate = _taskOwnerDayCollection.Max(wd => wd.CurrentDate);
                    break;
                case TaskOwnerPeriodType.Week:
                    returnDate = new DateOnly(DateHelper.GetLastDateInWeek(CurrentDate, CultureInfo.CurrentCulture));
                    break;
            }

            return returnDate;
        }

        /// <summary>
        /// Gets the start date.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-19
        /// </remarks>
        private DateOnly GetStartDate()
        {
            var returnDate = CurrentDate;

            switch (_typeOfTaskOwnerPeriod)
            {
                case TaskOwnerPeriodType.Month:
                    returnDate = new DateOnly(DateHelper.GetFirstDateInMonth(CurrentDate, CultureInfo.CurrentCulture));
                    break;
                case TaskOwnerPeriodType.Other:
                    if (_taskOwnerDayCollection.Count > 0)
                        returnDate = _taskOwnerDayCollection.Min(wd => wd.CurrentDate);
                    break;
                case TaskOwnerPeriodType.Week:
                    returnDate = new DateOnly(DateHelper.GetFirstDateInWeek(CurrentDate, CultureInfo.CurrentCulture));
                    break;
            }

            return returnDate;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-08
        /// </remarks>
        private void Initialize()
        {
            RecalculateDailyTasks();
            RecalculateDailyAverageTimes();
            RecalculateDailyCampaignTasks();
            RecalculateDailyAverageCampaignTimes();
            RecalculateDailyStatisticTasks();
            RecalculateDailyAverageStatisticTimes();
            
            _initialized = true;
        }

        /// <summary>
        /// Tasks the owner day open collection.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        private IList<ITaskOwner> TaskOwnerDayOpenCollection()
        {
            return _taskOwnerDayCollection.Where(wd => wd.OpenForWork.IsOpen).ToList();
        }

        /// <summary>
        /// Gets or sets the average after task time.
        /// </summary>
        /// <value>The average after task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        public TimeSpan AverageAfterTaskTime
        {
            get
            {
                if (!_initialized) Initialize();
                return _averageAfterTaskTime;
            }
            set
            {
                checkIsLoaded();

                long averageAfterTaskTimeTicks = AverageAfterTaskTime.Ticks;
                if (averageAfterTaskTimeTicks == 0) averageAfterTaskTimeTicks = TimeSpan.FromSeconds(1).Ticks;
                ValueDistributor.DistributeTaskTimes(
                    ((double)value.Ticks / averageAfterTaskTimeTicks),
                    TaskOwnerDayOpenCollection(), 
                    TaskFieldToDistribute.AverageAfterTaskTime);

                _averageAfterTaskTime = value;

                RecalculateDailyAverageTimes();
                RecalculateDailyAverageCampaignTimes();

                OnAverageTaskTimeChanged();
            }
        }

        private void checkIsLoaded()
        {
            if (!IsLoaded) 
                throw new InvalidOperationException("The workload period must contain workload days before using this operation.");
        }

        /// <summary>
        /// Gets or sets the average task time.
        /// </summary>
        /// <value>The average task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        public TimeSpan AverageTaskTime
        {
            get
            {
                if (!_initialized) Initialize();
                return _averageTaskTime;
            }
            set
            {
                checkIsLoaded();

                long averageTaskTimeTicks = AverageTaskTime.Ticks;
                if (averageTaskTimeTicks == 0) averageTaskTimeTicks = TimeSpan.FromSeconds(1).Ticks;
                ValueDistributor.DistributeTaskTimes(
                    ((double)value.Ticks / averageTaskTimeTicks),
                    TaskOwnerDayOpenCollection(),
                    TaskFieldToDistribute.AverageTaskTime);

                _averageTaskTime = value;

                RecalculateDailyAverageTimes();
                RecalculateDailyAverageCampaignTimes();

                OnAverageTaskTimeChanged();
            }
        }

        /// <summary>
        /// Locks this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-22
        /// </remarks>
        public void Lock()
        {
            _turnOffInternalRecalc = true;
            foreach (ITaskOwner parent in _parents)
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
        public void Release()
        {
            if (_taskOwnerDayCollection.Any(w => w.IsLocked)) return;

            _turnOffInternalRecalc = false;
            if (_isDirty)
            {
                Initialize();
                _isDirty = false;
            }
            foreach (ITaskOwner parent in _parents)
            {
                parent.Release();
            }
        }

        /// <summary>
        /// Sets the entity as dirty.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-23
        /// </remarks>
        public void SetDirty()
        {
            _isDirty = true;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is closed.
        /// </summary>
        /// <value><c>true</c> if this instance is closed; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public virtual IOpenForWork OpenForWork
        {
            //get { return _taskOwnerDayCollection.All(wd => wd.IsClosed); }
            get
            {
                return new OpenForWork()
                {
                    IsOpenForIncomingWork =
                        _taskOwnerDayCollection.Any(wd => wd.OpenForWork.IsOpenForIncomingWork),
                    IsOpen = _taskOwnerDayCollection.Any(wd=>wd.OpenForWork.IsOpen)
                };
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is locked.
        /// </summary>
        /// <value><c>true</c> if this instance is locked; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public virtual bool IsLocked
        {
            get { return _turnOffInternalRecalc; }
        }

        /// <summary>
        /// Updates the name of the template.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-14
        /// </remarks>
        public void UpdateTemplateName()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the parent.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public virtual void RemoveParent(ITaskOwner parent)
        {
            if (_parents.Contains(parent))
                _parents.Remove(parent);
        }

        public virtual void ClearParents()
        {
            _parents.Clear();
        }

        /// <summary>
        /// Adds the parent.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        public void AddParent(ITaskOwner parent)
        {
            if (!_parents.Contains(parent))
                _parents.Add(parent);
        }

        /// <summary>
        /// Gets or sets the current date.
        /// </summary>
        /// <value>The current date.</value>
        /// <remarks>
        /// Start and end date for the month should be calculated based on this date.
        /// 
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        public DateOnly CurrentDate
        {
            get { return _currentDate; }
            set { _currentDate = value; }
        }

        /// <summary>
        /// Gets the total statistic calculated tasks.
        /// </summary>
        /// <value>The total statistic calculated tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public double TotalStatisticCalculatedTasks
        {
            get {
                if (!_initialized) Initialize();
                return _totalStatisticCalculatedTasks; }
        }

        /// <summary>
        /// Gets the total statistic answered tasks.
        /// </summary>
        /// <value>The total statistic answered tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public double TotalStatisticAnsweredTasks
        {
            get {
                if (!_initialized) Initialize();
                return _totalStatisticAnsweredTasks; }
        }

        /// <summary>
        /// Gets the total statistic abandoned tasks.
        /// </summary>
        /// <value>The total statistic abandoned tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public double TotalStatisticAbandonedTasks
        {
            get {
                if (!_initialized) Initialize();
                return _totalStatisticAbandonedTasks; }
        }

        /// <summary>
        /// Gets the total statistic average task time.
        /// </summary>
        /// <value>The total statistic average task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public TimeSpan TotalStatisticAverageTaskTime
        {
            get {
                if (!_initialized) Initialize();
                return _totalStatisticAverageTaskTime; }
        }

        /// <summary>
        /// Gets the total statistic average after task time.
        /// </summary>
        /// <value>The total statistic average after task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public TimeSpan TotalStatisticAverageAfterTaskTime
        {
            get {
                if (!_initialized) Initialize();
                return _totalStatisticAverageAfterTaskTime; }
        }

        /// <summary>
        /// Recalculates the daily average statistic times.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public void RecalculateDailyAverageStatisticTimes()
        {
            if (!_turnOffInternalRecalc)
            {
                double sumTasks = _taskOwnerDayCollection.Sum(t => t.TotalStatisticAnsweredTasks);
                if (sumTasks > 0d)
                {
                    _totalStatisticAverageTaskTime = TimeSpan.FromTicks((long)
                            (_taskOwnerDayCollection.Sum(t => t.TotalStatisticAverageTaskTime.Ticks * t.TotalStatisticAnsweredTasks) / sumTasks));
                    _totalStatisticAverageAfterTaskTime = TimeSpan.FromTicks((long)
                        (_taskOwnerDayCollection.Sum(t => t.TotalStatisticAverageAfterTaskTime.Ticks * t.TotalStatisticAnsweredTasks) / sumTasks));
                }
                else
                {
                    if (_taskOwnerDayCollection.Count > 0)
                    {
                        _totalStatisticAverageTaskTime = TimeSpan.FromTicks((long)
                                (_taskOwnerDayCollection.Average(t => t.TotalStatisticAverageTaskTime.Ticks)));
                        _totalStatisticAverageAfterTaskTime = TimeSpan.FromTicks((long)
                            (_taskOwnerDayCollection.Average(t => t.TotalStatisticAverageAfterTaskTime.Ticks)));
                    }
                    else
                    {
                        _totalStatisticAverageTaskTime = TimeSpan.FromSeconds(0);
                        _totalStatisticAverageAfterTaskTime = TimeSpan.FromSeconds(0);
                    }
                }
            }
            else
            {
                _isDirty = true;
            }
        }

        /// <summary>
        /// Recalculates the daily statistic tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public void RecalculateDailyStatisticTasks()
        {
            if (!_turnOffInternalRecalc)
            {
                _totalStatisticCalculatedTasks = _taskOwnerDayCollection.Sum(t => t.TotalStatisticCalculatedTasks);
                _totalStatisticAnsweredTasks = _taskOwnerDayCollection.Sum(t => t.TotalStatisticAnsweredTasks);
                _totalStatisticAbandonedTasks = _taskOwnerDayCollection.Sum(t => t.TotalStatisticAbandonedTasks);
            }
            else
            {
                _isDirty = true;
            }
        }

        /// <summary>
        /// Gets the total average after task time.
        /// </summary>
        /// <value>The total average after task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public TimeSpan TotalAverageAfterTaskTime
        {
            get
            {
                if (!_initialized) Initialize();
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
        public TimeSpan TotalAverageTaskTime
        {
            get
            {
                if (!_initialized) Initialize();
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
        public double Tasks
        {
            get
            {
                if (!_initialized) Initialize();
                return _tasks;
            }
            set
            {
                if (!IsLoaded) throw new InvalidOperationException("The workload period must contain workload days before using this operation.");

                ValueDistributor.Distribute(value, TaskOwnerDayOpenCollection(), DistributionType.ByPercent);
                _tasks = value;

                RecalculateDailyTasks();
                RecalculateDailyCampaignTasks();

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
        public Percent CampaignTasks
        {
            get
            {
                if (!_initialized) Initialize();
                return _campaignTasks;
            }
            set
            {
                checkIfIsClosed();

                bool currentState = _turnOffInternalRecalc;
                _turnOffInternalRecalc = true;
                TaskOwnerDayOpenCollection().ForEach(t => t.CampaignTasks = value);
                _turnOffInternalRecalc = currentState;

                _campaignTasks = value;

                RecalculateDailyTasks();
                RecalculateDailyAverageTimes();
                RecalculateDailyAverageCampaignTimes();

                OnCampaignTasksChanged();
            }
        }

        private void checkIfIsClosed()
        {
            if (!OpenForWork.IsOpen) 
                throw new InvalidOperationException("Workload day must be open.");
        }

        /// <summary>
        /// Gets or sets the campaign task time.
        /// </summary>
        /// <value>The campaign task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public Percent CampaignTaskTime
        {
            get
            {
                if (!_initialized) Initialize();
                return _campaignTaskTime;
            }
            set
            {
                checkIfIsClosed();

                bool currentState = _turnOffInternalRecalc;
                _turnOffInternalRecalc = true;
                TaskOwnerDayOpenCollection().ForEach(t => t.CampaignTaskTime = value);
                _turnOffInternalRecalc = currentState;

                _campaignTaskTime = value;

                RecalculateDailyAverageTimes();

                OnCampaignAverageTimesChanged();
            }
        }

        /// <summary>
        /// Gets or sets the campaign after task time.
        /// </summary>
        /// <value>The campaign after task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public Percent CampaignAfterTaskTime
        {
            get
            {
                if (!_initialized) Initialize();
                return _campaignAfterTaskTime;
            }
            set
            {
                checkIfIsClosed();

                bool currentState = _turnOffInternalRecalc;
                _turnOffInternalRecalc = true;
                TaskOwnerDayOpenCollection().ForEach(t => t.CampaignAfterTaskTime = value);
                _turnOffInternalRecalc = currentState;

                _campaignAfterTaskTime = value;

                RecalculateDailyAverageTimes();

                OnCampaignAverageTimesChanged();
            }
        }

        /// <summary>
        /// Gets the forecasted incoming demand.
        /// </summary>
        /// <value>The forecasted incoming demand.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-05
        /// </remarks>
        public TimeSpan ForecastedIncomingDemand
        {
            get
            {
                var skillDayList = _taskOwnerDayCollection.OfType<SkillDay>();
                return TimeSpan.FromMinutes(skillDayList.Sum(s => s.ForecastedIncomingDemand.TotalMinutes));
            }
        }

        /// <summary>
        /// Gets the forecasted incoming demand with shrinkage.
        /// </summary>
        /// <value>The forecasted incoming demand with shrinkage.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-05
        /// </remarks>
        public TimeSpan ForecastedIncomingDemandWithShrinkage
        {
            get
            {
                var skillDayList = _taskOwnerDayCollection.OfType<SkillDay>();
                return TimeSpan.FromMinutes(skillDayList.Sum(s => s.ForecastedIncomingDemandWithShrinkage.TotalMinutes));
            }
        }

        /// <summary>
        /// Recalcs the daily tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        public void RecalculateDailyCampaignTasks()
        {
            if (!_turnOffInternalRecalc)
            {
                _turnOffInternalRecalc = true;
                if (_tasks > 0)
                {
                    _campaignTasks = new Percent((_totalTasks / _tasks) - 1d);
                }
                _turnOffInternalRecalc = false;

                //Inform parent about my changed value!
                OnCampaignTasksChanged();
            }
            else
            {
                _isDirty = true;
            }
        }

        /// <summary>
        /// Recalcs the daily average times.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        public void RecalculateDailyAverageCampaignTimes()
        {
            if (!_turnOffInternalRecalc)
            {
                _turnOffInternalRecalc = true;
                if (_averageTaskTime != TimeSpan.Zero)
                {
                    _campaignTaskTime = new Percent(((double)_totalAverageTaskTime.Ticks / (double)_averageTaskTime.Ticks) - 1d);
                }
                if (_averageAfterTaskTime != TimeSpan.Zero)
                {
                    _campaignAfterTaskTime = new Percent(((double)_totalAverageAfterTaskTime.Ticks / (double)_averageAfterTaskTime.Ticks) - 1d);
                }
                _turnOffInternalRecalc = false;

                //Inform parent about my changed value!
                OnCampaignAverageTimesChanged();
            }
            else
            {
                _isDirty = true;
            }
        }
        /// <summary>
        /// Resets the task owner.
        /// </summary>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-05-26
        /// </remarks>
        public void ResetTaskOwner()
        {
            Tasks = 0;
            AverageAfterTaskTime = TimeSpan.Zero;
            AverageTaskTime = TimeSpan.Zero;
            CampaignAfterTaskTime = new Percent(0);
            CampaignTasks = new Percent(0);
            CampaignTaskTime = new Percent(0);
        }
    }
}
