using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IResourceCalculationContextFactory
	{
		ResourceCalculationContext Create();
		ResourceCalculationContext Create(DateOnlyPeriod period);
	}
}