using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IResourceCalculation
	{
		void ResourceCalculate(DateOnlyPeriod period, ResourceCalculationData resourceCalculationData, Func<IDisposable> getResourceCalculationContext = null);
	}
}
