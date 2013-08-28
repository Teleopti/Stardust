using System;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources
{
	public interface IScheduledResourcesReadModel
	{
	}

	public interface IScheduledResourcesReadModelUpdaterActions
	{
		void AddResource(ResourceLayer resourceLayer, SkillCombination combination);
		void RemoveResource(ResourceLayer resourceLayer, SkillCombination combination);
	}

	public interface IScheduledResourcesReadModelUpdater
	{
		void Update(string dataSource, Guid businessUnitId, Action<IScheduledResourcesReadModelUpdaterActions> action);
	}
}