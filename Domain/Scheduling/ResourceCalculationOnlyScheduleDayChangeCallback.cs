using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class ResourceCalculationOnlyScheduleDayChangeCallback : IScheduleDayChangeCallback
    {

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
    }
}