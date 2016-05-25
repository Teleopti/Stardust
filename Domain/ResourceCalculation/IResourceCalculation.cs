using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IResourceCalculation
	{
		void All();
		void Period(DateOnlyPeriod period);
	}
}