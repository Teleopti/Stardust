using System;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingResourceCalculation : IResourceOptimizationHelper
	{
		private readonly ResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly Func<ISchedulerStateHolder> _stateHolder;
		private readonly ShovelResources _shovelResources;

		public CascadingResourceCalculation(ResourceOptimizationHelper resourceOptimizationHelper,
																Func<ISchedulerStateHolder> stateHolder,
																ShovelResources shovelResources)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_stateHolder = stateHolder;
			_shovelResources = shovelResources;
		}

		public void ResourceCalculate(DateOnlyPeriod dateOnlyPeriod, ResourceOptimizationData resourceOptimizationData)
		{
			doForPeriod(dateOnlyPeriod, resourceOptimizationData);
		}

		public void ResourceCalculate(DateOnly localDate, ResourceOptimizationData resourceOptimizationData)
		{
			doForPeriod(new DateOnlyPeriod(localDate, localDate), resourceOptimizationData);
		}

		private void doForPeriod(DateOnlyPeriod period, ResourceOptimizationData resourceOptimizationData)
		{
			var resultState = _stateHolder().SchedulingResultState;
			foreach (var date in period.DayCollection())
			{
				_resourceOptimizationHelper.ResourceCalculate(date, resourceOptimizationData);
			}
			if (!ResourceCalculationContext.PrimarySkillMode()) 
			{
				_shovelResources.Execute(resultState.SkillStaffPeriodHolder, resourceOptimizationData.Schedules, resultState.Skills, period);
			}
		}
	}
}