using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
    public class SchedulerStateScheduleDayChangedCallback : IScheduleDayChangeCallback
    {
        private readonly ScheduleChangesAffectedDates _resourceCalculateDaysDecider;
        private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;

        public SchedulerStateScheduleDayChangedCallback(ScheduleChangesAffectedDates resourceCalculateDaysDecider, Func<ISchedulerStateHolder> schedulerStateHolder)
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

        public void ScheduleDayChanged(IScheduleDay partBefore)
        {
            if (partBefore != null)
            {
	            var partAfter = partBefore.ReFetch();
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