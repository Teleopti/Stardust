using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public static class StatisticTaskExtensions
	{
		public static IStatisticTask MergeStatisticTasks(this IEnumerable<IStatisticTask> statisticTasksToMerge)
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
				statTaskAverageHandleTimeSeconds += statisticTask.StatAnsweredTasks * statisticTask.StatAverageHandleTimeSeconds;

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
				newStatisticTask.StatAverageAfterTaskTimeSeconds = statTaskAverageAfterTaskTimeSeconds/statAnsweredTasks;
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

		public static IStatisticTask GetStatisticsWithPercentage(this IStatisticTask statisticTask, Percent percentage)
		{
			if (percentage.Value == 1) return statisticTask;

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

		public static IList<ITemplateTaskPeriod> CreateTaskPeriodsFromPeriodized(this IEnumerable<IPeriodized> periodizedData)
		{
			IList<ITemplateTaskPeriod> taskPeriods = new List<ITemplateTaskPeriod>();
			foreach (IPeriodized periodized in periodizedData)
			{
				taskPeriods.Add(new TemplateTaskPeriod(new Task(), periodized.Period));
			}
			return taskPeriods;
		}

		public static void ApplyStatisticsTo(this IStatisticTask statisticTask, ITemplateTaskPeriod taskPeriod)
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
			taskPeriod.StatisticTask.StatAbandonedTasksWithinSL = statisticTask.StatAbandonedTasksWithinSL;
			taskPeriod.StatisticTask.StatAnsweredTasksWithinSL = statisticTask.StatAnsweredTasksWithinSL;
			taskPeriod.StatisticTask.StatOverflowInTasks = statisticTask.StatOverflowInTasks;
			taskPeriod.StatisticTask.StatOverflowOutTasks = statisticTask.StatOverflowOutTasks;

			taskPeriod.StatisticTask.StatAverageHandleTimeSeconds = statisticTask.StatAverageHandleTimeSeconds;
			taskPeriod.StatisticTask.StatAverageQueueTimeSeconds = statisticTask.StatAverageQueueTimeSeconds;
			taskPeriod.StatisticTask.StatAverageTimeLongestInQueueAbandonedSeconds = statisticTask.StatAverageTimeLongestInQueueAbandonedSeconds;
			taskPeriod.StatisticTask.StatAverageTimeLongestInQueueAnsweredSeconds = statisticTask.StatAverageTimeLongestInQueueAnsweredSeconds;
			taskPeriod.StatisticTask.StatAverageTimeToAbandonSeconds = statisticTask.StatAverageTimeToAbandonSeconds;
		}
	}
}