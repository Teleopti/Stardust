using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Outbound
{
	public class OutboundProductionPlanFactory
	{
		private readonly IncomingTaskFactory _incomingTaskFactory;

		public OutboundProductionPlanFactory(IncomingTaskFactory incomingTaskFactory)
		{
			_incomingTaskFactory = incomingTaskFactory;
		}

		public IncomingTask CreateAndMakeInitialPlan(DateOnlyPeriod campaignPeriod, int campaignTasks,
			TimeSpan averageTimeForHandlingTasks, IDictionary<DayOfWeek, TimePeriod> workingHours)
		{
			var incomingTask = _incomingTaskFactory.Create(campaignPeriod, campaignTasks, averageTimeForHandlingTasks);

			foreach (var date in incomingTask.SpanningPeriod.DayCollection())
			{
				incomingTask.Close(date);
				foreach (var workingHour in workingHours)
				{
					if (workingHour.Key == date.DayOfWeek) incomingTask.Open(date);
				}
			}

			incomingTask.RecalculateDistribution();
			return incomingTask;
		}
	}
}