using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Has validated volume information for one day
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-03-27
    /// </remarks>
    public class ValidatedVolumeDay : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IValidatedVolumeDay
    {
        private readonly IWorkload _workload;
        private readonly DateOnly _volumeDayDate;
        private ITaskOwner _taskOwnerDay;
        private double? _validatedTasks;
        private TimeSpan? validatedTaskTime;
        private TimeSpan? validatedAfterTaskTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatedVolumeDay"/> class.
        /// </summary>
        /// <param name="workload">The workload.</param>
        /// <param name="date">The date.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        public ValidatedVolumeDay(IWorkload workload, DateOnly date)
        {
            InParameter.NotNull(nameof(workload), workload);

            _workload = workload;
            _volumeDayDate = date;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatedVolumeDay"/> class.
        /// For NHibernate.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        protected ValidatedVolumeDay()
        {
        }

        /// <summary>
        /// Gets a value indicating whether this instance has values.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has values; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-28
        /// </remarks>
        public virtual bool HasValues => (_validatedTasks.HasValue ||
										  validatedTaskTime.HasValue ||
										  validatedAfterTaskTime.HasValue);

	    /// <summary>
        /// Gets the workload.
        /// </summary>
        /// <value>The workload.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        public virtual IWorkload Workload => _workload;

	    /// <summary>
        /// Gets the date.
        /// </summary>
        /// <value>The date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        public virtual DateOnly VolumeDayDate => _volumeDayDate;

	    /// <summary>
        /// Gets or sets the task owner.
        /// </summary>
        /// <value>The task owner.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-28
        /// </remarks>
        public virtual ITaskOwner TaskOwner
        {
            get { return _taskOwnerDay; }
            set { _taskOwnerDay = value; }
        }

        /// <summary>
        /// Gets or sets the validated tasks.
        /// </summary>
        /// <value>The validated tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        public virtual double ValidatedTasks
        {
            get
            {
                if (_validatedTasks.HasValue)
                    return _validatedTasks.Value;
                return OriginalTasks;
            }
            set
            {
	            if (value < 0) return;
				_validatedTasks = value;
            }
        }

        /// <summary>
        /// Gets the original tasks.
        /// </summary>
        /// <value>The original tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        public virtual double OriginalTasks
        {
            get
            {
                if (_taskOwnerDay != null)
                {
                    return _taskOwnerDay.TotalStatisticCalculatedTasks;
                }
                return 0d;
            }
        }

        /// <summary>
        /// Gets or sets the validated average task time.
        /// </summary>
        /// <value>The validated average task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        public virtual TimeSpan ValidatedAverageTaskTime
        {
            get
            {
                if (validatedTaskTime.HasValue)
                    return validatedTaskTime.Value;
                return OriginalAverageTaskTime;
            }
            set
            {
	            if (value < TimeSpan.Zero) return;
				validatedTaskTime = value;
            }
        }

        /// <summary>
        /// Gets the original average task time.
        /// </summary>
        /// <value>The original average task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        public virtual TimeSpan OriginalAverageTaskTime
        {
            get
            {
                if (_taskOwnerDay != null)
                {
                    return _taskOwnerDay.TotalStatisticAverageTaskTime;
                }
                return TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Gets or sets the validated average after task time.
        /// </summary>
        /// <value>The validated average after task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        public virtual TimeSpan ValidatedAverageAfterTaskTime
        {
            get
            {
                if (validatedAfterTaskTime.HasValue)
                    return validatedAfterTaskTime.Value;
                return OriginalAverageAfterTaskTime;
            }
            set
            {
	            if (value < TimeSpan.Zero) return;
				validatedAfterTaskTime = value;
            }
        }

        /// <summary>
        /// Gets the original average after task time.
        /// </summary>
        /// <value>The original average after task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        public virtual TimeSpan OriginalAverageAfterTaskTime
        {
            get
            {
                if (_taskOwnerDay != null)
                {
                    return _taskOwnerDay.TotalStatisticAverageAfterTaskTime;
                }
                return TimeSpan.Zero;
            }
        }

        #region ITaskOwner Members
        /// <summary>
        /// Resets the task owner.
        /// </summary>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-05-26
        /// </remarks>
        public virtual void ResetTaskOwner()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Gets or sets the total tasks.
        /// </summary>
        /// <value>The total tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual double TotalTasks
        {
            get { return _taskOwnerDay.TotalTasks; }
        }

	    public virtual double? OverrideTasks
	    {
		    get { return null; }
	    }

		public virtual TimeSpan? OverrideAverageTaskTime { get; set; }
		public virtual TimeSpan? OverrideAverageAfterTaskTime { get; set; }

	    /// <summary>
        /// Gets the total average after task time.
        /// </summary>
        /// <value>The total average after task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual TimeSpan TotalAverageAfterTaskTime
        {
            get { return _taskOwnerDay.TotalAverageAfterTaskTime; }
        }

        /// <summary>
        /// Gets the total average task time.
        /// </summary>
        /// <value>The total average task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual TimeSpan TotalAverageTaskTime
        {
            get { return _taskOwnerDay.TotalAverageTaskTime; }
        }

        /// <summary>
        /// Gets or sets the tasks.
        /// </summary>
        /// <value>The tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual double Tasks
        {
            get
            {
                return _taskOwnerDay.Tasks;
            }
            set
            {
                _taskOwnerDay.Tasks = value;
            }
        }

        /// <summary>
        /// Gets or sets the average after task time.
        /// </summary>
        /// <value>The average after task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual TimeSpan AverageAfterTaskTime
        {
            get
            {
                return _taskOwnerDay.AverageAfterTaskTime;
            }
            set
            {
                _taskOwnerDay.AverageAfterTaskTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the average task time.
        /// </summary>
        /// <value>The average task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual TimeSpan AverageTaskTime
        {
            get
            {
                return _taskOwnerDay.AverageTaskTime;
            }
            set
            {
                _taskOwnerDay.AverageTaskTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the campaign tasks.
        /// </summary>
        /// <value>The campaign tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual Percent CampaignTasks
        {
            get
            {
                return _taskOwnerDay.CampaignTasks;
            }
            set
            {
                _taskOwnerDay.CampaignTasks = value;
            }
        }

        /// <summary>
        /// Gets or sets the campaign task time.
        /// </summary>
        /// <value>The campaign task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual Percent CampaignTaskTime
        {
            get
            {
                return _taskOwnerDay.CampaignTaskTime;
            }
            set
            {
                _taskOwnerDay.CampaignTaskTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the campaign after task time.
        /// </summary>
        /// <value>The campaign after task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual Percent CampaignAfterTaskTime
        {
            get
            {
                return _taskOwnerDay.CampaignAfterTaskTime;
            }
            set
            {
                _taskOwnerDay.CampaignAfterTaskTime = value;
            }
        }

        /// <summary>
        /// Gets the total statistic calculated tasks.
        /// </summary>
        /// <value>The total statistic calculated tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual double TotalStatisticCalculatedTasks
        {
            get { return ValidatedTasks; }
        }

        /// <summary>
        /// Gets the total statistic answered tasks.
        /// </summary>
        /// <value>The total statistic answered tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual double TotalStatisticAnsweredTasks
        {
            get { return _taskOwnerDay.TotalStatisticAnsweredTasks; }
        }

        /// <summary>
        /// Gets the total statistic abandoned tasks.
        /// </summary>
        /// <value>The total statistic abandoned tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual double TotalStatisticAbandonedTasks
        {
            get { return _taskOwnerDay.TotalStatisticAbandonedTasks; }
        }

        /// <summary>
        /// Gets the total statistic average task time.
        /// </summary>
        /// <value>The total statistic average task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual TimeSpan TotalStatisticAverageTaskTime
        {
            get { return ValidatedAverageTaskTime; }
        }

        /// <summary>
        /// Gets the total statistic average after task time.
        /// </summary>
        /// <value>The total statistic average after task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual TimeSpan TotalStatisticAverageAfterTaskTime
        {
            get { return ValidatedAverageAfterTaskTime; }
        }

        /// <summary>
        /// Recalcs the daily average times.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual void RecalculateDailyAverageTimes()
        {
            _taskOwnerDay.RecalculateDailyAverageTimes();
        }

        /// <summary>
        /// Recalcs the daily tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual void RecalculateDailyTasks()
        {
            _taskOwnerDay.RecalculateDailyTasks();
        }

        /// <summary>
        /// Recalcs the daily tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual void RecalculateDailyCampaignTasks()
        {
            _taskOwnerDay.RecalculateDailyCampaignTasks();
        }

        /// <summary>
        /// Recalcs the daily average campaign times.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual void RecalculateDailyAverageCampaignTimes()
        {
            _taskOwnerDay.RecalculateDailyAverageCampaignTimes();
        }

        /// <summary>
        /// Recalculates the daily average statistic times.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual void RecalculateDailyAverageStatisticTimes()
        {
            _taskOwnerDay.RecalculateDailyAverageStatisticTimes();
        }

        /// <summary>
        /// Recalculates the daily statistic tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual void RecalculateDailyStatisticTasks()
        {
            _taskOwnerDay.RecalculateDailyStatisticTasks();
        }

        /// <summary>
        /// Locks this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual void Lock()
        {
            _taskOwnerDay.Lock();
        }

        /// <summary>
        /// Releases this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual void Release()
        {
            _taskOwnerDay.Release();
        }

        /// <summary>
        /// Sets the entity as dirty.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual void SetDirty()
        {
            _taskOwnerDay.SetDirty();
        }

        /// <summary>
        /// Adds the parent.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual void AddParent(ITaskOwner parent)
        {
            _taskOwnerDay.AddParent(parent);
        }

        /// <summary>
        /// Removes the parent.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual void RemoveParent(ITaskOwner parent)
        {
            _taskOwnerDay.RemoveParent(parent);
        }

        public virtual void ClearParents()
        {
            _taskOwnerDay.ClearParents();
        }

        /// <summary>
        /// Gets the current date.
        /// </summary>
        /// <value>The current date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual DateOnly CurrentDate
        {
            get { return _taskOwnerDay.CurrentDate; }
        }

        public virtual OpenForWork OpenForWork
        {
            get
            {
                return _taskOwnerDay.OpenForWork;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is locked.
        /// </summary>
        /// <value><c>true</c> if this instance is locked; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual bool IsLocked
        {
            get { return _taskOwnerDay.IsLocked; }
        }

        /// <summary>
        /// Updates the name of the template.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public virtual void ClearTemplateName()
        {
            _taskOwnerDay.ClearTemplateName();
        }

        #endregion
    }
}
