using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IWorkShiftCalculatorsManager
	{
		IList<IWorkShiftCalculationResultHolder> RunCalculators(IPerson person,
			IList<IShiftProjectionCache> shiftProjectionCaches,
			IWorkShiftCalculatorSkillStaffPeriodData dataHolders,
			IDictionary<ISkill, ISkillStaffPeriodDictionary> nonBlendSkillPeriods,
			ISchedulingOptions schedulingOptions);
	}

	public class WorkShiftCalculatorsManager : IWorkShiftCalculatorsManager
	{
		private readonly IWorkShiftCalculator _workShiftCalculator;
		private readonly INonBlendWorkShiftCalculator _nonBlendWorkShiftCalculator;

		public WorkShiftCalculatorsManager(IWorkShiftCalculator workShiftCalculator, INonBlendWorkShiftCalculator nonBlendWorkShiftCalculator)
		{
			_workShiftCalculator = workShiftCalculator;
			_nonBlendWorkShiftCalculator = nonBlendWorkShiftCalculator;
		}

		public IList<IWorkShiftCalculationResultHolder> RunCalculators(IPerson person,
				IList<IShiftProjectionCache> shiftProjectionCaches,
				IWorkShiftCalculatorSkillStaffPeriodData dataHolders,
				IDictionary<ISkill, ISkillStaffPeriodDictionary> nonBlendSkillPeriods,
				ISchedulingOptions schedulingOptions)
		{
			IList<IWorkShiftCalculationResultHolder> allValues =
				new List<IWorkShiftCalculationResultHolder>(shiftProjectionCaches.Count);
			foreach (IShiftProjectionCache shiftProjection in shiftProjectionCaches)
			{
				double? nonBlendValue = null;
				double thisValue = _workShiftCalculator.CalculateShiftValue(shiftProjection.MainShiftProjection,
																			dataHolders,
																			schedulingOptions.WorkShiftLengthHintOption,
																			schedulingOptions.UseMinimumPersons,
																			schedulingOptions.UseMaximumPersons);

				if (nonBlendSkillPeriods.Count > 0)
					nonBlendValue = _nonBlendWorkShiftCalculator.CalculateShiftValue(person,
																					 shiftProjection.MainShiftProjection,
																					 nonBlendSkillPeriods,
																					 schedulingOptions.WorkShiftLengthHintOption,
																					 schedulingOptions.UseMinimumPersons,
																					 schedulingOptions.UseMaximumPersons);
				if (nonBlendValue.HasValue)
				{
					if (thisValue.Equals(double.MinValue))
						thisValue = nonBlendValue.Value;
					else
					{
						thisValue += nonBlendValue.Value;
					}
				}

				if (thisValue > double.MinValue)
				{
					var workShiftFinderResultHolder = new WorkShiftCalculationResult { ShiftProjection = shiftProjection, Value = thisValue };
					allValues.Add(workShiftFinderResultHolder);
				}
			}
			return allValues;
		}
	}
}