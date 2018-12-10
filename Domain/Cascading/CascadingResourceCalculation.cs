using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

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

		//TODO: always in context here. Put needed data (from resourceCalculationData) on context instead and grab the data from there...)
		public virtual void ResourceCalculate(DateOnlyPeriod period, ResourceCalculationData resourceCalculationData, Func< IDisposable> getResourceCalculationContext = null )
		{
			if (!ResourceCalculationContext.InContext)
				throw new NoCurrentResourceCalculationContextException();
			
			if (resourceCalculationData.SkipResourceCalculation|| !resourceCalculationData.Skills.Any())
				return;

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