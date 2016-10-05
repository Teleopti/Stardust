using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingResourceCalculation : IResourceOptimization
	{
		private readonly ResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly ShovelResources _shovelResources;

		public CascadingResourceCalculation(ResourceOptimizationHelper resourceOptimizationHelper,
																ShovelResources shovelResources)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_shovelResources = shovelResources;
		}

		public void ResourceCalculate(DateOnlyPeriod dateOnlyPeriod, IResourceCalculationData resourceCalculationData)
		{
			doForPeriod(dateOnlyPeriod, resourceCalculationData);
		}

		public void ResourceCalculate(DateOnly localDate, IResourceCalculationData resourceCalculationData)
		{
			doForPeriod(new DateOnlyPeriod(localDate, localDate), resourceCalculationData);
		}

		private void doForPeriod(DateOnlyPeriod period, IResourceCalculationData resourceCalculationData)
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