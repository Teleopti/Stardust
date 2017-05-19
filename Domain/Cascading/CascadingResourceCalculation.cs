using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

		public virtual void ResourceCalculate(DateOnlyPeriod period, ResourceCalculationData resourceCalculationData, Func< IDisposable> getResourceCalculationContext = null )
		{
			foreach (var date in period.DayCollection())
			{
				_resourceOptimizationHelper.ResourceCalculate(date, resourceCalculationData);
			}
			if (!ResourceCalculationContext.PrimarySkillMode())
			{
				_shovelResources.Execute(resourceCalculationData.SkillResourceCalculationPeriodDictionary, resourceCalculationData.Schedules, resourceCalculationData.Skills, period, resourceCalculationData.ShovelingCallback, getResourceCalculationContext);
			}
		}
	}
}