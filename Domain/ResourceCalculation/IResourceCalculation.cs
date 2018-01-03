using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IResourceCalculation
	{
		void ResourceCalculate(DateOnlyPeriod period, ResourceCalculationData resourceCalculationData, Func<IDisposable> getResourceCalculationContext = null);
	}
}
