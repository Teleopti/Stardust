using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingResourceCalculation
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;

		public CascadingResourceCalculation(IResourceOptimizationHelper resourceOptimizationHelper)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
		}

		public void ForDay(DateOnly date)
		{
			_resourceOptimizationHelper.ResourceCalculateDate(date, false, false); //check this later
		}
	}
}