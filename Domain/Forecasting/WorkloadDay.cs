using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	/// <summary>
    /// Class for holding one day of workload data
    /// </summary>
    public class WorkloadDay : WorkloadDayBase, IWorkloadDay
    {
        private ITemplateReference _templateReference = new WorkloadDayTemplateReference(); // this new() is to VerifyBudget test work
        private string _annotation;
        
        /// <summary>
        /// Creates the specified date.
        /// </summary>
        /// <param name="workloadDate">The workload date.</param>
        /// <param name="workload">The workload.</param>
        /// <param name="openHourList">The open hour list.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-01-23
        /// </remarks>
        public new virtual void Create(DateOnly workloadDate, IWorkload workload, IList<TimePeriod> openHourList)
        {
            base.Create(workloadDate, workload, openHourList);
            ((WorkloadDayTemplateReference)_templateReference).Workload = workload;
        }


        /// <summary>
        /// Creates the worklad day.
        /// </summary>
        /// <param name="workloadDate">The date.</param>
        /// <param name="workload">The workload.</param>
        /// <param name="workloadDayTemplate">The workload day template.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created WorkloadDate: 2008-01-30
        /// </remarks>
        public virtual void CreateFromTemplate(DateOnly workloadDate, IWorkload workload, IWorkloadDayTemplate workloadDayTemplate)
        {
            var templateGuid = workloadDayTemplate.Id.GetValueOrDefault();
            _templateReference = new WorkloadDayTemplateReference(templateGuid, workloadDayTemplate.VersionNumber, workloadDayTemplate.Name, workloadDayTemplate.DayOfWeek, workload) { UpdatedDate = workloadDayTemplate.UpdatedDate };
            Create(workloadDate, workload, workloadDayTemplate.OpenHourList);

            copyValuesFromTemplate(workloadDayTemplate, day => day.Lock(), day => day.Release());
        }

        private void copyValuesFromTemplate(IWorkloadDayTemplate workloadDayTemplate, Action<IWorkloadDayBase> lockAction, Action<IWorkloadDayBase> releaseAction)
        {
            Close();
            SetOpenHours(workloadDayTemplate.OpenHourList);
            innerSplitTemplateTaskPeriods(new List<ITemplateTaskPeriod>(TaskPeriodList), lockAction, releaseAction);

            double taskTimeFactor = TaskTimeFactorTaskTime(workloadDayTemplate);
            double taskAfterTaskTimeFactor = TaskTimeFactorAfterTaskTime(workloadDayTemplate);

            var timeZone = Workload.Skill.TimeZone;
            lockAction(this);
            foreach (var keyValuePair in workloadDayTemplate.SortedTaskPeriodList)
            {
                var localTemplatePeriod = keyValuePair.Period.TimePeriod(timeZone);
				var taskPeriodAndLocalTime = TaskPeriodList.Select(t => new {t, localTime = t.Period.TimePeriod(timeZone)}).ToArray();
	            var taskPeriods = taskPeriodAndLocalTime.Where(t => localTemplatePeriod.Contains(t.localTime)).ToArray();
                int taskPeriodCount = taskPeriods.Length;
                if (taskPeriodCount == 2 &&
                    taskPeriods[0].localTime == taskPeriods[1].localTime)
                {
                    //Do nothing as we wan't to set the same values to both periods in this case (ambigious periods due to change from DST)
                }
                else if (taskPeriodCount > 1)
                {
                    innerMergeTemplateTaskPeriods(taskPeriods.Select(t => t.t).ToArray(), lockAction, releaseAction);
                    taskPeriods =
						taskPeriodAndLocalTime.Where(t => localTemplatePeriod.StartTime == t.localTime.StartTime)
                            .ToArray();
                    taskPeriodCount = taskPeriods.Length;
                }
                if (taskPeriodCount == 0) continue;

                foreach (var newTaskPeriod in taskPeriods)
                {
                    newTaskPeriod.t.Tasks = keyValuePair.Task.Tasks;
                    newTaskPeriod.t.AverageTaskTime = keyValuePair.AverageTaskTime;
                    newTaskPeriod.t.AverageAfterTaskTime = keyValuePair.AverageAfterTaskTime;
                    newTaskPeriod.t.AverageTaskTime = TimeSpan.FromSeconds(newTaskPeriod.t.AverageTaskTime.TotalSeconds * taskTimeFactor);
                    newTaskPeriod.t.AverageAfterTaskTime = TimeSpan.FromSeconds(newTaskPeriod.t.AverageAfterTaskTime.TotalSeconds * taskAfterTaskTimeFactor);
                }
            }

            releaseAction(this);
            Guid templateGuid = workloadDayTemplate.Id.GetValueOrDefault();
            _templateReference = new WorkloadDayTemplateReference(templateGuid, workloadDayTemplate.VersionNumber, workloadDayTemplate.Name, workloadDayTemplate.DayOfWeek,
                workloadDayTemplate.Workload) { UpdatedDate = workloadDayTemplate.UpdatedDate };

            ResetStatistics();
        }

		public override void DistributeTasks(IEnumerable<ITemplateTaskPeriod> sortedTemplateTaskPeriods)
		{
			var originalCampaignTasks = CampaignTasks;
			var originalTasks = Tasks;
			var originalAverageTaskTime = AverageTaskTime;
			var originalAfterTaskTime = AverageAfterTaskTime;

			Close();
			var template = (IWorkloadDayTemplate)Workload.GetTemplateAt(TemplateTarget.Workload, (int)CurrentDate.DayOfWeek);
			SetOpenHours(template.OpenHourList);
			innerSplitTemplateTaskPeriods(new List<ITemplateTaskPeriod>(TaskPeriodList), day => day.Lock(), day => { });

			var timeZone = Workload.Skill.TimeZone;

			Lock();

			foreach (var templateTaskPeriod in sortedTemplateTaskPeriods)
			{
				var localTemplatePeriod = templateTaskPeriod.Period.TimePeriod(timeZone);
				var taskPeriods = TaskPeriodList.Where(t => localTemplatePeriod.Contains(t.Period.TimePeriod(timeZone))).ToList();
				var taskPeriodCount = taskPeriods.Count;
				if (taskPeriodCount == 2 && taskPeriods[0].Period.TimePeriod(timeZone) == taskPeriods[1].Period.TimePeriod(timeZone))
				{
					//Do nothing as we wan't to set the same values to both periods in this case (ambigious periods due to change from DST)
				}
				else if (taskPeriodCount > 1)
				{
					innerMergeTemplateTaskPeriods(taskPeriods, day => day.Lock(), day => { });
					taskPeriods =
						TaskPeriodList.Where(t => localTemplatePeriod.StartTime == t.Period.StartDateTimeLocal(timeZone).TimeOfDay)
							.ToList();
					taskPeriodCount = taskPeriods.Count;
				}
				if (taskPeriodCount == 0) continue;

				foreach (var newTaskPeriod in taskPeriods)
				{
					newTaskPeriod.Tasks = templateTaskPeriod.Task.Tasks;
				}
			}

			if (isOpenForIncomingWork())
			{
				CampaignTasks = originalCampaignTasks;
				Tasks = originalTasks;
				AverageTaskTime = originalAverageTaskTime;
				AverageAfterTaskTime = originalAfterTaskTime;

				if (Math.Abs(Tasks) < 0.0001)
				{
					AverageTaskTime = TimeSpan.Zero;
					AverageAfterTaskTime = TimeSpan.Zero;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public virtual void ApplyTemplate(IWorkloadDayTemplate workloadDayTemplate, Action<IWorkloadDayBase> lockAction, Action<IWorkloadDayBase> releaseAction)
        {
            double tasks = Tasks;
            Percent campaignTasks = CampaignTasks;
            Percent campaignAfterTasksTime = CampaignAfterTaskTime;
            Percent campaignTasksTime = CampaignTaskTime;
            TimeSpan originalAverageTaskTime = AverageTaskTime;
            TimeSpan originalAfterTaskTime = AverageAfterTaskTime;
            double tasksSum = 0.0;
            double averageTaskTimeSum = 0.0;
            double averageAfterTaskTimeSum = 0.0;
            IList<ITemplateTaskPeriod> taskPeriods = null;

            Close();
            SetOpenHours(workloadDayTemplate.OpenHourList);
            innerSplitTemplateTaskPeriods(new List<ITemplateTaskPeriod>(TaskPeriodList), lockAction, releaseAction);

            double taskTimeFactor = TaskTimeFactorTaskTime(workloadDayTemplate);
            double taskAfterTaskTimeFactor = TaskTimeFactorAfterTaskTime(workloadDayTemplate);

            var timeZone = Workload.Skill.TimeZone;
            lockAction(this);
            foreach (var keyValuePair in workloadDayTemplate.SortedTaskPeriodList)
            {
                var localTemplatePeriod = keyValuePair.Period.TimePeriod(timeZone);
                 taskPeriods =
                    TaskPeriodList.Where(t => localTemplatePeriod.Contains(t.Period.TimePeriod(timeZone))).ToList();
                int taskPeriodCount = taskPeriods.Count;
                if (taskPeriodCount == 2 &&
                    taskPeriods[0].Period.TimePeriod(timeZone) == taskPeriods[1].Period.TimePeriod(timeZone))
                {
                    //Do nothing as we wan't to set the same values to both periods in this case (ambigious periods due to change from DST)
                }
                else if (taskPeriodCount > 1)
                {
                    innerMergeTemplateTaskPeriods(taskPeriods, lockAction, releaseAction);
                    taskPeriods =
                        TaskPeriodList.Where(t => localTemplatePeriod.StartTime == t.Period.StartDateTimeLocal(timeZone).TimeOfDay)
                            .ToList();
                    taskPeriodCount = taskPeriods.Count;
                }
                if (taskPeriodCount == 0) continue;
                
                foreach (ITemplateTaskPeriod newTaskPeriod in taskPeriods)
                {
                    tasksSum += keyValuePair.Task.Tasks;
                    newTaskPeriod.Tasks = keyValuePair.Task.Tasks;
                    newTaskPeriod.AverageTaskTime = keyValuePair.AverageTaskTime;
                    newTaskPeriod.AverageAfterTaskTime = keyValuePair.AverageAfterTaskTime;

                    if (newTaskPeriod.AverageTaskTime.TotalSeconds == 0)
                        newTaskPeriod.AverageTaskTime = TimeSpan.FromSeconds(taskTimeFactor);
                    else
                        newTaskPeriod.AverageTaskTime = TimeSpan.FromSeconds(newTaskPeriod.AverageTaskTime.TotalSeconds * taskTimeFactor);

                    averageTaskTimeSum += newTaskPeriod.AverageTaskTime.TotalSeconds * taskTimeFactor * keyValuePair.Task.Tasks;
                    
                    if (newTaskPeriod.AverageAfterTaskTime.TotalSeconds == 0)
                        newTaskPeriod.AverageAfterTaskTime = TimeSpan.FromSeconds(taskAfterTaskTimeFactor);
                    else
                        newTaskPeriod.AverageAfterTaskTime = TimeSpan.FromSeconds(newTaskPeriod.AverageAfterTaskTime.TotalSeconds * taskAfterTaskTimeFactor);

                    averageAfterTaskTimeSum += newTaskPeriod.AverageAfterTaskTime.TotalSeconds * taskAfterTaskTimeFactor * keyValuePair.Task.Tasks;
                }
            }

            if (isOpenForIncomingWork())
            {
                CampaignTasks = campaignTasks;
                CampaignTaskTime = campaignTasksTime;
				CampaignAfterTaskTime = campaignAfterTasksTime;

                if (tasks == 0 && originalAfterTaskTime.TotalSeconds == 0 && originalAverageTaskTime.TotalSeconds == 0)
                {
                    Tasks = tasksSum;
                    if (tasksSum != 0)
                    {
                        setAverageTaskTimeOnEachTaskPeriod(TimeSpan.FromSeconds(averageTaskTimeSum / tasksSum), taskPeriods);
                        setAverageAfterTaskTimeOnEachTaskPeriod(TimeSpan.FromSeconds(averageAfterTaskTimeSum / tasksSum), taskPeriods);
                    }
                    else
                    {
                        AverageTaskTime = TimeSpan.Zero;
                        AverageAfterTaskTime = TimeSpan.Zero;
                    }
                }
                else
                {
                    Tasks = tasks;
                    AverageTaskTime = originalAverageTaskTime;
                    AverageAfterTaskTime = originalAfterTaskTime;
                }
            }
            releaseAction(this);
            Guid templateGuid = workloadDayTemplate.Id.GetValueOrDefault();
            _templateReference = new WorkloadDayTemplateReference(templateGuid, workloadDayTemplate.VersionNumber, workloadDayTemplate.Name, workloadDayTemplate.DayOfWeek,
                workloadDayTemplate.Workload) { UpdatedDate = workloadDayTemplate.UpdatedDate };

            //Apply the original volumes
            if (isOpenForIncomingWork())
            {
                lockAction(this);
                CampaignTasks = campaignTasks;
                CampaignTaskTime = campaignTasksTime;
				CampaignAfterTaskTime = campaignAfterTasksTime;

                if (tasks == 0 && originalAfterTaskTime.TotalSeconds == 0 && originalAverageTaskTime.TotalSeconds == 0)
                {
                    Tasks = tasksSum;
                    if (tasksSum != 0)
                    {
                        setAverageTaskTimeOnEachTaskPeriod(TimeSpan.FromSeconds(averageTaskTimeSum / tasksSum), taskPeriods);
                        setAverageAfterTaskTimeOnEachTaskPeriod(TimeSpan.FromSeconds(averageAfterTaskTimeSum / tasksSum), taskPeriods);
                    }
                    else
                    {
                        AverageTaskTime = TimeSpan.Zero;
                        AverageAfterTaskTime = TimeSpan.Zero;
                    }
                }
                else
                {
                    Tasks = tasks;
                    AverageTaskTime = originalAverageTaskTime;
                    AverageAfterTaskTime = originalAfterTaskTime;
                }
                
                releaseAction(this);
            }

            ResetStatistics();
        }
        
        private void setAverageTaskTimeOnEachTaskPeriod(TimeSpan averageTaskTime, IList<ITemplateTaskPeriod> taskPeriods)
        {
            foreach (var taskPeriod in taskPeriods)
            {
                taskPeriod.AverageTaskTime = averageTaskTime;
            }
        }

        private void setAverageAfterTaskTimeOnEachTaskPeriod(TimeSpan averageAfterTaskTime, IList<ITemplateTaskPeriod> taskPeriods)
        {
            foreach (var taskPeriod in taskPeriods)
            {
                taskPeriod.AverageAfterTaskTime = averageAfterTaskTime;
            }
        }

        private bool isOpenForIncomingWork()
        {
            return OpenForWork.IsOpenForIncomingWork;
        }

        private double TaskTimeFactorTaskTime(IWorkloadDayTemplate workloadDayTemplate)
        {
            double taskTimesAverageTalkTime = 0;

            foreach (TemplateTaskPeriod taskPeriod in workloadDayTemplate.TaskPeriodList)
            {

                if (taskPeriod.AverageTaskTime.TotalSeconds == 0)
                    taskTimesAverageTalkTime += taskPeriod.Tasks;
                else
                    taskTimesAverageTalkTime += taskPeriod.Tasks * taskPeriod.AverageTaskTime.TotalSeconds;
            }

            TimeSpan templateHandlingTime = TimeSpan.FromSeconds(taskTimesAverageTalkTime);
            double templateTasks = workloadDayTemplate.TaskPeriodList.Sum(t => t.Tasks);

            TimeSpan templateAvgHandlingTime = templateHandlingTime;
            if (templateTasks > 0)
                templateAvgHandlingTime = TimeSpan.FromSeconds(templateHandlingTime.TotalSeconds / templateTasks);
            else
            {
                foreach (TemplateTaskPeriod taskPeriod in workloadDayTemplate.TaskPeriodList)
                {
                    taskTimesAverageTalkTime += taskPeriod.AverageTaskTime.TotalSeconds;
                }
                if (workloadDayTemplate.TaskPeriodList.Count > 0)
                    templateAvgHandlingTime = TimeSpan.FromSeconds(taskTimesAverageTalkTime / workloadDayTemplate.TaskPeriodList.Count);
            }

            double taskTimeFactor = 1;
            if (AverageTaskTime.TotalSeconds > 0 && templateAvgHandlingTime.TotalSeconds > 0)
                taskTimeFactor = AverageTaskTime.TotalSeconds / templateAvgHandlingTime.TotalSeconds;
            return taskTimeFactor;
        }

        private double TaskTimeFactorAfterTaskTime(IWorkloadDayTemplate workloadDayTemplate)
        {
            double taskTimesAverageAfterTalkTime = 0;

            foreach (TemplateTaskPeriod taskPeriod in workloadDayTemplate.TaskPeriodList)
            {
                if(taskPeriod.AverageAfterTaskTime.TotalSeconds == 0)
                    taskTimesAverageAfterTalkTime += taskPeriod.Tasks;
                else 
                    taskTimesAverageAfterTalkTime += taskPeriod.Tasks * taskPeriod.AverageAfterTaskTime.TotalSeconds;
            }

            TimeSpan templateAfterTaskTime = TimeSpan.FromSeconds(taskTimesAverageAfterTalkTime);
            double templateTasks = workloadDayTemplate.TaskPeriodList.Sum(t => t.Tasks);

            TimeSpan templateAveragegAfterTaskTime = templateAfterTaskTime;
            if (templateTasks > 0)
                templateAveragegAfterTaskTime = TimeSpan.FromSeconds(templateAfterTaskTime.TotalSeconds / templateTasks);
            else
            {
                foreach (TemplateTaskPeriod taskPeriod in workloadDayTemplate.TaskPeriodList)
                {
                    taskTimesAverageAfterTalkTime += taskPeriod.AverageTaskTime.TotalSeconds;
                }
                if (workloadDayTemplate.TaskPeriodList.Count > 0)
                    templateAveragegAfterTaskTime = TimeSpan.FromSeconds(taskTimesAverageAfterTalkTime / workloadDayTemplate.TaskPeriodList.Count);
            }


            double taskTimeFactor = 1;
            if (AverageAfterTaskTime.TotalSeconds > 0 && templateAveragegAfterTaskTime.TotalSeconds > 0)
                taskTimeFactor = AverageAfterTaskTime.TotalSeconds / templateAveragegAfterTaskTime.TotalSeconds;

            return taskTimeFactor;
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
            // this really means that when the day is updated, you have broken the reference to the original template
            _templateReference = new WorkloadDayTemplateReference(Guid.Empty, 0, string.Empty, null, null);
        }


        /// <summary>
        /// Gets or sets the total tasks.
        /// </summary>
        /// <value>The total tasks.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-14
        /// </remarks>
        public override double Tasks
        {
            get
            {
                return base.Tasks;
            }
            set
            {
                //This might need some refactoring, but 
                //would be good if someone has a better solution
                //templateName should NOT be changed if we come
                //from here
                ITemplateReference templateReference = _templateReference;
                base.Tasks = value;
                if (value > 0)
                    _templateReference = templateReference;
            }
        }

        /// <summary>
        /// Gets or sets the average after task time.
        /// </summary>
        /// <value>The average after task time.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-14
        /// </remarks>
        public override TimeSpan AverageAfterTaskTime
        {
            get
            {
                return base.AverageAfterTaskTime;
            }
            set
            {
                //better solution?
                ITemplateReference templateRef = _templateReference;
                base.AverageAfterTaskTime = value;
                _templateReference = templateRef;
            }
        }

        /// <summary>
        /// Gets or sets the average task time.
        /// </summary>
        /// <value>The average task time.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-14
        /// </remarks>
        public override TimeSpan AverageTaskTime
        {
            get
            {
                return base.AverageTaskTime;
            }
            set
            {
                //better solution?
                ITemplateReference templateRef = _templateReference;
                base.AverageTaskTime = value;
                _templateReference = templateRef;
            }
        }

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
            get { return _templateReference; }
            protected set { _templateReference = value; }
        }


        /// <summary>
        /// Gets or sets the annotation.
        /// </summary>
        /// <value>The annotation.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-10-14
        /// </remarks>
        public virtual string Annotation
        {
            get { return _annotation; }
            set
            {
                _annotation = value;
                OnTasksChanged();
            }
        }

        public virtual object Clone()
        {
            return EntityClone();
        }


        public virtual IWorkloadDay EntityClone()
        {
            WorkloadDay newWorkloadDay = (WorkloadDay)MemberwiseClone();
            EntityCloneTaskPeriodList(newWorkloadDay);
            return newWorkloadDay;
        }

        public virtual IWorkloadDay NoneEntityClone()
        {
            WorkloadDay newWorkloadDay = (WorkloadDay)MemberwiseClone();
            newWorkloadDay.SetId(null);

            NoneEntityCloneTaskPeriodList(newWorkloadDay);
            return newWorkloadDay;
        }

		public virtual IWorkloadDay MakeCopyAndNewParentList()
		{
			var ret = (WorkloadDay)EntityClone();
			ret.Parents = new List<ITaskOwner>();
			return ret;
		}
    }
}