using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class AddBpoResourcesToContext
	{
		public void Execute(ResourceCalculationDataContainer resourceCalculationDataContainer, IEnumerable<ExternalStaff> bpoResources)
		{
			foreach (var bpoResource in bpoResources)
			{
				var tempAgent = bpoResource.CreateExternalAgent();
				foreach (var resourceLayer in bpoResource.CreateResourceLayers(TimeSpan.FromMinutes(resourceCalculationDataContainer.MinSkillResolution)))
				{
					resourceCalculationDataContainer.AddResources(tempAgent, DateOnly.Today, resourceLayer);
				}
			}
		}
	}
}