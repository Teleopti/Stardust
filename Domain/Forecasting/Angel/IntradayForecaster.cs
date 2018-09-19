using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class IntradayForecaster 
	{
		private const int smoothing = 5;
		private readonly ILoadStatistics _loadStatistics;
		public ILog Logger = LogManager.GetLogger(typeof(IntradayForecaster));
		private readonly Stopwatch _stopwatch = new Stopwatch();

		public IntradayForecaster(ILoadStatistics loadStatistics)
		{
			_loadStatistics = loadStatistics;
		}

		public void Apply(IWorkload workload, DateOnlyPeriod templatePeriod, IEnumerable<IWorkloadDayBase> futureWorkloadDays)
		{
			_stopwatch.Start();
			var sortedTemplateTaskPeriodsDic = calculatePattern(workload, templatePeriod);

			foreach (var futureWorkloadDay in futureWorkloadDays)
			{
				futureWorkloadDay.DistributeTasks(sortedTemplateTaskPeriodsDic[futureWorkloadDay.CurrentDate.DayOfWeek]);

				if(!(futureWorkloadDay is WorkloadDay workloadDay)) continue;
				workloadDay.TemplateReference.TemplateName = TemplateReference.WebTemplateKey;
			}
			_stopwatch.Stop();
		}

		private IDictionary<DayOfWeek, IEnumerable<ITemplateTaskPeriod>> calculatePattern(IWorkload workload, DateOnlyPeriod templatePeriod)
		{
			Logger.Debug($"Before _loadStatistics.LoadWorkloadDay: {_stopwatch.ElapsedMilliseconds} ms");
			var workloadDays = _loadStatistics.LoadWorkloadDay(workload, templatePeriod).ToLookup(k => k.CurrentDate.DayOfWeek);
			Logger.Debug($"After _loadStatistics.LoadWorkloadDay: {_stopwatch.ElapsedMilliseconds} ms");

			var sortedTemplateTaskPeriodsDic = new Dictionary<DayOfWeek, IEnumerable<ITemplateTaskPeriod>>();
			foreach (DayOfWeek day in Enum.GetValues(typeof (DayOfWeek)))
			{
				sortedTemplateTaskPeriodsDic.Add(day, calculateTemplateTaskPeriods(workloadDays[day], workload));
			}
			return sortedTemplateTaskPeriodsDic;
		}

		private IEnumerable<ITemplateTaskPeriod> createExtendedTaskPeriodList(IEnumerable<IWorkloadDayBase> workloadDays, DateTime startTimeTemplate)
		{
			Logger.Debug($"Before createExtendedTaskPeriodList: {_stopwatch.ElapsedMilliseconds} ms");
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
			Logger.Debug($"After createExtendedTaskPeriodList: {_stopwatch.ElapsedMilliseconds} ms");
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
			Logger.Debug($"Before calculateTemplateTaskPeriods: {_stopwatch.ElapsedMilliseconds} ms");
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

			Logger.Debug($"After calculateTemplateTaskPeriods: {_stopwatch.ElapsedMilliseconds} ms");
			return list;
		}
	}
}