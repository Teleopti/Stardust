using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class QueueStatisticsProvider : IQueueStatisticsProvider
    {
        private readonly IQueueStatisticsCalculator _calculator;
        private readonly StatisticsPerInterval _statisticsPerInterval = new StatisticsPerInterval();

        public QueueStatisticsProvider(IEnumerable<IStatisticTask> statisticsList, IQueueStatisticsCalculator calculator)
        {
            _calculator = calculator;
            foreach (var statisticTask in statisticsList)
            {
                var intervalWithUtcSpecified = DateTime.SpecifyKind(statisticTask.Interval, DateTimeKind.Utc);
                var key = new DateTimePeriod(intervalWithUtcSpecified,
                                             intervalWithUtcSpecified.AddMinutes(1));

                IStatisticTask foundStatisticTask;
                if (_statisticsPerInterval.TryGetValue(key, out foundStatisticTask))
                {
                    _statisticsPerInterval.Remove(key);
					_statisticsPerInterval.Add(key, new List<IStatisticTask> { foundStatisticTask, statisticTask }.MergeStatisticTasks());
                }
                else
                {
                    _statisticsPerInterval.Add(key, statisticTask);
                }
            }
        }

        public IStatisticTask GetStatisticsForPeriod(DateTimePeriod period)
        {
            var task = InternalGetStatisticsForPeriod(period);
            _calculator.Calculate(task);
            return task;
        }

        private IStatisticTask InternalGetStatisticsForPeriod(DateTimePeriod period)
        {
            IStatisticTask statisticTask;
            if (_statisticsPerInterval.TryGetValue(period, out statisticTask))
            {
                return statisticTask;
            }
            return statisticTask;
        }

        private class StatisticsPerInterval
        {
            private readonly SortedList<DateTime, IStatisticTask> _innerDictionary = new SortedList<DateTime, IStatisticTask>();

            public void Add(DateTimePeriod period, IStatisticTask statisticTask)
            {
                _innerDictionary.Add(period.StartDateTime,statisticTask);
            }

            public bool TryGetValue(DateTimePeriod key,out IStatisticTask value)
            {
                var foundStatistics = new List<IStatisticTask>();
                var periodList = key.Intervals(TimeSpan.FromMinutes(1));

                foreach (var period in periodList)
                {
                    IStatisticTask task;
                    if (_innerDictionary.TryGetValue(period.StartDateTime,out task))
                        foundStatistics.Add(task);
                }
                if (foundStatistics.Count == 0)
                {
                    value = new StatisticTask {Interval = key.StartDateTime};
                    return false;
                }

                //Merge!
				value = foundStatistics.MergeStatisticTasks();
                return true;
            }

            public void Remove(DateTimePeriod key)
            {
                _innerDictionary.Remove(key.StartDateTime);
            }
        }
    }
}