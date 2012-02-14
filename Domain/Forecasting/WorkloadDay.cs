﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Interfaces.Domain;

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
            var templateGuid = workloadDayTemplate.Id ?? Guid.Empty;
            _templateReference = new WorkloadDayTemplateReference(templateGuid, workloadDayTemplate.VersionNumber, workloadDayTemplate.Name, workloadDayTemplate.DayOfWeek, workload)
                                 	{UpdatedDate = workloadDayTemplate.UpdatedDate};
        	Create(workloadDate, workload, workloadDayTemplate.OpenHourList);

            ApplyTemplate(workloadDayTemplate);
        }

        /// <summary>
        /// Applies the template.
        /// </summary>
        /// <param name="workloadDayTemplate">The workload day template.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-03
        /// </remarks>
        public virtual void ApplyTemplate(IWorkloadDayTemplate workloadDayTemplate)
        {
            double tasks = Tasks;
            Percent campaignTasks = CampaignTasks;
            Percent campaignAfterTasksTime = CampaignAfterTaskTime;
            Percent campaignTasksTime = CampaignTaskTime;
            TimeSpan originalAverageTaskTime = AverageTaskTime;
            TimeSpan originalAfterTaskTime = AverageAfterTaskTime;

            Close();
            SetOpenHours(workloadDayTemplate.OpenHourList);
            SplitTemplateTaskPeriods(new List<ITemplateTaskPeriod>(TaskPeriodList));

            double taskTimeFactor = TaskTimeFactorTaskTime(workloadDayTemplate);
            double taskAfterTaskTimeFactor = TaskTimeFactorAfterTaskTime(workloadDayTemplate);

            var timeZone = Workload.Skill.TimeZone;
            Lock();
            foreach (var keyValuePair in workloadDayTemplate.SortedTaskPeriodList)
            {
                var localTemplatePeriod = keyValuePair.Period.TimePeriod(timeZone);
                var taskPeriods =
                    TaskPeriodList.Where(t => localTemplatePeriod.Contains(t.Period.TimePeriod(timeZone))).ToList();
                int taskPeriodCount = taskPeriods.Count;
                if (taskPeriodCount == 2 &&
                    taskPeriods[0].Period.TimePeriod(timeZone) == taskPeriods[1].Period.TimePeriod(timeZone))
                {
                    //Do nothing as we wan't to set the same values to both periods in this case (ambigious periods due to change from DST)
                }
                else if (taskPeriodCount > 1)
                {
                    MergeTemplateTaskPeriods(taskPeriods);
                    taskPeriods =
                        TaskPeriodList.Where(t => localTemplatePeriod.StartTime == t.Period.TimePeriod(timeZone).StartTime)
                            .ToList();
                    taskPeriodCount = taskPeriods.Count;
                }
                if (taskPeriodCount == 0) continue;

                
                foreach (ITemplateTaskPeriod newTaskPeriod in taskPeriods)
                {
                    newTaskPeriod.Tasks = keyValuePair.Task.Tasks;
                    newTaskPeriod.AverageTaskTime = keyValuePair.AverageTaskTime;
                    newTaskPeriod.AverageAfterTaskTime = keyValuePair.AverageAfterTaskTime;
                    newTaskPeriod.AverageTaskTime = TimeSpan.FromSeconds(newTaskPeriod.AverageTaskTime.TotalSeconds * taskTimeFactor);
                    newTaskPeriod.AverageAfterTaskTime = TimeSpan.FromSeconds(newTaskPeriod.AverageAfterTaskTime.TotalSeconds * taskAfterTaskTimeFactor); newTaskPeriod.Tasks = keyValuePair.Task.Tasks;
				}
            }
            if (isOpenForIncomingWork() && tasks > 0)
            {
                CampaignTasks = campaignTasks;
                CampaignTaskTime = campaignTasksTime;
                CampaignAfterTaskTime = campaignAfterTasksTime;
                Tasks = tasks;
                AverageTaskTime = originalAverageTaskTime;
                AverageAfterTaskTime = originalAfterTaskTime;
            }
            Release();
            Guid templateGuid = workloadDayTemplate.Id ?? Guid.Empty;
            _templateReference = new WorkloadDayTemplateReference(templateGuid, workloadDayTemplate.VersionNumber, workloadDayTemplate.Name, workloadDayTemplate.DayOfWeek,
                workloadDayTemplate.Workload) {UpdatedDate = workloadDayTemplate.UpdatedDate};

        	//Apply the original volumes
            if (isOpenForIncomingWork())
            {
                if (tasks > 0)
                {
                    Lock();
                    CampaignTasks = campaignTasks;
                    CampaignTaskTime = campaignTasksTime;
                    CampaignAfterTaskTime = campaignAfterTasksTime;
                    Tasks = tasks;
                    AverageTaskTime = originalAverageTaskTime;
                    AverageAfterTaskTime = originalAfterTaskTime;
                    Release();
                }
                else
                {
                    OnTasksChanged();
                }
            }

            ResetStatistics();
        }

        private bool isOpenForIncomingWork()
        {
            return !IsClosed || IsEmailWorkload;
        }

        private double TaskTimeFactorTaskTime(IWorkloadDayTemplate workloadDayTemplate)
        {
            double taskTimesAverageTalkTime = 0;

            foreach (TemplateTaskPeriod taskPeriod in workloadDayTemplate.TaskPeriodList)
            {
                taskTimesAverageTalkTime += taskPeriod.Tasks * taskPeriod.AverageTaskTime.TotalSeconds;
            }

            TimeSpan templateHandlingTime = TimeSpan.FromSeconds(taskTimesAverageTalkTime);
            double templateTasks = workloadDayTemplate.TaskPeriodList.Sum(t => t.Tasks);

            TimeSpan templateAvgHandlingTime = templateHandlingTime;
            if (templateTasks > 0)
                templateAvgHandlingTime = TimeSpan.FromSeconds(templateHandlingTime.TotalSeconds / templateTasks);

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
                taskTimesAverageAfterTalkTime += taskPeriod.Tasks * taskPeriod.AverageAfterTaskTime.TotalSeconds;
            }

            TimeSpan templateAfterTaskTime = TimeSpan.FromSeconds(taskTimesAverageAfterTalkTime);
            double templateTasks = workloadDayTemplate.TaskPeriodList.Sum(t => t.Tasks);

            TimeSpan templateAveragegAfterTaskTime = templateAfterTaskTime;
            if (templateTasks > 0)
                templateAveragegAfterTaskTime = TimeSpan.FromSeconds(templateAfterTaskTime.TotalSeconds / templateTasks);

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
        public override void UpdateTemplateName()
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
    }
}