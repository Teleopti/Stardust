using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IResourceCalculation
	{
		void ResourceCalculate(DateOnlyPeriod period, IResourceCalculationData resourceCalculationData, Func<IDisposable> getResourceCalculationContext = null);
	}
}
