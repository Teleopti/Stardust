using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources
{
	public class ScheduledResourcesReadModelUpdater : IScheduledResourcesReadModelUpdater
	{
		private readonly IScheduledResourcesReadModelPersister _persister;

		public ScheduledResourcesReadModelUpdater(IScheduledResourcesReadModelPersister persister)
		{
			_persister = persister;
		}

		public void AddResource(ResourceLayer resourceLayer, SkillCombination combination)
		{
			var resourceId = _persister.AddResources(resourceLayer.PayloadId, resourceLayer.RequiresSeat,
			                                         combination.Key, resourceLayer.Period,
			                                         resourceLayer.Resource, 1);
			foreach (var skillEfficiency in combination.SkillEfficiencies)
			{
				_persister.AddSkillEfficiency(resourceId, skillEfficiency.Key, skillEfficiency.Value);
			}
		}

		public void RemoveResource(ResourceLayer resourceLayer, SkillCombination combination)
		{
			var resourceId = _persister.RemoveResources(resourceLayer.PayloadId, combination.Key,
			                                            resourceLayer.Period, resourceLayer.Resource, 1);
			if (!resourceId.HasValue) return;

			foreach (var skillEfficiency in combination.SkillEfficiencies)
			{
				_persister.RemoveSkillEfficiency(resourceId.Value, skillEfficiency.Key, skillEfficiency.Value);
			}
		}

	}
}