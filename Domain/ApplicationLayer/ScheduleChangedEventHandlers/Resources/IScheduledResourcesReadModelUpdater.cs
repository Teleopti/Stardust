using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources
{
	public interface IScheduledResourcesReadModelUpdater
	{
		void AddResource(ResourceLayer resourceLayer, SkillCombination combination);
		void RemoveResource(ResourceLayer resourceLayer, SkillCombination combination);
	}
}