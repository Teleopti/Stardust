using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ScheduleDayChangeCallback : IScheduleDayChangeCallback
	{
		private readonly IResourceCalculateDaysDecider _resourceCalculateDaysDecider;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;

		public ScheduleDayChangeCallback(IResourceCalculateDaysDecider resourceCalculateDaysDecider, Func<ISchedulerStateHolder> schedulerStateHolder)
		{
			_resourceCalculateDaysDecider = resourceCalculateDaysDecider;
			_schedulerStateHolder = schedulerStateHolder;
		}

		public void ScheduleDayBeforeChanging()
		{
			if (ResourceCalculationContext.InContext)
			{
				ResourceCalculationContext.Fetch();
			}
		}

		public void ScheduleDayChanged(IScheduleDay partBefore, IScheduleDay partAfter)
		{
			if (partBefore != null && partAfter != null)
			{
				applyChangesToResourceContainer(partBefore, partAfter);
				markDaysToRecalculate(partBefore, partAfter);
			}
		}

		private static void applyChangesToResourceContainer(IScheduleDay partBefore, IScheduleDay partAfter)
		{
			if (ResourceCalculationContext.InContext)
			{
				var container = ResourceCalculationContext.Fetch();
				container.RemoveScheduleDayFromContainer(partBefore, container.MinSkillResolution);
				container.AddScheduleDayToContainer(partAfter, container.MinSkillResolution);
			}
		}

		private void markDaysToRecalculate(IScheduleDay partBefore, IScheduleDay partAfter)
		{
			_resourceCalculateDaysDecider.DecideDates(partAfter, partBefore).ForEach(
					_schedulerStateHolder().MarkDateToBeRecalculated);
		}
	}
}