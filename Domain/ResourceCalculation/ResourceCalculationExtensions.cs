using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public static class ResourceCalculationExtensions
	{
		public static void ResourceCalculate(this IResourceCalculation resourceCalculation, DateOnly date, ResourceCalculationData resourceCalculationData)
		{
			resourceCalculation.ResourceCalculate(date.ToDateOnlyPeriod(), resourceCalculationData);
		}
	}
}