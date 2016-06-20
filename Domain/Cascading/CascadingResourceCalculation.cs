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
		
		public void ForAll()
		{
			doForPeriod(_stateHolder().RequestedPeriod.DateOnlyPeriod);
		}

		public void ResourceCalculateDate(DateOnly localDate, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			//TODO: need to consider params above
			doForPeriod(new DateOnlyPeriod(localDate, localDate));
		}

		private void doForPeriod(DateOnlyPeriod period)
		{
			var resultState = _stateHolder().SchedulingResultState;
			foreach (var date in period.DayCollection())
			{
				//TODO: ska det vara true, true (?) här - fixa och lägg på test senare. behövs nog i nästkommande PBIer...
				_resourceOptimizationHelper.ResourceCalculateDate(date, false, false);
			}
			if (!ResourceCalculationContext.PrimarySkillMode()) 
			{
				_shovelResources.Execute(resultState.SkillStaffPeriodHolder, resultState.Schedules, resultState.Skills, period);
			}
		}
	}
}