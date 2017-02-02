using System;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingResourceCalculation : IResourceCalculation
	{
		private readonly ResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly ShovelResources _shovelResources;

		public CascadingResourceCalculation(ResourceOptimizationHelper resourceOptimizationHelper, ShovelResources shovelResources)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_shovelResources = shovelResources;
		}

		public void ResourceCalculate(DateOnlyPeriod period, IResourceCalculationData resourceCalculationData, Func< IDisposable> getResourceCalculationContext = null )
		{
			foreach (var date in period.DayCollection())
			{
				_resourceOptimizationHelper.ResourceCalculate(date, resourceCalculationData);
			}
			if (!ResourceCalculationContext.PrimarySkillMode())
			{
				_shovelResources.Execute(resourceCalculationData.SkillStaffPeriodHolder, resourceCalculationData.Schedules, resourceCalculationData.Skills, period,getResourceCalculationContext);
			}
		}
		
	}
}