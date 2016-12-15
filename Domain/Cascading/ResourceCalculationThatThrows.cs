using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ResourceCalculationThatThrows : IResourceCalculation
	{
		public void ResourceCalculate(DateOnlyPeriod period, IResourceCalculationData resourceCalculationData)
		{
			throw new NotSupportedException("Res calc is not supported");
		}
	}
}