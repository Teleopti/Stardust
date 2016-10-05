namespace Teleopti.Interfaces.Domain
{
    public interface IResourceOptimizationHelper
    {
		void ResourceCalculate(DateOnly localDate, IResourceCalculationData resourceCalculationData);
		void ResourceCalculate(DateOnlyPeriod dateOnlyPeriod, IResourceCalculationData resourceCalculationData);
    }
}
