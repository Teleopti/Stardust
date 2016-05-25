using Teleopti.Ccc.Domain.Cascading;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class FullResourceCalculationWithCascading : IFullResourceCalculation
	{
		private readonly CascadingResourceCalculation _cascadingResourceCalculation;

		public FullResourceCalculationWithCascading(CascadingResourceCalculation cascadingResourceCalculation)
		{
			_cascadingResourceCalculation = cascadingResourceCalculation;
		}

		public void Execute()
		{
			_cascadingResourceCalculation.ForAll();
		}
	}
}