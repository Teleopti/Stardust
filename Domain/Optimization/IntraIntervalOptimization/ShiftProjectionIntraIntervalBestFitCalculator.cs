using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface IShiftProjectionIntraIntervalBestFitCalculator
	{
		IList<IWorkShiftCalculationResultHolder> GetShiftProjectionCachesSortedByBestIntraIntervalFit(IList<IWorkShiftCalculationResultHolder> workShiftCalculationResults, IList<ISkillStaffPeriod> issues, ISkill skill);
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

		public IList<IWorkShiftCalculationResultHolder> GetShiftProjectionCachesSortedByBestIntraIntervalFit(IList<IWorkShiftCalculationResultHolder> workShiftCalculationResults, IList<ISkillStaffPeriod> issues, ISkill skill)
		{
			IComparer<IWorkShiftCalculationResultHolder> comparer = new WorkShiftCalculationResultComparer();
			var sortedList = new List<IWorkShiftCalculationResultHolder>();

			foreach (var workShiftCalculationResultHolder in workShiftCalculationResults)
			{
				var shiftProjectionCache = workShiftCalculationResultHolder.ShiftProjection;
				var totalValue = 0d;

				foreach (var skillStaffPeriod in issues)
				{
					var affectedPeriods = _skillStaffPeriodIntraIntervalPeriodFinder.Find(skillStaffPeriod.Period, shiftProjectionCache, skill);
					var samples = _skillActivityCounter.Count(affectedPeriods, skillStaffPeriod.Period);
					var value = _shiftProjectionCacheIntraIntervalValueCalculator.Calculate(skillStaffPeriod.IntraIntervalSamples, samples);
					
					totalValue += value;
				}

				sortedList.Add(new WorkShiftCalculationResult { ShiftProjection = shiftProjectionCache, Value = totalValue });

			}

			sortedList.Sort(comparer);
			return sortedList;
		}	
	}
}
