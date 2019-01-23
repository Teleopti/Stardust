using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class WorkShiftCalculatorsManager
	{
		private readonly IWorkShiftCalculator _workShiftCalculator;
		private readonly INonBlendWorkShiftCalculator _nonBlendWorkShiftCalculator;

		public WorkShiftCalculatorsManager(IWorkShiftCalculator workShiftCalculator, INonBlendWorkShiftCalculator nonBlendWorkShiftCalculator)
		{
			_workShiftCalculator = workShiftCalculator;
			_nonBlendWorkShiftCalculator = nonBlendWorkShiftCalculator;
		}

		public IEnumerable<IWorkShiftCalculationResultHolder> RunCalculators(IPerson person,
				IEnumerable<ShiftProjectionCache> shiftProjectionCaches,
				IWorkShiftCalculatorSkillStaffPeriodData dataHolders,
				IDictionary<ISkill, ISkillStaffPeriodDictionary> nonBlendSkillPeriods,
				SchedulingOptions schedulingOptions)
		{
			var shouldCalculateNonBlendValue = nonBlendSkillPeriods.Count > 0;
			var allValues = new List<IWorkShiftCalculationResultHolder>();

			if (shouldCalculateNonBlendValue)
			{
				foreach (var shiftProjectionCache in shiftProjectionCaches)
				{
					//var shiftProjection = ((IWorkShiftCalculatableProjection)shiftProjectionCache).WorkShiftCalculatableLayers;
					var resultFromOneCache = _nonBlendWorkShiftCalculator.CalculateShiftValue(person,
						shiftProjectionCache.MainShiftProjection(),
						nonBlendSkillPeriods,
						schedulingOptions.WorkShiftLengthHintOption,
						schedulingOptions.UseMinimumStaffing,
						schedulingOptions.UseMaximumStaffing);

					if (resultFromOneCache.Value > Double.MinValue)
					{
						allValues.Add(new WorkShiftCalculationResult
						{
							ShiftProjection = resultFromOneCache.ShiftProjection,
							Value = resultFromOneCache.Value,
							LengthInMinutes = resultFromOneCache.LengthInMinutes
						});
					}
				}
			}
			else
			{
				foreach (var shiftProjectionCache in shiftProjectionCaches)
				{
					var shiftProjection = ((IWorkShiftCalculatableProjection)shiftProjectionCache).WorkShiftCalculatableLayers;
					var resultFromOneCache = _workShiftCalculator.CalculateShiftValue(shiftProjection,
						dataHolders,
						schedulingOptions.WorkShiftLengthHintOption,
						schedulingOptions.UseMinimumStaffing,
						schedulingOptions.UseMaximumStaffing);

					if(resultFromOneCache.Value > Double.MinValue)
					{
						allValues.Add(new WorkShiftCalculationResult
						{
							ShiftProjection = shiftProjectionCache,
							Value = resultFromOneCache.Value,
							LengthInMinutes = resultFromOneCache.LengthInMinutes
						});
					}
				}
			}

			return allValues;
		}
	}
}