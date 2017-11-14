using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceCalculateWithNewContext_OnlyToBeUsedFromTest
	{
		private readonly IResourceCalculation _resourceCalculation;
		private readonly CascadingResourceCalculationContextFactory _cascadingResourceCalculationContextFactory;

		public ResourceCalculateWithNewContext_OnlyToBeUsedFromTest(IResourceCalculation resourceCalculation, CascadingResourceCalculationContextFactory cascadingResourceCalculationContextFactory)
		{
			_resourceCalculation = resourceCalculation;
			_cascadingResourceCalculationContextFactory = cascadingResourceCalculationContextFactory;
		}

		public void ResourceCalculate(DateOnly date, ResourceCalculationData resourceCalculationData)
		{
			ResourceCalculate(date.ToDateOnlyPeriod(), resourceCalculationData);
		}
		
		public void ResourceCalculate(DateOnlyPeriod period, ResourceCalculationData resourceCalculationData)
		{
			using (_cascadingResourceCalculationContextFactory.Create(resourceCalculationData.Schedules, resourceCalculationData.Skills, false, period))
			{
				_resourceCalculation.ResourceCalculate(period, resourceCalculationData);
			}
		}
	}
}