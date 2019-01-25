using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios
{
	//just a wrapper class for simpler testing. Not to be moved to domain!
	public class ResourceCalculateWithNewContext
	{
		private readonly IResourceCalculation _resourceCalculation;
		private readonly CascadingResourceCalculationContextFactory _cascadingResourceCalculationContextFactory;

		public ResourceCalculateWithNewContext(IResourceCalculation resourceCalculation, CascadingResourceCalculationContextFactory cascadingResourceCalculationContextFactory)
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
			using (_cascadingResourceCalculationContextFactory.Create(resourceCalculationData.Schedules, resourceCalculationData.Skills, Enumerable.Empty<ExternalStaff>(), false, period))
			{
				_resourceCalculation.ResourceCalculate(period, resourceCalculationData);
			}
		}
	}
}