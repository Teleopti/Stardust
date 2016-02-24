using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class ResourceCalculationOnlyScheduleDayChangeCallback : IScheduleDayChangeCallback
    {
        private IScheduleDay _dayBefore;

        public void ScheduleDayChanging(IScheduleDay partBefore)
        {
            _dayBefore = partBefore;
        }

        public void ScheduleDayChanged(IScheduleDay partAfter)
        {
            if (_dayBefore != null && partAfter != null)
            {
                applyChangesToResourceContainer(partAfter);
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
    }
}