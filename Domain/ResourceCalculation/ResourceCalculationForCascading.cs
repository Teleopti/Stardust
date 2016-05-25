using System;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceCalculationForCascading : IResourceCalculation
	{
		private readonly CascadingResourceCalculation _cascadingResourceCalculation;

		public ResourceCalculationForCascading(CascadingResourceCalculation cascadingResourceCalculation)
		{
			_cascadingResourceCalculation = cascadingResourceCalculation;
		}

		public void All()
		{
			_cascadingResourceCalculation.ForAll();
		}

		public void Period(DateOnlyPeriod period)
		{
			throw new NotImplementedException();
		}
	}
}