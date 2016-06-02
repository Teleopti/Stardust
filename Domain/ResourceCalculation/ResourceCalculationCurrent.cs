using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public static class ResourceCalculationCurrent
	{
		public static IDisposable PreserveContext()
		{
			Lazy<IResourceCalculationDataContainerWithSingleOperation> existingContext = null;
			if (ResourceCalculationContext.InContext)
			{
				var currentContext = ResourceCalculationContext.Fetch();
				existingContext = new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => currentContext);
			}
			return new GenericDisposable(() =>
			{
				if (existingContext != null)
				{
					new ResourceCalculationContext(existingContext);
				}
			});
		}
	}
}