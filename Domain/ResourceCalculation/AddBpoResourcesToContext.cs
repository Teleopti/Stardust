using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class AddBpoResourcesToContext
	{
		public void Execute(ResourceCalculationDataContainer resourceCalculationDataContainer, IEnumerable<BpoResource> bpoResources)
		{
			foreach (var bpoResource in bpoResources)
			{
				var tempAgent = bpoResource.CreateBpoAgent();
				foreach (var resourceLayer in bpoResource.CreateResourceLayers(TimeSpan.FromMinutes(resourceCalculationDataContainer.MinSkillResolution)))
				{
					resourceCalculationDataContainer.AddResources(tempAgent, DateOnly.Today, resourceLayer);
				}
			}
		}
	}
}