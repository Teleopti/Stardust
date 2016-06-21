using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingResourceCalculation : IResourceOptimizationHelper
	{
		private readonly ResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly ShovelResources _shovelResources;

		public CascadingResourceCalculation(ResourceOptimizationHelper resourceOptimizationHelper,
																ShovelResources shovelResources)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_shovelResources = shovelResources;
		}

		public void ResourceCalculate(DateOnlyPeriod dateOnlyPeriod, ResourceCalculationData resourceCalculationData)
		{
			doForPeriod(dateOnlyPeriod, resourceCalculationData);
		}

		public void ResourceCalculate(DateOnly localDate, ResourceCalculationData resourceCalculationData)
		{
			doForPeriod(new DateOnlyPeriod(localDate, localDate), resourceCalculationData);
		}

		private void doForPeriod(DateOnlyPeriod period, ResourceCalculationData resourceCalculationData)
		{
			foreach (var date in period.DayCollection())
			{
				_resourceOptimizationHelper.ResourceCalculate(date, resourceCalculationData);
			}
			if (!ResourceCalculationContext.PrimarySkillMode()) 
			{
				_shovelResources.Execute(resourceCalculationData.SkillStaffPeriodHolder, resourceCalculationData.Schedules, resourceCalculationData.Skills, period);
			}
		}
	}
}