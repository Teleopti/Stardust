using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public static class ResourceCalculationExtensions
	{
		public static void ResourceCalculate(this IResourceCalculation resourceCalculation, DateOnly date, IResourceCalculationData resourceCalculationData)
		{
			resourceCalculation.ResourceCalculate(date.ToDateOnlyPeriod(), resourceCalculationData);
		}
	}
}