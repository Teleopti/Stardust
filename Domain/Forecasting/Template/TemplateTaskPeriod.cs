using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Template
{


    /// <summary>
    /// Holds a TimePeriod Template
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2007-11-13
    /// </remarks>
    public class TemplateTaskPeriod : AggregateEntity, ITemplateTaskPeriod
    {
        private ITask _task;
	    private double? _overrideTasks;
        private ICampaign _campaign = new Campaign();
        private double _aggregatedTasks;
        private IStatisticTask _statisticTask = new StatisticTask();
        private readonly DateTimePeriod _period;
        private static readonly TimeSpan MaxTime = TimeSpan.FromHours(100);

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateTaskPeriod"/> class.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="period">The period.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-13
        /// </remarks>
        public TemplateTaskPeriod(ITask task, DateTimePeriod period)
        {
            _task = task;
            _period = period;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateTaskPeriod"/> class.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="campaign">The campaign.</param>
        /// <param name="period">The period.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public TemplateTaskPeriod(ITask task, ICampaign campaign, DateTimePeriod period) : this(task,period)
        {
            _campaign = campaign;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateTaskPeriod"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-14
        /// </remarks>
        protected TemplateTaskPeriod(){}

        /// <summary>
        /// Gets or sets the task.
        /// </summary>
        /// <value>The task.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-13
        /// </remarks>
        public virtual ITask Task
        {
            get { return _task; }
        }

        /// <summary>
        /// Gets the campaign.
        /// </summary>
        /// <value>The campaign.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public virtual ICampaign Campaign
        {
            get { return _campaign; }
        }

        /// <summary>
        /// Gets the time period.
        /// </summary>
        /// <value>The time period.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-13
        /// </remarks>
        public virtual DateTimePeriod Period
        {
            get { return _period; }
        }

	    public virtual double? OverrideTasks
	    {
		    get { return _overrideTasks; }
		    set { _overrideTasks = value; }
	    }

	    /// <summary>
        /// Sets the number of tasks.
        /// </summary>
        /// <param name="numberOfTasks">The number of tasks.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 5.12.2007
        /// </remarks>
        public virtual void SetTasks(double numberOfTasks)
        {
            ITask oldTask = new Task(_task.Tasks,_task.AverageTaskTime,_task.AverageAfterTaskTime);
            _task = new Task(numberOfTasks, _task.AverageTaskTime, _task.AverageAfterTaskTime);
            OnTaskPeriodChange(oldTask);
            OnTasksChanged();
        }

        /// <summary>
        /// Gets the task owner.
        /// </summary>
        /// <value>The task owner.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        internal virtual protected ITaskOwner TaskOwner
        {
            get { return Parent as ITaskOwner; }
        }

        /// <summary>
        /// Called when [tasks changed].
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 10.12.2007
        /// </remarks>
        public virtual void OnTasksChanged()
        {
            if (TaskOwner != null)
            {
                TaskOwner.RecalculateDailyTasks();
                TaskOwner.RecalculateDailyAverageTimes();
                TaskOwner.RecalculateDailyCampaignTasks();
                TaskOwner.RecalculateDailyAverageCampaignTimes();
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
            if (TaskOwner != null)
            {
                TaskOwner.RecalculateDailyTasks();
                TaskOwner.RecalculateDailyAverageTimes();
                TaskOwner.RecalculateDailyCampaignTasks();
                TaskOwner.RecalculateDailyAverageCampaignTimes();
            }
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
            if (TaskOwner != null)
            {
                TaskOwner.RecalculateDailyTasks();
                TaskOwner.RecalculateDailyAverageTimes();
                TaskOwner.RecalculateDailyCampaignTasks();
                TaskOwner.RecalculateDailyAverageCampaignTimes();
            }
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
            if (TaskOwner != null)
            {
                TaskOwner.RecalculateDailyTasks();
                TaskOwner.RecalculateDailyAverageTimes();
                TaskOwner.RecalculateDailyCampaignTasks();
                TaskOwner.RecalculateDailyAverageCampaignTimes();
            }
        }

        /// <summary>
        /// Called when [task period change].
        /// 
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-14
        /// </remarks>
        public virtual void OnTaskPeriodChange(ITask oldTask)
        {

            if (_task.Tasks != oldTask.Tasks ||
                _task.AverageAfterTaskTime != oldTask.AverageAfterTaskTime ||
                _task.AverageTaskTime != oldTask.AverageTaskTime)
            {
                if (TaskOwner != null)
                {
                    TaskOwner.UpdateTemplateName();
                }
            }
        }
        /// <summary>
        /// Combines the specified task period.
        /// </summary>
        /// <param name="taskPeriod">The task period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-11
        /// </remarks>
        public virtual IList<ITemplateTaskPeriod> Combine(ITemplateTaskPeriod taskPeriod)
        {
            IList<ITemplateTaskPeriod> retList = new List<ITemplateTaskPeriod>();

            if (_period.Intersect(taskPeriod.Period))
            {
                if (_period.Contains(taskPeriod.Period))
                {
                    retList = CombineContainingPeriods(this, taskPeriod);
                }
                else
                {
                    if (taskPeriod.Period.Contains(_period))
                    {
                        retList = CombineContainingPeriods(taskPeriod, this);
                    } 
                    else
                    {
                        retList = CombineNonContainingPeriods(taskPeriod);
                    }
                }                                
            }

            return (from tp in retList
                    orderby tp.Period.StartDateTime, tp.Period.EndDateTime
                    select tp).ToList();
        }

        

        private IList<ITemplateTaskPeriod> CombineNonContainingPeriods(ITemplateTaskPeriod taskPeriod)
        {
            IList<ITemplateTaskPeriod> retList = new List<ITemplateTaskPeriod>();

            double intersectingTasks = 0, intersectingTaskSeconds = 0, intersectingWrapSeconds = 0;

            DateTimePeriod intersectPeriod = _period.Intersection(taskPeriod.Period).Value;
            double intersectingPercent = IntersectPercent(_period, intersectPeriod);
            double nonIntersectingPercent = 1 - intersectingPercent;
            double tasks = _task.Tasks * nonIntersectingPercent;
            intersectingTasks += _task.Tasks - tasks;
            intersectingTaskSeconds += _task.Tasks * _task.AverageTaskTime.TotalSeconds * intersectingPercent;
            intersectingWrapSeconds += _task.Tasks * _task.AverageAfterTaskTime.TotalSeconds * intersectingPercent;

            ITask task = new Task(tasks, _task.AverageTaskTime, _task.AverageAfterTaskTime);
            retList.Add(new TemplateTaskPeriod(task, NonInterSectingPart(_period, intersectPeriod)));

            intersectingPercent = IntersectPercent(taskPeriod.Period, intersectPeriod);
            nonIntersectingPercent = 1 - intersectingPercent;
            tasks = taskPeriod.Task.Tasks * nonIntersectingPercent;
            intersectingTasks += taskPeriod.Task.Tasks - tasks;
            intersectingTaskSeconds += taskPeriod.Task.Tasks * taskPeriod.Task.AverageTaskTime.TotalSeconds * intersectingPercent;
            intersectingWrapSeconds += taskPeriod.Task.Tasks * taskPeriod.Task.AverageAfterTaskTime.TotalSeconds * intersectingPercent;

            task = new Task(tasks, taskPeriod.Task.AverageTaskTime, taskPeriod.Task.AverageAfterTaskTime);
            retList.Add(new TemplateTaskPeriod(task, NonInterSectingPart(taskPeriod.Period, intersectPeriod)));

            TimeSpan intersectingAverageTaskTime = TimeSpan.FromSeconds(intersectingTaskSeconds / intersectingTasks);
            TimeSpan intersectingAverageWrapTime = TimeSpan.FromSeconds(intersectingWrapSeconds / intersectingTasks);
            task = new Task(intersectingTasks, intersectingAverageTaskTime, intersectingAverageWrapTime);
            retList.Add(new TemplateTaskPeriod(task, intersectPeriod));

            return retList;
        }

        private static IList<ITemplateTaskPeriod> CombineContainingPeriods(ITemplateTaskPeriod outerPeriod, ITemplateTaskPeriod innerPeriod)
        {
            IList<ITemplateTaskPeriod> retList = new List<ITemplateTaskPeriod>();
            double tasks;
            ITask task;
            double intersectingTasks = 0, 
                intersectingTaskSeconds = 0, 
                intersectingWrapSeconds = 0;

            if (innerPeriod.Period.StartDateTime > outerPeriod.Period.StartDateTime)
            {
                DateTimePeriod period = new DateTimePeriod(outerPeriod.Period.StartDateTime, innerPeriod.Period.StartDateTime);
                double percent = period.ElapsedTime().TotalSeconds / outerPeriod.Period.ElapsedTime().TotalSeconds;
                tasks = outerPeriod.Task.Tasks * percent;
                task = new Task(tasks, outerPeriod.Task.AverageTaskTime, outerPeriod.Task.AverageAfterTaskTime);
                retList.Add(new TemplateTaskPeriod(task, period));
            }
            DateTimePeriod intersectPeriod = outerPeriod.Period.Intersection(innerPeriod.Period).Value;
            double intersectingPercent = IntersectPercent(outerPeriod.Period, intersectPeriod);
            intersectingTasks += outerPeriod.Task.Tasks * intersectingPercent;
            intersectingTaskSeconds += outerPeriod.Task.Tasks * outerPeriod.Task.AverageTaskTime.TotalSeconds * intersectingPercent;
            intersectingWrapSeconds += outerPeriod.Task.Tasks * outerPeriod.Task.AverageAfterTaskTime.TotalSeconds * intersectingPercent;
            intersectingTasks += innerPeriod.Task.Tasks;
            intersectingTaskSeconds += innerPeriod.Task.Tasks * innerPeriod.Task.AverageTaskTime.TotalSeconds;
            intersectingWrapSeconds += innerPeriod.Task.Tasks * innerPeriod.Task.AverageAfterTaskTime.TotalSeconds;

            TimeSpan intersectingAverageTaskTime = TimeSpan.FromSeconds(intersectingTaskSeconds / intersectingTasks);
            TimeSpan intersectingAverageWrapTime = TimeSpan.FromSeconds(intersectingWrapSeconds / intersectingTasks);
            task = new Task(intersectingTasks, intersectingAverageTaskTime, intersectingAverageWrapTime);
            retList.Add(new TemplateTaskPeriod(task, intersectPeriod));

            if (outerPeriod.Period.EndDateTime > innerPeriod.Period.EndDateTime)
            {
                DateTimePeriod period = new DateTimePeriod(innerPeriod.Period.EndDateTime, outerPeriod.Period.EndDateTime);
                double percent = period.ElapsedTime().TotalSeconds / outerPeriod.Period.ElapsedTime().TotalSeconds;
                tasks = outerPeriod.Task.Tasks * percent;
                task = new Task(tasks, outerPeriod.Task.AverageTaskTime, outerPeriod.Task.AverageAfterTaskTime);
                retList.Add(new TemplateTaskPeriod(task, period));
            }

            return retList;
        }

        private static DateTimePeriod NonInterSectingPart(DateTimePeriod period, DateTimePeriod intersectingPeriod)
        {
            if (period.StartDateTime == intersectingPeriod.StartDateTime)
            {
                return new DateTimePeriod(intersectingPeriod.EndDateTime,period.EndDateTime);
            }

            return new DateTimePeriod(period.StartDateTime,intersectingPeriod.StartDateTime);
        }

        private static double IntersectPercent(DateTimePeriod period, DateTimePeriod intersectingPeriod)
        {
            return intersectingPeriod.ElapsedTime().TotalSeconds/period.ElapsedTime().TotalSeconds;
        }

        #region ITaskOwner Members

        /// <summary>
        /// Gets or sets the average after task time.
        /// </summary>
        /// <value>The average after task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        public virtual TimeSpan AverageAfterTaskTime
        {
            get
            {
                return _task.AverageAfterTaskTime;
            }
            set
            {
	            if (value < TimeSpan.Zero) return;
                ITask oldTask = new Task(_task.Tasks, _task.AverageTaskTime, _task.AverageAfterTaskTime);
                _task = new Task(_task.Tasks,_task.AverageTaskTime, ApplyMax(value));
                OnTaskPeriodChange(oldTask);
                OnAverageTaskTimesChanged();
            }
        }

        /// <summary>
        /// Gets or sets the average task time.
        /// </summary>
        /// <value>The average task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-17
        /// </remarks>
        public virtual TimeSpan AverageTaskTime
        {
            get
            {
                return _task.AverageTaskTime;
            }
            set
            {
	            if (value < TimeSpan.Zero) return;
                ITask oldTask = new Task(_task.Tasks, _task.AverageTaskTime, _task.AverageAfterTaskTime);
                _task = new Task(_task.Tasks,ApplyMax(value),_task.AverageAfterTaskTime);
                OnTaskPeriodChange(oldTask);
                OnAverageTaskTimesChanged();
            }
        }

        private TimeSpan ApplyMax(TimeSpan value)
        {
            if (value < MaxTime)
                return value;

            return MaxTime;
        }

        /// <summary>
        /// Gets or sets the total tasks.
        /// </summary>
        /// <value>The total tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-22
        /// </remarks>
        public virtual double TotalTasks
        {
	        get
	        {
		        if (OverrideTasks.HasValue)
			        return OverrideTasks.Value;
		        return _task.Tasks * (1d + _campaign.CampaignTasksPercent.Value);
	        }
        }

        /// <summary>
        /// Locks this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-22
        /// </remarks>
        public virtual void Lock()
        {
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
        }

        public virtual void ClearParents()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the current date.
        /// </summary>
        /// <value>The current date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public virtual DateOnly CurrentDate
        {
            get { return DateOnly.MinValue; }
        }

        public virtual OpenForWork OpenForWork
        {
            get
            {
               return new OpenForWork(true,true);
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
            get { return false; }
        }

        /// <summary>
        /// Gets the statistic task.
        /// </summary>
        /// <value>The statistic task.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-11
        /// </remarks>
        public virtual IStatisticTask StatisticTask
        {
			get { return _statisticTask; }
			set { _statisticTask = value; }
        }

        /// <summary>
        /// Sets the parent.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public virtual void AddParent(ITaskOwner parent)
        {
            //throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the total statistic calculated tasks.
        /// </summary>
        /// <value>The total statistic calculated tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public virtual double TotalStatisticCalculatedTasks
        {
            get { return _statisticTask.StatCalculatedTasks; }
        }

        /// <summary>
        /// Gets the total statistic answered tasks.
        /// </summary>
        /// <value>The total statistic answered tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public virtual double TotalStatisticAnsweredTasks
        {
            get { return _statisticTask.StatAnsweredTasks; }
        }

        /// <summary>
        /// Gets the total statistic abandoned tasks.
        /// </summary>
        /// <value>The total statistic abandoned tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public virtual double TotalStatisticAbandonedTasks
        {
            get { return _statisticTask.StatAbandonedTasks; }
        }

        /// <summary>
        /// Gets the total statistic average task time.
        /// </summary>
        /// <value>The total statistic average task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public virtual TimeSpan TotalStatisticAverageTaskTime
        {
            get { return _statisticTask.StatAverageTaskTime; }
        }

        /// <summary>
        /// Gets the total statistic average after task time.
        /// </summary>
        /// <value>The total statistic average after task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public virtual TimeSpan TotalStatisticAverageAfterTaskTime
        {
            get { return _statisticTask.StatAverageAfterTaskTime; }
        }

        /// <summary>
        /// Recalculates the daily average statistic times.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public virtual void RecalculateDailyAverageStatisticTimes()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Recalculates the daily statistic tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        public virtual void RecalculateDailyStatisticTasks()
        {
            throw new NotImplementedException();
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
            get { return TimeSpan.FromTicks(
                (long)(_task.AverageAfterTaskTime.Ticks * (1d + _campaign.CampaignAfterTaskTimePercent.Value)));
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
                return TimeSpan.FromTicks(
                    (long)(_task.AverageTaskTime.Ticks * (1d + _campaign.CampaignTaskTimePercent.Value)));
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
                return _task.Tasks;
            }
            set
            {
	            if (value < 0) return;
                Task oldTask = new Task(_task.Tasks, _task.AverageTaskTime, _task.AverageAfterTaskTime);
                _task = new Task(value, _task.AverageTaskTime, _task.AverageAfterTaskTime);
                OnTaskPeriodChange(oldTask);
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
                return _campaign.CampaignTasksPercent;
            }
            set
            {
                _campaign = new Campaign(value, _campaign.CampaignTaskTimePercent,
                    _campaign.CampaignAfterTaskTimePercent);
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
                return _campaign.CampaignTaskTimePercent;
            }
            set
            {
                _campaign = new Campaign(_campaign.CampaignTasksPercent,
                    value,
                    _campaign.CampaignAfterTaskTimePercent);
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
        public virtual Percent CampaignAfterTaskTime
        {
            get
            {
                return _campaign.CampaignAfterTaskTimePercent;
            }
            set
            {
                _campaign = new Campaign(_campaign.CampaignTasksPercent,
                    _campaign.CampaignTaskTimePercent,
                    value);
                OnCampaignAverageTimesChanged();
            }
        }

        /// <summary>
        /// Gets or sets the aggregated tasks.
        /// </summary>
        /// <value>The aggregated tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-02
        /// </remarks>
        public virtual double AggregatedTasks
        {
            get { return _aggregatedTasks; }
            set { _aggregatedTasks = value; }
        }

        protected internal virtual LocalPeriodCache LocalPeriodCache { get; set; }

        /// <summary>
        /// Recalcs the dayly tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        public virtual void RecalculateDailyCampaignTasks()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Recalcs the dayly average times.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        public virtual void RecalculateDailyAverageCampaignTimes()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ITaskSource Members

        /// <summary>
        /// Recalcs the dayly average times.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        public virtual void RecalculateDailyAverageTimes()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Recalcs the dayly tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-18
        /// </remarks>
        public virtual void RecalculateDailyTasks()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Updates the name of the template.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-14
        /// </remarks>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-14
        /// </remarks>
        public virtual void UpdateTemplateName()
        {
            throw new NotImplementedException();
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
            AverageAfterTaskTime = new TimeSpan(0);
            AverageTaskTime = new TimeSpan(0);
            CampaignAfterTaskTime = new Percent(0);
            CampaignTasks = new Percent(0);
            CampaignTaskTime = new Percent(0);
        }
        #endregion

        #region Implementation of ICloneable

        public virtual object Clone()
        {
            return NoneEntityClone();
        }

        #endregion

        #region Implementation of ICloneableEntity<ITemplateTaskPeriod>

        public virtual ITemplateTaskPeriod NoneEntityClone()
        {
            ITemplateTaskPeriod retobj = (ITemplateTaskPeriod) MemberwiseClone();
            retobj.SetId(null);
            return retobj;
        }

        public virtual ITemplateTaskPeriod EntityClone()
        {
            ITemplateTaskPeriod retobj = (ITemplateTaskPeriod)MemberwiseClone();
            return retobj;
        }

        #endregion

        /// <summary>
        /// Merges the specified template task periods into one single.
        /// </summary>
        /// <param name="templateTaskPeriods">The template task periods.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2009-01-25
        /// </remarks>
        public static ITemplateTaskPeriod Merge(IList<ITemplateTaskPeriod> templateTaskPeriods)
        {
            IList<ITemplateTaskPeriod> sortedTemplateTaskPeriods = (from t in templateTaskPeriods
                                        orderby t.Period.StartDateTime ascending
                                        select t).ToList();
            DateTimePeriod newDateTimePeriod = new DateTimePeriod(sortedTemplateTaskPeriods[0].Period.StartDateTime, sortedTemplateTaskPeriods[sortedTemplateTaskPeriods.Count - 1].Period.EndDateTime);
            TaskOwnerPeriod taskOwnerPeriod = new TaskOwnerPeriod(DateOnly.MinValue, sortedTemplateTaskPeriods.OfType<ITaskOwner>().ToList(), TaskOwnerPeriodType.Other);

                ITask newTask = new Task(
                    taskOwnerPeriod.Tasks,
                    taskOwnerPeriod.AverageTaskTime,
                    taskOwnerPeriod.AverageAfterTaskTime);

                ICampaign newCampaign = new Campaign(
                    taskOwnerPeriod.CampaignTasks,
                    taskOwnerPeriod.CampaignTaskTime,
                    taskOwnerPeriod.CampaignAfterTaskTime);
            
            ITemplateTaskPeriod newTemplateTaskPeriod = new TemplateTaskPeriod(newTask, newCampaign, newDateTimePeriod);

            newTemplateTaskPeriod.StatisticTask.StatAbandonedTasks = taskOwnerPeriod.TotalStatisticAbandonedTasks;
            newTemplateTaskPeriod.StatisticTask.StatAnsweredTasks = taskOwnerPeriod.TotalStatisticAnsweredTasks;
            newTemplateTaskPeriod.StatisticTask.StatCalculatedTasks = taskOwnerPeriod.TotalStatisticCalculatedTasks;
            newTemplateTaskPeriod.StatisticTask.StatAverageAfterTaskTimeSeconds =
                (int)taskOwnerPeriod.TotalStatisticAverageAfterTaskTime.TotalSeconds;
            newTemplateTaskPeriod.StatisticTask.StatAverageTaskTimeSeconds =
                (int)taskOwnerPeriod.TotalStatisticAverageTaskTime.TotalSeconds;

            newTemplateTaskPeriod.Lock();
            newTemplateTaskPeriod.SetParent(sortedTemplateTaskPeriods[0].Parent);
            newTemplateTaskPeriod.Release();
            return newTemplateTaskPeriod;
        }

        public virtual IList<ITemplateTaskPeriodView> Split(TimeSpan periodLength)
        {
            if (Period.ElapsedTime() < periodLength)
                throw new ArgumentOutOfRangeException("periodLength", periodLength,
                                                      "You cannot split to a higher period length");

            if (Period.ElapsedTime().TotalMinutes % periodLength.TotalMinutes > 0)
                throw new ArgumentOutOfRangeException("periodLength", periodLength,
                                                      "You cannot split if you get a remaining time");

            IList<ITemplateTaskPeriodView> newViews = new List<ITemplateTaskPeriodView>();
            IList<DateTimePeriod> newPeriods = Period.Intervals(periodLength);
            
            foreach (DateTimePeriod newPeriod in newPeriods)
            {
                ITemplateTaskPeriodView newView = new TemplateTaskPeriodView();
                newView.Parent = (IWorkloadDay)Parent;
                newView.Period = newPeriod;
                newView.Tasks = Tasks / newPeriods.Count;
                newView.TotalTasks = TotalTasks / newPeriods.Count;
                newView.AverageTaskTime = AverageTaskTime;
                newView.AverageAfterTaskTime = AverageAfterTaskTime;
                // Change to use the number of tasks instead of a percent
                newView.CampaignTasks = newView.Tasks * CampaignTasks.Value;
                newView.TotalAverageAfterTaskTime = TotalAverageAfterTaskTime;
                newView.TotalAverageTaskTime = TotalAverageTaskTime;
                newView.CampaignAfterTaskTime = CampaignAfterTaskTime;
                newView.CampaignTaskTime = CampaignTaskTime;
                newViews.Add(newView);
            }
            return newViews;
        }
    }
}
