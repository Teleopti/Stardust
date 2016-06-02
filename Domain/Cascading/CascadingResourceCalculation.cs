using System;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingResourceCalculation : IResourceOptimizationHelper
	{
		private readonly ResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly Func<ISchedulerStateHolder> _stateHolder;
		private readonly CascadeResources _cascadeResources;
		private readonly ITimeZoneGuard _timeZoneGuard;

		public CascadingResourceCalculation(ResourceOptimizationHelper resourceOptimizationHelper,
																Func<ISchedulerStateHolder> stateHolder,
																CascadeResources cascadeResources,
																ITimeZoneGuard timeZoneGuard)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_stateHolder = stateHolder;
			_cascadeResources = cascadeResources;
			_timeZoneGuard = timeZoneGuard;
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
				//TODO: context för en viss period här?
				using (new ResourceCalculationContextFactory(_stateHolder, () => new CascadingPersonSkillProvider(), _timeZoneGuard).Create())
				{
					foreach (var date in period.DayCollection())
					{
						//TODO: ska det vara true, true (?) här - fixa och lägg på test senare. behövs nog i nästkommande PBIer...
						_resourceOptimizationHelper.ResourceCalculateDate(date, false, false);
					}
				}
				using (new ResourceCalculationContextFactory(_stateHolder, () => new PersonSkillProvider(), _timeZoneGuard).Create())
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