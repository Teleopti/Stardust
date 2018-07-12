using System;
using System.Collections.Generic;
using System.Linq;
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

		public IList<IWorkShiftCalculationResultHolder> RunCalculators(IPerson person,
				IList<ShiftProjectionCache> shiftProjectionCaches,
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
						shiftProjectionCache.MainShiftProjection,
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

			

			//var allValues = shiftProjectionCaches.Select(shiftProjection =>
			//{
			//	var v = new
			//	{
			//		shiftProjection,
			//		shiftValueAndLength = _workShiftCalculator.CalculateShiftValue(((IWorkShiftCalculatableProjection)shiftProjection).WorkShiftCalculatableLayers,
			//			dataHolders,
			//			schedulingOptions.WorkShiftLengthHintOption,
			//			schedulingOptions.UseMinimumStaffing,
			//			schedulingOptions.UseMaximumStaffing),
			//		nonBlendValue = shouldCalculateNonBlendValue
			//			? _nonBlendWorkShiftCalculator.CalculateShiftValue(person,
			//				shiftProjection.MainShiftProjection,
			//				nonBlendSkillPeriods,
			//				schedulingOptions.WorkShiftLengthHintOption,
			//				schedulingOptions.UseMinimumStaffing,
			//				schedulingOptions.UseMaximumStaffing)
			//			: null
			//	};
			//	double value = v.shiftValueAndLength.Value;
			//	if (v.nonBlendValue.HasValue)
			//	{
			//		if (v.shiftValueAndLength.Value.Equals(double.MinValue))
			//		{
			//			value = v.nonBlendValue.Value;
			//		}
			//		else
			//		{
			//			value += v.nonBlendValue.Value;
			//		}
			//	}
			//	return (IWorkShiftCalculationResultHolder)new WorkShiftCalculationResult { ShiftProjection = v.shiftProjection, Value = value, LengthInMinutes = v.shiftValueAndLength.LengthInMinutes};
			//}).Where(w => w.Value > double.MinValue).ToList();

			return allValues;
		}
	}
}