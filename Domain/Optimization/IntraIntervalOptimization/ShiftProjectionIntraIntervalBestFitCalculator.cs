using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface IShiftProjectionIntraIntervalBestFitCalculator
	{
		IList<IWorkShiftCalculationResultHolder> GetShiftProjectionCachesSortedByBestIntraIntervalFit(IList<IWorkShiftCalculationResultHolder> workShiftCalculationResults, IList<ISkillStaffPeriod> issues, ISkill skill, double limit);
	}

	public class ShiftProjectionIntraIntervalBestFitCalculator : IShiftProjectionIntraIntervalBestFitCalculator
	{
		private readonly ISkillStaffPeriodIntraIntervalPeriodFinder _skillStaffPeriodIntraIntervalPeriodFinder;
		private readonly ISkillActivityCounter _skillActivityCounter;
		private readonly IShiftProjectionCacheIntraIntervalValueCalculator _shiftProjectionCacheIntraIntervalValueCalculator;

		public ShiftProjectionIntraIntervalBestFitCalculator(ISkillStaffPeriodIntraIntervalPeriodFinder skillStaffPeriodIntraIntervalPeriodFinder, ISkillActivityCounter skillActivityCounter, IShiftProjectionCacheIntraIntervalValueCalculator shiftProjectionCacheIntraIntervalValueCalculator)
		{
			_skillStaffPeriodIntraIntervalPeriodFinder = skillStaffPeriodIntraIntervalPeriodFinder;
			_skillActivityCounter = skillActivityCounter;
			_shiftProjectionCacheIntraIntervalValueCalculator = shiftProjectionCacheIntraIntervalValueCalculator;
		}

		public IList<IWorkShiftCalculationResultHolder> GetShiftProjectionCachesSortedByBestIntraIntervalFit(IList<IWorkShiftCalculationResultHolder> workShiftCalculationResults, IList<ISkillStaffPeriod> issues, ISkill skill, double limit)
		{
			IComparer<IWorkShiftCalculationResultHolder> comparer = new WorkShiftCalculationResultComparer();
			var sortedList = new List<IWorkShiftCalculationResultHolder>();

			foreach (var workShiftCalculationResultHolder in workShiftCalculationResults)
			{
				var shiftProjectionCache = workShiftCalculationResultHolder.ShiftProjection;
				var totalValue = 0d;

				if (shiftProjectionCache.TheMainShift.LayerCollection[1].Period.StartDateTime.Minute == 40)
				{
					
				}

				if (shiftProjectionCache.TheMainShift.LayerCollection[1].Period.StartDateTime.Minute == 0)
				{

				}

				foreach (var skillStaffPeriod in issues)
				{
					var affectedPeriods = _skillStaffPeriodIntraIntervalPeriodFinder.Find(skillStaffPeriod.Period, shiftProjectionCache, skill);

					if(affectedPeriods.Count == 0) 
						continue;
				
					var samples = _skillActivityCounter.Count(affectedPeriods, skillStaffPeriod.Period);

					if (skillStaffPeriod.IntraIntervalSamples.Count == 0)
					{
						//totalValue += skillStaffPeriod.IntraIntervalValue;
						continue;
					}

					var value = _shiftProjectionCacheIntraIntervalValueCalculator.Calculate(skillStaffPeriod.IntraIntervalSamples, samples);

					if (value > limit)
					{
						//totalValue += 1.0 - value;
						continue;
						
					}
					
					
					totalValue += limit - value;
				}

				sortedList.Add(new WorkShiftCalculationResult { ShiftProjection = shiftProjectionCache, Value = totalValue });

			}

			sortedList.Sort(comparer);
			return sortedList;
		}	
	}
}
