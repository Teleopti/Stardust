using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Forecasting.Template;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using TemplateReference=Teleopti.Ccc.Domain.Forecasting.Template.TemplateReference;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Template for WorkloadDays
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2008-01-23
    /// </remarks>
    public class WorkloadDayTemplate : WorkloadDayBase, IWorkloadDayTemplate
    {
        private DateTime _createdDate;
        private string _name = string.Empty;
        private readonly IList<ITemplateTaskPeriod> _snapshotTaskPeriodList = new List<ITemplateTaskPeriod>();
        private int _versionNumber;
    	private DateTime _updatedDate;
    	private bool _templateVersionNumberIncreased;

    	/// <summary>
        /// Creates the specified created date.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="createdDate">The created date.</param>
        /// <param name="workload">The workload.</param>
        /// <param name="openHourList">The open hour list.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-01-23
        /// </remarks>
        public virtual void Create(string name, DateTime createdDate, IWorkload workload, IList<TimePeriod> openHourList)
        {
            InParameter.VerifyDateIsUtc(nameof(createdDate), createdDate);
            InParameter.NotNull(nameof(name), name);

            SetParent(workload);
            _createdDate = createdDate;
    		_updatedDate = createdDate;
            _name = name;

            base.Create(SkillDayTemplate.BaseDate, workload, openHourList);
               
        }
        /// <summary>
        /// Gets the workload date.
        /// </summary>
        /// <value>The workload date.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-01-23
        /// </remarks>
        public virtual DateTime CreatedDate => _createdDate;

	    /// <summary>
        /// Gets the template reference.
        /// </summary>
        /// <value>The template reference.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-09
        /// </remarks>
        public override ITemplateReference TemplateReference
        {
            // TODO: this implementation seems sick. Remove? /KlasM
            get { return new TemplateReference(Guid.Empty, 0, string.Empty, null); }
            protected set
            {}
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-12
        /// </remarks>
        public virtual string Name
        {
            get
            {
                return TemplateReference.DisplayName(DayOfWeek, _name, false);
            }
            set
            {
                _name = value;
            }
        }

        /// <summary>
        /// Gets the day of week.
        /// </summary>
        /// <value>The day of week.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-12
        /// </remarks>
        public virtual DayOfWeek? DayOfWeek
        {
            get
            {
                int wDayIndex = WeekdayIndex;
                if (!Enum.IsDefined(typeof(DayOfWeek), wDayIndex))
                    return null;
                return (DayOfWeek)wDayIndex;
            }
        }

        public virtual int VersionNumber
        {
            get { return _versionNumber; }
            protected set { _versionNumber = value; }
        }

    	///<summary>
    	/// Gets the updated date.
    	///</summary>
    	/// <value>The updated date.</value>
    	public virtual DateTime UpdatedDate
    	{
			get { return _updatedDate = _updatedDate == new DateTime() ? CreatedDate : _updatedDate; }
			protected set { _updatedDate = value; }
    	}

    	/// <summary>
        /// Gets or sets the index of the weekday.
        /// Just here for bi-directional mapping purposes (will be saved in column)
        /// </summary>
        /// <value>The index of the weekday.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-06-30
        /// </remarks>
        protected virtual int WeekdayIndex
        {
            get
            {
                IWorkload workload = Parent as IWorkload;
                if (workload == null || workload.TemplateWeekCollection.Count == 0) return -1;

                var item = workload.TemplateWeekCollection.SingleOrDefault(t => t.Value.Equals(this));
                if (item.Value == null) return -1;

                return item.Key;
            }
            set
            {
                //do nada
            }
        }

        /// <summary>
        /// Gets the sorted original task period list.
        /// </summary>
        /// <value>The sorted original task period list.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-26
        /// </remarks>
        public virtual ReadOnlyCollection<ITemplateTaskPeriod> SortedSnapshotTaskPeriodList
        {
            get
            {
                IList<ITemplateTaskPeriod> list = _snapshotTaskPeriodList
                    .OrderBy(tp => tp.Period.StartDateTime)
                    .ToList();
                return new ReadOnlyCollection<ITemplateTaskPeriod>(list);
            }
        }
        /// <summary>
        /// Updates the name of the template.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-14
        /// </remarks>
        public override void ClearTemplateName()
        {
        }


        /// <summary>
        /// Does the running smoothning.
        /// </summary>
        /// <param name="periods">The periods.</param>
        /// <param name="type">The type.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-27
        /// </remarks>
        public virtual void DoRunningSmoothing(int periods, TaskPeriodType type)
        {
            //vilken typ 
            //Hämta dictionary från SortedTaskList
            //You cannot do TaskPeriodSnapshotTyp.All on this method. How do we stop that? Exception, or do we fixit? 

            IDictionary<DateTimePeriod, double> numbers = ripSmoothingBasis(type);

            StatisticalSmoothing smoothing = new StatisticalSmoothing(numbers);
            numbers = smoothing.CalculateRunningAverage(periods);

            setSmoothingResult(numbers, type);
        }

        private void setSmoothingResult(IDictionary<DateTimePeriod, double> numbers, TaskPeriodType type)
        {
            foreach (ITemplateTaskPeriod taskPeriod in SortedTaskPeriodList)
            {
                switch (type)
                {
                    case TaskPeriodType.Tasks:
                        taskPeriod.SetTasks(numbers[taskPeriod.Period]);
                        break;
                    case TaskPeriodType.AverageTaskTime:
                        taskPeriod.AverageTaskTime = new TimeSpan((long)numbers[taskPeriod.Period]);
                        break;
                    case TaskPeriodType.AverageAfterTaskTime:
                        taskPeriod.AverageAfterTaskTime = new TimeSpan((long)numbers[taskPeriod.Period]);
                        break;
                }
            }
        }

        private IDictionary<DateTimePeriod, double> ripSmoothingBasis(TaskPeriodType type)
        {
            IDictionary<DateTimePeriod, double> numbers = new Dictionary<DateTimePeriod, double>();

            foreach (ITemplateTaskPeriod taskPeriod in _snapshotTaskPeriodList)
            {
                switch(type)
                {
                    case TaskPeriodType.Tasks:
                        numbers.Add(taskPeriod.Period,taskPeriod.Tasks);
                        break;
                    case TaskPeriodType.AverageTaskTime:
                        numbers.Add(taskPeriod.Period, taskPeriod.AverageTaskTime.Ticks);
                        break;
                    case TaskPeriodType.AverageAfterTaskTime:
                        numbers.Add(taskPeriod.Period, taskPeriod.AverageAfterTaskTime.Ticks);
                        break;
                }
                
            }
            return numbers;
        }

        /// <summary>
        /// Snapshots the template task period list.
        /// Copies from SortedTaskPeriodList to a snapshotlist
        /// It only copies the selected type Tasks, AverageTasksTime, AverageAfterTaskTime 
        /// Or All.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-27
        /// </remarks>
        public virtual void SnapshotTemplateTaskPeriodList(TaskPeriodType type)
        {
            //First case, need to check if list is zero, or if it is merged or splitted
            //This will copy everything. Is check zero nessicary?
            if (_snapshotTaskPeriodList.Count == 0 || 
                _snapshotTaskPeriodList.Count!=SortedTaskPeriodList.Count ||
                type == TaskPeriodType.All)
            {
                _snapshotTaskPeriodList.Clear();
                foreach (TemplateTaskPeriod taskPeriod in SortedTaskPeriodList)
                {
                    double tasks = taskPeriod.Tasks;
                    TimeSpan averageTaskTime = taskPeriod.AverageTaskTime;
                    TimeSpan averageAfterTaskTime = taskPeriod.AverageAfterTaskTime;

                    DateTimePeriod period = taskPeriod.Period;
                    _snapshotTaskPeriodList.Add(createTaskPeriod(tasks, averageTaskTime, averageAfterTaskTime, period));
                }
                
            }else if(type==TaskPeriodType.Tasks)
            {
                copyTasksToSnapShot();
            }
            else if (type==TaskPeriodType.AverageTaskTime)
            {
                copyAverageTaskTimeToSnapShot();   
            }
            else if (type==TaskPeriodType.AverageAfterTaskTime)
            {
               copyAverageAfterTaskTimeToSnapShot();
            }
        }

        /// <summary>
        /// Increases the version by one, should be used each time a change is made.
        /// This is needed so that WorkdloadDays can know if they are referring to a current or old template
        /// </summary>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-10-16
        /// </remarks>
        public virtual void IncreaseVersionNumber()
        {
        	_versionNumber++;
        	_templateVersionNumberIncreased = true;
        }

		public virtual void RefreshUpdatedDate()
		{
			if(_templateVersionNumberIncreased)
				_updatedDate = DateTime.UtcNow;
    	}

    	private void copyTasksToSnapShot()
        {
            int index = 0;
            foreach (TemplateTaskPeriod templateTaskPeriod in SortedTaskPeriodList)
            {
                double tasks = templateTaskPeriod.Tasks;
                TimeSpan averageTaskTime = _snapshotTaskPeriodList[index].AverageTaskTime;
                TimeSpan averageAfterTaskTime = _snapshotTaskPeriodList[index].AverageAfterTaskTime;

                DateTimePeriod period = templateTaskPeriod.Period;
                _snapshotTaskPeriodList[index] = createTaskPeriod(tasks, averageTaskTime, averageAfterTaskTime, period);
                index++;
            }
        }
        private void copyAverageAfterTaskTimeToSnapShot()
        {
            int index = 0;
            foreach (ITemplateTaskPeriod templateTaskPeriod in SortedTaskPeriodList)
            {
                double tasks = _snapshotTaskPeriodList[index].Tasks;
                TimeSpan averageTaskTime = _snapshotTaskPeriodList[index].AverageTaskTime;
                TimeSpan averageAfterTaskTime = templateTaskPeriod.AverageAfterTaskTime;

                DateTimePeriod period = templateTaskPeriod.Period;
                _snapshotTaskPeriodList[index] = createTaskPeriod(tasks, averageTaskTime, averageAfterTaskTime, period);
                index++;
            }
        }
        private void copyAverageTaskTimeToSnapShot()
        {
            int index = 0;
            foreach (TemplateTaskPeriod templateTaskPeriod in SortedTaskPeriodList)
            {
                double tasks = _snapshotTaskPeriodList[index].Tasks;
                TimeSpan averageTaskTime = templateTaskPeriod.AverageTaskTime;
                TimeSpan averageAfterTaskTime = _snapshotTaskPeriodList[index].AverageAfterTaskTime;

                DateTimePeriod period = templateTaskPeriod.Period;
                _snapshotTaskPeriodList[index] = createTaskPeriod(tasks, averageTaskTime, averageAfterTaskTime, period);
                index++;
            }
        }

        private static ITemplateTaskPeriod createTaskPeriod(double tasks, TimeSpan averageTaskTime, TimeSpan averageAfterTaskTime, DateTimePeriod period)
        {
            ITask newTask = new Task(tasks, averageTaskTime, averageAfterTaskTime);
            return new TemplateTaskPeriod(newTask,period);
        }

        #region Implementation of ICloneable

        public virtual object Clone()
        {
            return NoneEntityClone();
        }

        #endregion

        #region Implementation of ICloneableEntity<IWorkloadDayTemplate>

        public virtual IWorkloadDayTemplate NoneEntityClone()
        {
            WorkloadDayTemplate retobj = (WorkloadDayTemplate)MemberwiseClone();
            retobj.SetId(null);
            NoneEntityCloneTaskPeriodList(retobj);
            return retobj;
        }

        public virtual IWorkloadDayTemplate EntityClone()
        {
            WorkloadDayTemplate retobj = (WorkloadDayTemplate)MemberwiseClone();
            EntityCloneTaskPeriodList(retobj);
            return retobj;
        }

        #endregion
   
        #region Recalculations

        public override void RecalculateDailyTasks()
        {
            base.RecalculateDailyTasks();
            IncreaseVersionNumber();
        }

        public override void RecalculateDailyAverageTimes()
        {
            base.RecalculateDailyAverageTimes();
            IncreaseVersionNumber();
        }

        public override void ChangeOpenHours(IList<TimePeriod> openHourList)
        {
            base.ChangeOpenHours(openHourList);
            IncreaseVersionNumber();
        }
        #endregion
    }
}
