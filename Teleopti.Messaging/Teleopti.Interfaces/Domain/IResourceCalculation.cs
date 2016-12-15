namespace Teleopti.Interfaces.Domain
{
    public interface IResourceCalculation
    {
		void ResourceCalculate(DateOnly localDate, IResourceCalculationData resourceCalculationData);
		void ResourceCalculate(DateOnlyPeriod dateOnlyPeriod, IResourceCalculationData resourceCalculationData);
    }
}
