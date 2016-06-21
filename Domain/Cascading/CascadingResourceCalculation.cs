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
			var resultState = _stateHolder().SchedulingResultState;
			foreach (var date in period.DayCollection())
			{
				_resourceOptimizationHelper.ResourceCalculate(date, resourceCalculationData);
			}
			if (!ResourceCalculationContext.PrimarySkillMode()) 
			{
				_shovelResources.Execute(resultState.SkillStaffPeriodHolder, resourceCalculationData.Schedules, resultState.Skills, period);
			}
		}
	}
}