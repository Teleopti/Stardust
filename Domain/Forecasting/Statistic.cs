using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	/// <summary>
    /// Class for handling StatisticTask 
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2008-02-20
    /// </remarks>
    public class Statistic : IStatistic
	{
        private IWorkload _workload;

        /// <summary>
        /// Initializes a new instance of the <see cref="Statistic"/> class.
        /// </summary>
        /// <param name="workload">The workload.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-22
        /// </remarks>
        public Statistic(IWorkload workload)
        {
            _workload = workload;
        }

		public Statistic()
		{}

        /// <summary>
        /// Matches the specified Task to statisticTasks.
        /// </summary>
        /// <param name="workloadDays">The workload days.</param>
        /// <param name="statisticTasks">The statistic tasks.</param>
        /// <remarks>
        /// Created by: peterwe, zoet
        /// Created date: 2008-02-20
        /// </remarks>
        public void Match(IEnumerable<IWorkloadDayBase> workloadDays, IList<IStatisticTask> statisticTasks)
        {
            if (statisticTasks.Count == 0) return;
            
            var queueStatisticsProvider = new QueueStatisticsProvider(statisticTasks,new QueueStatisticsCalculator(_workload.QueueAdjustments));
            workloadDays.ForEach(w => w.SetQueueStatistics(queueStatisticsProvider));

            workloadDays.ForEach(wld =>
            {
                wld.RecalculateDailyStatisticTasks();
                wld.RecalculateDailyAverageStatisticTimes();
            });
        }

		public void Match(IWorkload workload, IEnumerable<IWorkloadDayBase> workloadDays, IList<IStatisticTask> statisticTasks)
		{
			_workload = workload;
			Match(workloadDays,statisticTasks);
		}

		/// <summary>
		/// Calculates the template days.
		/// </summary>
		/// <param name="workloadDays">The workload days.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: peterwe
		/// Created date: 2008-02-22
		/// </remarks>
		public IWorkload CalculateTemplateDays(IList<IWorkloadDayBase> workloadDays)
		{
			foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
			{
				var workloadDayTemplate = (IWorkloadDayTemplate)_workload.GetTemplate(TemplateTarget.Workload, day);

				string templateName =
						string.Format(CultureInfo.CurrentUICulture, "<{0}>",
									  CultureInfo.CurrentUICulture.DateTimeFormat.GetAbbreviatedDayName(day).ToUpper(
										  CultureInfo.CurrentUICulture));

				workloadDayTemplate.Create(templateName, DateTime.UtcNow, _workload,
										   workloadDayTemplate
											   .OpenHourList.ToList());

				calculateTemplateDay(workloadDayTemplate, getOnlyDaysOfGivenWeekDay(day, workloadDays));
			}
			return _workload;
		}

        /// <summary>
        /// Calculates the custom template day.
        /// </summary>
        /// <param name="workloadDays">The workload days.</param>
        /// <param name="dayIndex">Index of the day.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-27
        /// </remarks>
        public IWorkload CalculateCustomTemplateDay(IList<IWorkloadDayBase> workloadDays, int dayIndex)
        {
            IWorkloadDayTemplate workloadDayTemplate;
			if (workloadDays != null && workloadDays.Count == 0)
			{
				workloadDayTemplate = new WorkloadDayTemplate();
				workloadDayTemplate.Create(workloadDayTemplate.Name, DateTime.UtcNow, _workload, ((IWorkloadDayTemplate)_workload.GetTemplateAt(TemplateTarget.Workload, dayIndex)).OpenHourList);
			}
			else
				workloadDayTemplate = (IWorkloadDayTemplate) _workload.GetTemplateAt(TemplateTarget.Workload, dayIndex);
			calculateTemplateDay(workloadDayTemplate, workloadDays);
			_workload.SetTemplateAt(dayIndex, workloadDayTemplate);

        	return _workload;
        }

		public IWorkload ReloadCustomTemplateDay(IList<IWorkloadDayBase> workloadDays, int dayIndex)
		{
			var day = (DayOfWeek)Enum.GetValues(typeof(DayOfWeek)).GetValue(dayIndex);
			var workloadDayTemplate = (IWorkloadDayTemplate)_workload.GetTemplate(TemplateTarget.Workload, day);

			string templateName =
					string.Format(CultureInfo.CurrentUICulture, "<{0}>",
								  CultureInfo.CurrentUICulture.DateTimeFormat.GetAbbreviatedDayName(day).ToUpper(
									  CultureInfo.CurrentUICulture));

			workloadDayTemplate.Create(templateName, DateTime.UtcNow, _workload,
									   workloadDayTemplate
										   .OpenHourList.ToList());

			calculateTemplateDay(workloadDayTemplate, getOnlyDaysOfGivenWeekDay(day, workloadDays));
			_workload.SetTemplateAt(dayIndex, workloadDayTemplate);
			return _workload;
		}

		private static IEnumerable<IWorkloadDayBase> getOnlyDaysOfGivenWeekDay(DayOfWeek dayOfWeek, IEnumerable<IWorkloadDayBase> workloadDays)
        {
            return workloadDays.Where(w => w.CurrentDate.DayOfWeek == dayOfWeek);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void calculateTemplateDay(IWorkloadDayTemplate workloadDayTemplate, IEnumerable<IWorkloadDayBase> workloadDays, bool includeStatistics = false)
    	{
    		TimeZoneInfo raptorTimeZoneInfo = _workload.Skill.TimeZone;
            DateTime startDateTime = raptorTimeZoneInfo.SafeConvertTimeToUtc(SkillDayTemplate.BaseDate.Date);
            
    		//Create the list with all periods
    		IEnumerable<ITemplateTaskPeriod> taskPeriods = createExtendedTaskPeriodList(workloadDays,startDateTime);

    		//Group taskperiods on timeperiod.
			var times = from t in taskPeriods
						group t by t.Period.StartDateTime.Subtract(startDateTime)
							into g
							select new
									{
										Period = g.Key,
										Task = g.Average(t => t.StatisticTask.StatCalculatedTasks),
										SumTasks = g.Sum(t => t.StatisticTask.StatCalculatedTasks),
										AverageTaskTime = g.Sum(ac => ac.StatisticTask.StatAverageTaskTime.Ticks * ac.StatisticTask.StatCalculatedTasks),
										AverageAfterTaskTime = g.Sum(acw => acw.StatisticTask.StatAverageAfterTaskTime.Ticks * acw.StatisticTask.StatCalculatedTasks),
										StatisticTask = includeStatistics ? new StatisticTask
										{
											StatCalculatedTasks = g.Average(t => t.StatisticTask.StatCalculatedTasks),
											StatAbandonedTasks = g.Average(t => t.StatisticTask.StatAbandonedTasks),
											StatAnsweredTasks = g.Average(t => t.StatisticTask.StatAnsweredTasks),
											StatAverageTaskTimeSeconds = g.Sum(t => t.StatisticTask.StatAnsweredTasks) > 0d ?
												(g.Sum(t => t.StatisticTask.StatAverageTaskTimeSeconds * t.StatisticTask.StatAnsweredTasks) / g.Sum(t => t.StatisticTask.StatAnsweredTasks))
												: (g.Average(t=>t.StatisticTask.StatAverageTaskTimeSeconds)),
											StatAverageAfterTaskTimeSeconds = g.Sum(t => t.StatisticTask.StatAnsweredTasks) > 0d ?
												(g.Sum(t => t.StatisticTask.StatAverageAfterTaskTimeSeconds * t.StatisticTask.StatAnsweredTasks) / g.Sum(t => t.StatisticTask.StatAnsweredTasks))
												: (g.Average(t => t.StatisticTask.StatAverageAfterTaskTimeSeconds))
										} : new StatisticTask()
									};
			
    		//Create the template from the extended list
    		IList<ITemplateTaskPeriod> newList = new List<ITemplateTaskPeriod>();
    		foreach (var time in times)
    		{
    			double tasks = time.Task;
    			TimeSpan averageTaskTime = new TimeSpan((long) (time.AverageTaskTime/time.SumTasks));
    			TimeSpan averageAfterTaskTime = new TimeSpan((long) (time.AverageAfterTaskTime/time.SumTasks));
    			if (averageTaskTime.TotalSeconds < 1) averageTaskTime = new TimeSpan(1);
                
    			if (averageAfterTaskTime.TotalSeconds < 1) averageAfterTaskTime = new TimeSpan(1);
                
    			Task task = new Task(tasks, averageTaskTime, averageAfterTaskTime);

    			int defaultResoulution = _workload.Skill.DefaultResolution;
    			DateTimePeriod dateTimePeriod = new DateTimePeriod(startDateTime.Add(time.Period),
    			                                                   startDateTime.Add(time.Period).Add(TimeSpan.FromMinutes(defaultResoulution)));

    			newList.Add(includeStatistics
    			            	? new TemplateTaskPeriod(task, dateTimePeriod) {StatisticTask = time.StatisticTask}
    			            	: new TemplateTaskPeriod(task, dateTimePeriod));
    		}
			if (includeStatistics) workloadDayTemplate.SetTaskPeriodCollectionWithStatistics(newList);
			else workloadDayTemplate.SetTaskPeriodCollection(newList);
    		if (workloadDayTemplate.Workload.Skill.SkillType.ForecastSource!=ForecastSource.InboundTelephony &&
				workloadDayTemplate.Workload.Skill.SkillType.ForecastSource!=ForecastSource.Retail)
    		{
    			if (workloadDayTemplate.OpenForWork.IsOpen)
    			{
    				workloadDayTemplate.AverageTaskTime = workloadDayTemplate.AverageTaskTime;
    				workloadDayTemplate.AverageAfterTaskTime = workloadDayTemplate.AverageAfterTaskTime;
    			}
    		}
    	}

    	public IWorkloadDayBase GetTemplateWorkloadDay(IWorkloadDayTemplate workloadDayTemplate, IList<IWorkloadDayBase> workloadDays)
		{
    		if (workloadDayTemplate == null) throw new ArgumentNullException(nameof(workloadDayTemplate));
    		if (workloadDays == null) throw new ArgumentNullException(nameof(workloadDays));
			
			var workloadDay = new WorkloadDay();
			workloadDay.Create(SkillDayTemplate.BaseDate, _workload, workloadDayTemplate.OpenHourList);
			if (workloadDays.Count == 0)
				return workloadDay;
			calculateTemplateDay(workloadDayTemplate, workloadDays, true);
			var totalStatisticAverageTaskTimeSeconds = 0d;
			var totalStatisticAverageAfterTaskTimeSeconds = 0d;
    		var sumAnsweredTasks = 0d;
    		var taskPeriodList = workloadDayTemplate.TaskPeriodList;
			foreach (var day in taskPeriodList)
			{
				workloadDay.TotalStatisticCalculatedTasks += day.StatisticTask.StatCalculatedTasks;
				workloadDay.TotalStatisticAbandonedTasks += day.StatisticTask.StatAbandonedTasks;
				var answeredTask = day.StatisticTask.StatAnsweredTasks;
				sumAnsweredTasks += answeredTask;
				totalStatisticAverageTaskTimeSeconds += day.StatisticTask.StatAverageTaskTimeSeconds * answeredTask;
				totalStatisticAverageAfterTaskTimeSeconds += day.StatisticTask.StatAverageAfterTaskTimeSeconds * answeredTask;
			}
			if (taskPeriodList.Count != 0)
			{
				workloadDay.TotalStatisticAverageTaskTime = sumAnsweredTasks > 0d
				                                            	? TimeSpan.FromTicks((long) (totalStatisticAverageTaskTimeSeconds/sumAnsweredTasks))
				                                            	: TimeSpan.FromTicks((long)(taskPeriodList.Average(t => t.StatisticTask.StatAverageTaskTime.Ticks)));
				workloadDay.TotalStatisticAverageAfterTaskTime = sumAnsweredTasks > 0d
				                                                 	? TimeSpan.FromTicks((long)(totalStatisticAverageAfterTaskTimeSeconds/sumAnsweredTasks))
																	: TimeSpan.FromTicks((long)(taskPeriodList.Average(t => t.StatisticTask.StatAverageAfterTaskTime.Ticks)));
			}
    		workloadDay.SetTaskPeriodCollectionWithStatistics(taskPeriodList);
			return workloadDay;
		}

        /// <summary>
        /// Creates the extended task period list.
        /// </summary>
        /// <param name="workloadDays">The workload days.</param>
        /// <param name="startTimeTemplate">The start date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-26
        /// </remarks>
        private static IEnumerable<ITemplateTaskPeriod> createExtendedTaskPeriodList(IEnumerable<IWorkloadDayBase> workloadDays, DateTime startTimeTemplate)
        {
            
            //All taskperiods on skillday into one list and create dummy date
            IList<ITemplateTaskPeriod> taskPeriods = new List<ITemplateTaskPeriod>();
            foreach (IWorkloadDay workloadDay in workloadDays)
            {
                var currentUtcDate = TimeZoneHelper.ConvertToUtc(workloadDay.CurrentDate.Date,
                                                                 workloadDay.Workload.Skill.TimeZone);

                var initialDiff = startTimeTemplate.Subtract(currentUtcDate);

                foreach (ITemplateTaskPeriod taskPeriod in workloadDay.SortedTaskPeriodList)
                {
                    DateTimePeriod dateTimePeriod = taskPeriod.Period.MovePeriod(initialDiff);

                    TemplateTaskPeriod templateTaskPeriod = new TemplateTaskPeriod(taskPeriod.Task, dateTimePeriod);
                    templateTaskPeriod.StatisticTask.StatCalculatedTasks = taskPeriod.StatisticTask.StatCalculatedTasks;
                    templateTaskPeriod.StatisticTask.StatAverageTaskTimeSeconds =
                        taskPeriod.StatisticTask.StatAverageTaskTimeSeconds;
                    templateTaskPeriod.StatisticTask.StatAverageAfterTaskTimeSeconds =
                        taskPeriod.StatisticTask.StatAverageAfterTaskTimeSeconds;
                	templateTaskPeriod.StatisticTask.StatAbandonedTasks = taskPeriod.StatisticTask.StatAbandonedTasks;
                	templateTaskPeriod.StatisticTask.StatAnsweredTasks = taskPeriod.StatisticTask.StatAnsweredTasks;
                    taskPeriods.Add(templateTaskPeriod);
                }
            }
            return taskPeriods;
        }

        /// <summary>
        /// Matches the specified target skill staff periods. Adds the statistics information to the skill staff period.
        /// </summary>
        /// <param name="targetSkillStaffPeriods">The target skill staff periods.</param>
        /// <param name="templateTaskPeriodsWithStatistics">The template task periods with statistics.</param>
        /// <param name="activeAgentCountCollection">The active agent count collection.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-22
        /// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Match(IList<ISkillStaffPeriod> targetSkillStaffPeriods, IList<ITemplateTaskPeriod> templateTaskPeriodsWithStatistics, IEnumerable<IActiveAgentCount> activeAgentCountCollection)
        {
            foreach (ISkillStaffPeriod skillStaffPeriod in targetSkillStaffPeriods)
            {
                ISkillDay skillDay = skillStaffPeriod.SkillDay;
                DateTimePeriod dateTimePeriod = skillStaffPeriod.Period;
                ITemplateTaskPeriod taskPeriod =
                    templateTaskPeriodsWithStatistics.FirstOrDefault(
                        t => t.Period.StartDateTime == dateTimePeriod.StartDateTime);
                if (taskPeriod != null)
                    skillStaffPeriod.StatisticTask = taskPeriod.StatisticTask.GetStatisticsWithPercentage(skillDay.SkillDayCalculator.GetPercentageForInterval(skillDay.Skill, dateTimePeriod));

                IActiveAgentCount activeAgentCount =
                    activeAgentCountCollection.FirstOrDefault(a => dateTimePeriod.Contains(a.Interval));
                skillStaffPeriod.ActiveAgentCount = new ActiveAgentCount();
                if (activeAgentCount != null) skillStaffPeriod.ActiveAgentCount.ActiveAgents = activeAgentCount.ActiveAgents;
            }
        }
    }
}
