namespace Teleopti.Interfaces.Domain
{
    public interface IResourceOptimization
    {
		void ResourceCalculate(DateOnly localDate, IResourceCalculationData resourceCalculationData);
		void ResourceCalculate(DateOnlyPeriod dateOnlyPeriod, IResourceCalculationData resourceCalculationData);
    }
}
