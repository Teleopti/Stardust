using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ResourceCalculationThatThrows : IResourceCalculation
	{
		public void ResourceCalculate(DateOnly localDate, IResourceCalculationData resourceCalculationData)
		{
			throw new NotSupportedException("Res calc is not supported");
		}

		public void ResourceCalculate(DateOnlyPeriod dateOnlyPeriod, IResourceCalculationData resourceCalculationData)
		{
			throw new NotSupportedException("Res calc is not supported");
		}
	}
}