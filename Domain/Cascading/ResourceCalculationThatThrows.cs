using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ResourceCalculationThatThrows : IResourceCalculation
	{
		public void ResourceCalculate(DateOnlyPeriod period, IResourceCalculationData resourceCalculationData, Func< IDisposable> getResourceCalculationContext = null)
		{
			throw new NotSupportedException("Res calc is not supported");
		}
	}
}