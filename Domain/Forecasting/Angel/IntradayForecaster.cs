using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class IntradayForecaster : IIntradayForecaster
	{
		private const int smoothing = 5;
		private readonly ILoadStatistics _loadStatistics;

		public IntradayForecaster(ILoadStatistics loadStatistics)
		{
			_loadStatistics = loadStatistics;
		}

		public void Apply(IWorkload workload, DateOnlyPeriod templatePeriod, IEnumerable<IWorkloadDayBase> futureWorkloadDays)
		{
			var workloadDays = _loadStatistics.LoadWorkloadDay(workload, templatePeriod).ToArray();

			var sortedTemplateTaskPeriods = calculateTemplateTaskPeriods(workloadDays, workload);

			foreach (var futureWorkloadDay in futureWorkloadDays)
			{
				futureWorkloadDay.DistributeTasks(sortedTemplateTaskPeriods);
			}

			// TODO  will add smoothing
		}

		private static IEnumerable<ITemplateTaskPeriod> createExtendedTaskPeriodList(IEnumerable<IWorkloadDayBase> workloadDays, DateTime startTimeTemplate)
		{
			//All taskperiods on skillday into one list and create dummy date
			var taskPeriods = new List<ITemplateTaskPeriod>();
			foreach (var workloadDay in workloadDays)
			{
				var currentUtcDate = TimeZoneHelper.ConvertToUtc(workloadDay.CurrentDate.Date, workloadDay.Workload.Skill.TimeZone);
				var initialDiff = startTimeTemplate.Subtract(currentUtcDate);
				foreach (var taskPeriod in workloadDay.SortedTaskPeriodList)
				{
					var dateTimePeriod = taskPeriod.Period.MovePeriod(initialDiff);

					var templateTaskPeriod = new TemplateTaskPeriod(taskPeriod.Task, dateTimePeriod)
					{
						StatisticTask =
						{
							StatCalculatedTasks = taskPeriod.StatisticTask.StatCalculatedTasks,
							StatAverageTaskTimeSeconds = taskPeriod.StatisticTask.StatAverageTaskTimeSeconds,
							StatAverageAfterTaskTimeSeconds = taskPeriod.StatisticTask.StatAverageAfterTaskTimeSeconds,
							StatAbandonedTasks = taskPeriod.StatisticTask.StatAbandonedTasks,
							StatAnsweredTasks = taskPeriod.StatisticTask.StatAnsweredTasks
						}
					};
					taskPeriods.Add(templateTaskPeriod);
				}
			}
			return taskPeriods;
		}

		private IList<ITemplateTaskPeriod> calculateTemplateTaskPeriods(IEnumerable<IWorkloadDayBase> workloadDaysToCalculateTemplate, IWorkload workload)
		{
			var raptorTimeZoneInfo = workload.Skill.TimeZone;
			var startDateTime = raptorTimeZoneInfo.SafeConvertTimeToUtc(SkillDayTemplate.BaseDate.Date);

			//Create the list with all periods
			var taskPeriods = createExtendedTaskPeriodList(workloadDaysToCalculateTemplate, startDateTime);

			//Group taskperiods on timeperiod.
			var times = from t in taskPeriods
						group t by t.Period.StartDateTime.Subtract(startDateTime)
							into g
							select new
							{
								Period = g.Key,
								Task = g.Average(t => t.StatisticTask.StatCalculatedTasks),
								SumTasks = g.Sum(t => t.StatisticTask.StatCalculatedTasks)
							};

			//Create the template from the extended list
			IList<ITemplateTaskPeriod> templateTaskPeriods = new List<ITemplateTaskPeriod>();
			foreach (var time in times)
			{
				var tasks = time.Task;
				var task = new Task(tasks, new TimeSpan(1), new TimeSpan(1));

				var defaultResoulution = workload.Skill.DefaultResolution;
				var dateTimePeriod = new DateTimePeriod(startDateTime.Add(time.Period), startDateTime.Add(time.Period).Add(TimeSpan.FromMinutes(defaultResoulution)));
				templateTaskPeriods.Add(new TemplateTaskPeriod(task, dateTimePeriod));
			}

			var list = templateTaskPeriods.OrderBy(tp => tp.Period.StartDateTime).ToList();
			return new ReadOnlyCollection<ITemplateTaskPeriod>(list);
		}
	}
}