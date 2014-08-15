using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public interface IStatistic
	{
		void Match(IEnumerable<IWorkloadDayBase> workloadDays, IList<IStatisticTask> statisticTasks);

		void Match(IWorkload workload, IEnumerable<IWorkloadDayBase> workloadDays, IList<IStatisticTask> statisticTasks);

		IWorkload CalculateTemplateDays(IList<IWorkloadDayBase> workloadDays);

		IWorkload CalculateCustomTemplateDay(IList<IWorkloadDayBase> workloadDays, int dayIndex);

		IWorkloadDayBase GetTemplateWorkloadDay(IWorkloadDayTemplate workloadDayTemplate, IList<IWorkloadDayBase> workloadDays);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		void Match(IList<ISkillStaffPeriod> targetSkillStaffPeriods, IList<ITemplateTaskPeriod> templateTaskPeriodsWithStatistics, IEnumerable<IActiveAgentCount> activeAgentCountCollection);

		void UpdateStatisticTask(IStatisticTask statisticTask, ITemplateTaskPeriod taskPeriod);
	}

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
            
            QueueStatisticsProvider queueStatisticsProvider = new QueueStatisticsProvider(statisticTasks,new QueueStatisticsCalculator(_workload.QueueAdjustments));
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

				calculateTemplateDay(workloadDayTemplate, GroupDays(day, workloadDays));
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

        /// <summary>
        /// Merges the statistic tasks.
        /// </summary>
        /// <param name="statisticTasksToMerge">The statistic tasks to merge.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-25
        /// </remarks>
        public static IStatisticTask MergeStatisticTasks(IEnumerable<IStatisticTask> statisticTasksToMerge)
        {
            if (statisticTasksToMerge.Count() == 1) return statisticTasksToMerge.First();

            double statAbandonedTasks = 0;
            double statAbandonedTasksWithinServiceLevel = 0;
            double statAbandonedShortTasks = 0;
            double statAnsweredTasks = 0;
			double statAnsweredTasksWithinSL = 0;
            double statCalculatedTasks = 0;
            double statOfferedTasks = 0;
            double statTaskAverageAfterTaskTimeSeconds = 0;
            double statTaskAverageTaskTimeSeconds = 0;
            double statTaskAverageQueueTimeSeconds = 0;
				double statTaskAverageHandleTimeSeconds = 0;

            double statOverflowInTasks = 0;
            double statOverflowOutTasks = 0;
 
            foreach (IStatisticTask statisticTask in statisticTasksToMerge)
            {
                statAbandonedTasks += statisticTask.StatAbandonedTasks;
                statAbandonedTasksWithinServiceLevel += statisticTask.StatAbandonedTasksWithinSL;
                statAbandonedShortTasks += statisticTask.StatAbandonedShortTasks;
                statAnsweredTasks += statisticTask.StatAnsweredTasks;
				statAnsweredTasksWithinSL += statisticTask.StatAnsweredTasksWithinSL;
                statCalculatedTasks += statisticTask.StatCalculatedTasks;
                statOfferedTasks += statisticTask.StatOfferedTasks;
                statTaskAverageAfterTaskTimeSeconds += statisticTask.StatAnsweredTasks * statisticTask.StatAverageAfterTaskTimeSeconds;
                statTaskAverageTaskTimeSeconds += statisticTask.StatAnsweredTasks * statisticTask.StatAverageTaskTimeSeconds;
                statTaskAverageQueueTimeSeconds += statisticTask.StatAnsweredTasks * statisticTask.StatAverageQueueTimeSeconds;
					 statTaskAverageHandleTimeSeconds  += statisticTask.StatAnsweredTasks * statisticTask.StatAverageHandleTimeSeconds;

                statOverflowInTasks += statisticTask.StatOverflowInTasks;
                statOverflowOutTasks += statisticTask.StatOverflowOutTasks;
            }

            IStatisticTask newStatisticTask = new StatisticTask();
            newStatisticTask.Interval = statisticTasksToMerge.First().Interval;
            newStatisticTask.StatAbandonedTasks = statAbandonedTasks;
            newStatisticTask.StatAbandonedTasksWithinSL = statAbandonedTasksWithinServiceLevel;
            newStatisticTask.StatAbandonedShortTasks = statAbandonedShortTasks;
            newStatisticTask.StatAnsweredTasks = statAnsweredTasks;
        	newStatisticTask.StatAnsweredTasksWithinSL = statAnsweredTasksWithinSL;
            newStatisticTask.StatCalculatedTasks = statCalculatedTasks;
            newStatisticTask.StatOfferedTasks = statOfferedTasks;
            
            newStatisticTask.StatOverflowInTasks = statOverflowInTasks;
            newStatisticTask.StatOverflowOutTasks = statOverflowOutTasks;

            if (statAnsweredTasks > 0)
            {
                newStatisticTask.StatAverageAfterTaskTimeSeconds = statTaskAverageAfterTaskTimeSeconds / statAnsweredTasks;
                newStatisticTask.StatAverageTaskTimeSeconds = statTaskAverageTaskTimeSeconds / statAnsweredTasks;
                newStatisticTask.StatAverageQueueTimeSeconds = statTaskAverageQueueTimeSeconds / statAnsweredTasks;
                newStatisticTask.StatAverageHandleTimeSeconds = statTaskAverageHandleTimeSeconds / statAnsweredTasks;
            }
            else
            {
                newStatisticTask.StatAverageAfterTaskTimeSeconds = 0;
                newStatisticTask.StatAverageTaskTimeSeconds = 0;
                newStatisticTask.StatAverageQueueTimeSeconds = 0;
            }

            return newStatisticTask;
        }

        /// <summary>
        /// Updates the statistic statisticTask.
        /// </summary>
        /// <param name="statisticTask">The task.</param>
        /// <param name="taskPeriod">The statisticTask period.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-26
        /// </remarks>
        public void UpdateStatisticTask(IStatisticTask statisticTask, ITemplateTaskPeriod taskPeriod)
        {
            taskPeriod.StatisticTask.Interval = statisticTask.Interval;
            taskPeriod.StatisticTask.StatAbandonedTasks = statisticTask.StatAbandonedTasks;
            taskPeriod.StatisticTask.StatAnsweredTasks = statisticTask.StatAnsweredTasks;
            taskPeriod.StatisticTask.StatOfferedTasks = statisticTask.StatOfferedTasks;
            taskPeriod.StatisticTask.StatCalculatedTasks = statisticTask.StatCalculatedTasks;

            taskPeriod.StatisticTask.StatAverageAfterTaskTimeSeconds =
                statisticTask.StatAverageAfterTaskTimeSeconds;
            taskPeriod.StatisticTask.StatAverageTaskTimeSeconds =
                statisticTask.StatAverageTaskTimeSeconds;

            taskPeriod.StatisticTask.StatAbandonedShortTasks = statisticTask.StatAbandonedShortTasks;
            taskPeriod.StatisticTask.StatAbandonedTasksWithinSL= statisticTask.StatAbandonedTasksWithinSL;
            taskPeriod.StatisticTask.StatAnsweredTasksWithinSL = statisticTask.StatAnsweredTasksWithinSL;
            taskPeriod.StatisticTask.StatOverflowInTasks = statisticTask.StatOverflowInTasks;
            taskPeriod.StatisticTask.StatOverflowOutTasks = statisticTask.StatOverflowOutTasks;

            taskPeriod.StatisticTask.StatAverageHandleTimeSeconds = statisticTask.StatAverageHandleTimeSeconds;
            taskPeriod.StatisticTask.StatAverageQueueTimeSeconds = statisticTask.StatAverageQueueTimeSeconds;
            taskPeriod.StatisticTask.StatAverageTimeLongestInQueueAbandonedSeconds = statisticTask.StatAverageTimeLongestInQueueAbandonedSeconds;
            taskPeriod.StatisticTask.StatAverageTimeLongestInQueueAnsweredSeconds = statisticTask.StatAverageTimeLongestInQueueAnsweredSeconds;
            taskPeriod.StatisticTask.StatAverageTimeToAbandonSeconds = statisticTask.StatAverageTimeToAbandonSeconds;
        }

        /// <summary>
        /// Groups the days o week.
        /// </summary>
        /// <param name="dayOfWeek">The day of week.</param>
        /// <param name="workloadDays">The workload days.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-20
        /// </remarks>
        private static IEnumerable<IWorkloadDayBase> GroupDays(DayOfWeek dayOfWeek, IEnumerable<IWorkloadDayBase> workloadDays)
        {
            return workloadDays.Where(w => w.CurrentDate.DayOfWeek == dayOfWeek);
        }

    	/// <summary>
    	/// Calculates the template days.
    	/// </summary>
    	/// <param name="workloadDayTemplate">The workload day template.</param>
    	/// <param name="workloadDays">The workload days.</param>
    	/// <param name="includeStatistics">Include statistics.</param>
    	/// <returns></returns>
    	/// <remarks>
    	/// Created by: peterwe, zoet
    	/// Created date: 2008-02-20
    	/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void calculateTemplateDay(IWorkloadDayTemplate workloadDayTemplate, IEnumerable<IWorkloadDayBase> workloadDays, bool includeStatistics = false)
    	{
    		TimeZoneInfo raptorTimeZoneInfo = _workload.Skill.TimeZone;
            DateTime startDateTime = raptorTimeZoneInfo.SafeConvertTimeToUtc(SkillDayTemplate.BaseDate);
            
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
    		if (workloadDayTemplate == null) throw new ArgumentNullException("workloadDayTemplate");
    		if (workloadDays == null) throw new ArgumentNullException("workloadDays");
			
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
                var currentUtcDate = TimeZoneHelper.ConvertToUtc(workloadDay.CurrentDate,
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
        /// Creates the task periods from periodized.
        /// </summary>
        /// <param name="periodizedData">The periodized data.</param>
        /// <returns>An empty template task period for each period in periodized data collection</returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-22
        /// </remarks>
        public static IList<ITemplateTaskPeriod> CreateTaskPeriodsFromPeriodized(IEnumerable periodizedData)
        {
            IEnumerable<IPeriodized> typedPeriodizedData = periodizedData.OfType<IPeriodized>();

            IList<ITemplateTaskPeriod> taskPeriods = new List<ITemplateTaskPeriod>();
            foreach (IPeriodized periodized in typedPeriodizedData)
            {
                taskPeriods.Add(new TemplateTaskPeriod(new Task(), periodized.Period));
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
                    skillStaffPeriod.StatisticTask =
                        GetStatisticsWithPercentage(taskPeriod.StatisticTask,
                                                    skillDay.SkillDayCalculator.GetPercentageForInterval(
                                                        skillDay.Skill, dateTimePeriod));

                IActiveAgentCount activeAgentCount =
                    activeAgentCountCollection.FirstOrDefault(a => dateTimePeriod.Contains(a.Interval));
                skillStaffPeriod.ActiveAgentCount = new ActiveAgentCount();
                if (activeAgentCount != null) skillStaffPeriod.ActiveAgentCount.ActiveAgents = activeAgentCount.ActiveAgents;
            }
        }

        private static IStatisticTask GetStatisticsWithPercentage(IStatisticTask statisticTask,Percent percentage)
        {
            if (percentage.Value==1) return statisticTask;

            IStatisticTask statisticTaskToReturn = new StatisticTask();
            statisticTaskToReturn.Interval = statisticTask.Interval;
            statisticTaskToReturn.StatAbandonedShortTasks = statisticTask.StatAbandonedShortTasks * percentage.Value;
            statisticTaskToReturn.StatAbandonedTasks = statisticTask.StatAbandonedTasks * percentage.Value;
            statisticTaskToReturn.StatAbandonedTasksWithinSL = statisticTask.StatAbandonedTasksWithinSL * percentage.Value;
            statisticTaskToReturn.StatAnsweredTasks = statisticTask.StatAnsweredTasks * percentage.Value;
            statisticTaskToReturn.StatAnsweredTasksWithinSL = statisticTask.StatAnsweredTasksWithinSL * percentage.Value;
            statisticTaskToReturn.StatAverageAfterTaskTimeSeconds = statisticTask.StatAverageAfterTaskTimeSeconds;
            statisticTaskToReturn.StatAverageHandleTimeSeconds = statisticTask.StatAverageHandleTimeSeconds;
            statisticTaskToReturn.StatAverageQueueTimeSeconds = statisticTask.StatAverageQueueTimeSeconds;
            statisticTaskToReturn.StatAverageTaskTimeSeconds = statisticTask.StatAverageTaskTimeSeconds;
            statisticTaskToReturn.StatAverageTimeLongestInQueueAbandonedSeconds = statisticTask.StatAverageTimeLongestInQueueAbandonedSeconds;
            statisticTaskToReturn.StatAverageTimeLongestInQueueAnsweredSeconds = statisticTask.StatAverageTimeLongestInQueueAnsweredSeconds;
            statisticTaskToReturn.StatAverageTimeToAbandonSeconds = statisticTask.StatAverageTimeToAbandonSeconds;
            statisticTaskToReturn.StatCalculatedTasks = statisticTask.StatCalculatedTasks * percentage.Value;
            statisticTaskToReturn.StatOfferedTasks = statisticTask.StatOfferedTasks * percentage.Value;
            statisticTaskToReturn.StatOverflowInTasks = statisticTask.StatOverflowInTasks * percentage.Value;
            statisticTaskToReturn.StatOverflowOutTasks = statisticTask.StatOverflowOutTasks * percentage.Value;

            return statisticTaskToReturn;
        }
    }
}
