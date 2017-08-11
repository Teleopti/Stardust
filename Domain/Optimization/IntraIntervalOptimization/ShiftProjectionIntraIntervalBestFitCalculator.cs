using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface IShiftProjectionIntraIntervalBestFitCalculator
	{
		IWorkShiftCalculationResultHolder GetShiftProjectionCachesSortedByBestIntraIntervalFit(IEnumerable<IWorkShiftCalculationResultHolder> workShiftCalculationResults, IList<ISkillStaffPeriod> issues, ISkill skill, double limit);
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


		public IWorkShiftCalculationResultHolder GetShiftProjectionCachesSortedByBestIntraIntervalFit(IEnumerable<IWorkShiftCalculationResultHolder> workShiftCalculationResults, IList<ISkillStaffPeriod> issues, ISkill skill, double limit)
		{
			var bestValue = Double.MaxValue;
			ShiftProjectionCache bestShiftProjectionCache = null;

			foreach (var workShiftCalculationResultHolder in workShiftCalculationResults)
			{
				var shiftProjectionCache = workShiftCalculationResultHolder.ShiftProjection;
				double? totalValue = null;

				foreach (var skillStaffPeriod in issues)
				{
					var affectedPeriods = _skillStaffPeriodIntraIntervalPeriodFinder.Find(skillStaffPeriod.Period, shiftProjectionCache, skill);

					if(affectedPeriods.Count == 0) 
						continue;

				
					var samples = _skillActivityCounter.Count(affectedPeriods, skillStaffPeriod.Period);

					if (skillStaffPeriod.IntraIntervalSamples.Count == 0)
					{
						continue;
					}

					var value = _shiftProjectionCacheIntraIntervalValueCalculator.Calculate(skillStaffPeriod.IntraIntervalSamples, samples);

					if (!totalValue.HasValue) totalValue = 0;

					totalValue = totalValue.Value + (limit - value);
				}

				if (!totalValue.HasValue || !(totalValue.Value < bestValue)) continue;
				bestShiftProjectionCache = shiftProjectionCache;
				bestValue = totalValue.Value;
			}

			if (bestShiftProjectionCache == null) return null;

			return new WorkShiftCalculationResult { ShiftProjection = bestShiftProjectionCache, Value = bestValue };	
		}	
	}
}
