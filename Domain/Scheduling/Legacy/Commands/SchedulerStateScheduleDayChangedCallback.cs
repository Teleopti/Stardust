using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
    public class SchedulerStateScheduleDayChangedCallback : IScheduleDayChangeCallback
    {
        private readonly IResourceCalculateDaysDecider _resourceCalculateDaysDecider;
        private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;

        private IScheduleDay _dayBefore;

        public SchedulerStateScheduleDayChangedCallback(IResourceCalculateDaysDecider resourceCalculateDaysDecider, Func<ISchedulerStateHolder> schedulerStateHolder)
        {
            _resourceCalculateDaysDecider = resourceCalculateDaysDecider;
            _schedulerStateHolder = schedulerStateHolder;
        }

        public void ScheduleDayChanging(IScheduleDay partBefore)
		{
	        if (ResourceCalculationContext.InContext)
	        {
		        ResourceCalculationContext.Fetch();
	        }
	        _dayBefore = partBefore;
        }

        public void ScheduleDayChanged(IScheduleDay partAfter)
        {
            if (_dayBefore!=null && partAfter!=null)
            {
                applyChangesToResourceContainer(partAfter);
                markDaysToRecalculate(partAfter);
            }
        }

        private void applyChangesToResourceContainer(IScheduleDay partAfter)
        {
            if (ResourceCalculationContext.InContext)
            {
                var container = ResourceCalculationContext.Fetch();
                container.RemoveScheduleDayFromContainer(_dayBefore, container.MinSkillResolution);
                container.AddScheduleDayToContainer(partAfter, container.MinSkillResolution);
            }
        }

        private void markDaysToRecalculate(IScheduleDay partAfter)
        {
            _resourceCalculateDaysDecider.DecideDates(partAfter, _dayBefore).ForEach(
                _schedulerStateHolder().MarkDateToBeRecalculated);
        }
    }
}