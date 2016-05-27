using System;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingResourceCalculation : IResourceOptimizationHelper
	{
		private readonly IResourceOptimizationHelperOld _resourceOptimizationHelper;
		private readonly Func<ISchedulerStateHolder> _stateHolder;
		private readonly CascadeResources _cascadeResources;

		public CascadingResourceCalculation(IResourceOptimizationHelperOld resourceOptimizationHelper,
																Func<ISchedulerStateHolder> stateHolder,
																CascadeResources cascadeResources)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_stateHolder = stateHolder;
			_cascadeResources = cascadeResources;
		}

		public void ForDay(DateOnly date)
		{
			doForPeriod(new DateOnlyPeriod(date, date));
		}

		public void ForAll()
		{
			doForPeriod(_stateHolder().RequestedPeriod.DateOnlyPeriod);
		}

		private void doForPeriod(DateOnlyPeriod period)
		{
			using (ResourceCalculationCurrent.PreserveContext())
			{
				using (new ResourceCalculationContextFactory(_stateHolder, () => new CascadingPersonSkillProvider()).Create())
				{
					foreach (var date in period.DayCollection())
					{
						//TODO: ska det vara true, true (?) här - fixa och lägg på test senare. behövs nog i nästkommande PBIer...
						_resourceOptimizationHelper.ResourceCalculateDate(date, false, false);
					}
				}
				using (new ResourceCalculationContextFactory(_stateHolder, () => new PersonSkillProvider()).Create())
				{
					foreach (var date in period.DayCollection())
					{
						_cascadeResources.Execute(date);
					}
				}
			}
		}

		public void ResourceCalculateDate(DateOnly localDate, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			//TODO: need to consider params above
			ForDay(localDate);
		}
	}
}