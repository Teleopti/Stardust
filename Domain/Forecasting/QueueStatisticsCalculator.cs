using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class QueueStatisticsCalculator : IQueueStatisticsCalculator
    {
        private readonly QueueAdjustment _queueAdjustment;

        public QueueStatisticsCalculator(QueueAdjustment queueAdjustment)
        {
            _queueAdjustment = queueAdjustment;
        }

        public void Calculate(IStatisticTask statisticTask)
        {
            if (statisticTask == null) return;

            //Warning! If you change this code in any way, your changes must be applied to the sql function mart.CalculateQueueStatistics as well!
            var abandonedAfterServiceLevel = statisticTask.StatAbandonedTasks - statisticTask.StatAbandonedShortTasks -
                                             statisticTask.StatAbandonedTasksWithinSL;

	        var calculatedTasks = statisticTask.StatOfferedTasks*_queueAdjustment.OfferedTasks.Value +
	                              statisticTask.StatAbandonedTasks*_queueAdjustment.Abandoned.Value +
	                              statisticTask.StatOverflowInTasks*_queueAdjustment.OverflowIn.Value +
	                              statisticTask.StatAbandonedShortTasks*_queueAdjustment.AbandonedShort.Value +
	                              statisticTask.StatAbandonedTasksWithinSL*_queueAdjustment.AbandonedWithinServiceLevel.Value +
	                              abandonedAfterServiceLevel*_queueAdjustment.AbandonedAfterServiceLevel.Value +
	                              statisticTask.StatOverflowOutTasks*_queueAdjustment.OverflowOut.Value;

	        statisticTask.StatCalculatedTasks = Math.Max(calculatedTasks,0);
        }
    }
}