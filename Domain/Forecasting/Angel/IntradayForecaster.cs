using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class IntradayForecaster 
	{
		private const int smoothing = 5;
		private readonly Stopwatch _stopwatch = new Stopwatch();
		
		public void Apply(IWorkload workload, IEnumerable<IWorkloadDayBase> queueStatistics, IEnumerable<IWorkloadDayBase> futureWorkloadDays)
		{
			_stopwatch.Start();
			var sortedTemplateTaskPeriodsDic = calculatePattern(workload, queueStatistics);

			foreach (var futureWorkloadDay in futureWorkloadDays)
			{
				futureWorkloadDay.DistributeTasks(sortedTemplateTaskPeriodsDic[futureWorkloadDay.CurrentDate.DayOfWeek]);

				if(!(futureWorkloadDay is WorkloadDay workloadDay)) continue;
				workloadDay.TemplateReference.TemplateName = TemplateReference.WebTemplateKey;
			}
			_stopwatch.Stop();
		}

		private IDictionary<DayOfWeek, IEnumerable<ITemplateTaskPeriod>> calculatePattern(IWorkload workload, IEnumerable<IWorkloadDayBase> queueStatistics)
		{
			var workloadDays = queueStatistics.ToLookup(k => k.CurrentDate.DayOfWeek);

			var sortedTemplateTaskPeriodsDic = new Dictionary<DayOfWeek, IEnumerable<ITemplateTaskPeriod>>();
			foreach (DayOfWeek day in Enum.GetValues(typeof (DayOfWeek)))
			{
				sortedTemplateTaskPeriodsDic.Add(day, calculateTemplateTaskPeriods(workloadDays[day], workload));
			}
			return sortedTemplateTaskPeriodsDic;
		}

		private IEnumerable<ITemplateTaskPeriod> createExtendedTaskPeriodList(IEnumerable<IWorkloadDayBase> statisticDays, DateTime startTimeTemplate)
		{
			var taskPeriods = new List<ITemplateTaskPeriod>();
			foreach (var statisticDay in statisticDays)
			{
				var currentUtcDate = TimeZoneHelper.ConvertToUtc(statisticDay.CurrentDate.Date, statisticDay.Workload.Skill.TimeZone);
				var initialDiff = startTimeTemplate.Subtract(currentUtcDate);
				foreach (var statisticTaskPeriod in statisticDay.SortedTaskPeriodList)
				{
					var dateTimePeriod = statisticTaskPeriod.Period.MovePeriod(initialDiff);

					var templateTaskPeriod = new TemplateTaskPeriod(statisticTaskPeriod.Task, dateTimePeriod)
					{
						StatisticTask =
						{
							StatCalculatedTasks = statisticTaskPeriod.StatisticTask.StatCalculatedTasks,
							StatAverageTaskTimeSeconds = statisticTaskPeriod.StatisticTask.StatAverageTaskTimeSeconds,
							StatAverageAfterTaskTimeSeconds = statisticTaskPeriod.StatisticTask.StatAverageAfterTaskTimeSeconds,
							StatAbandonedTasks = statisticTaskPeriod.StatisticTask.StatAbandonedTasks,
							StatAnsweredTasks = statisticTaskPeriod.StatisticTask.StatAnsweredTasks
						}
					};
					taskPeriods.Add(templateTaskPeriod);
				}
			}
			return taskPeriods;
		}

		private void Smoothing(IList<ITemplateTaskPeriod> list)
		{
			IDictionary<DateTimePeriod, double> numbers = list.ToDictionary(templateTaskPeriod => templateTaskPeriod.Period,
				templateTaskPeriod => templateTaskPeriod.Tasks);
			numbers = new StatisticalSmoothing(numbers).CalculateRunningAverage(smoothing);
			foreach (var templateTaskPeriod in list)
			{
				templateTaskPeriod.SetTasks(numbers[templateTaskPeriod.Period]);
			}
		}

		private IEnumerable<ITemplateTaskPeriod> calculateTemplateTaskPeriods(IEnumerable<IWorkloadDayBase> workloadDaysToCalculateTemplate, IWorkload workload)
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
								Task = g.Average(t => t.StatisticTask.StatCalculatedTasks)
							};

			//Create the template from the extended list
			var defaultResoulution = TimeSpan.FromMinutes(workload.Skill.DefaultResolution);
			var oneTickTaskTime = new TimeSpan(1);
			var templateTaskPeriods = new List<ITemplateTaskPeriod>();

			foreach (var time in times)
			{
				var tasks = time.Task;
				var task = new Task(tasks, oneTickTaskTime, oneTickTaskTime);

				var dateTime = startDateTime.Add(time.Period);
				var dateTimePeriod = new DateTimePeriod(dateTime, dateTime.Add(defaultResoulution));
				templateTaskPeriods.Add(new TemplateTaskPeriod(task, dateTimePeriod));
			}

			var list = templateTaskPeriods.OrderBy(tp => tp.Period.StartDateTime).ToList();
			Smoothing(list);

			return list;
		}
	}
}