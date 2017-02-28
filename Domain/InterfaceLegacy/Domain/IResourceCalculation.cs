using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IResourceCalculation
	{
		void ResourceCalculate(DateOnlyPeriod period, IResourceCalculationData resourceCalculationData, Func<IDisposable> getResourceCalculationContext = null);
	}
}
