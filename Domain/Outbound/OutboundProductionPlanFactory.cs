using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Interfaces.Domain;

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
			TimeSpan averageTimeForHandlingTasks, IList<CampaignWorkingPeriod> campaignWorkingPeriods)
		{
			var incomingTask = _incomingTaskFactory.Create(campaignPeriod, campaignTasks, averageTimeForHandlingTasks);

			foreach (var date in incomingTask.SpanningPeriod.DayCollection())
			{
				incomingTask.Close(date);
				foreach (var campaignWorkingPeriod in campaignWorkingPeriods)
				{
					foreach (var assignment in campaignWorkingPeriod.CampaignWorkingPeriodAssignments)
					{
						if (assignment.WeekdayIndex == date.DayOfWeek)
							incomingTask.Open(date);
					}
				}
			}

			incomingTask.RecalculateDistribution();
			return incomingTask;
		}
	}
}