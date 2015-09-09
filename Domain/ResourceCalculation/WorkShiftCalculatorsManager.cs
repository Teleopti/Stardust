using System.Collections.Generic;
using System.Linq;
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
			var shouldCalculateNonBlendValue = nonBlendSkillPeriods.Count > 0;

			var allValues = shiftProjectionCaches.AsParallel().Select(shiftProjection =>
			{
				var v = new
				{
					shiftProjection,
					shiftValue = _workShiftCalculator.CalculateShiftValue(shiftProjection.WorkShiftCalculatableLayers,
						dataHolders,
						schedulingOptions.WorkShiftLengthHintOption,
						schedulingOptions.UseMinimumPersons,
						schedulingOptions.UseMaximumPersons),
					nonBlendValue = shouldCalculateNonBlendValue
						? _nonBlendWorkShiftCalculator.CalculateShiftValue(person,
							shiftProjection.MainShiftProjection,
							nonBlendSkillPeriods,
							schedulingOptions.WorkShiftLengthHintOption,
							schedulingOptions.UseMinimumPersons,
							schedulingOptions.UseMaximumPersons)
						: null
				};
				double value = v.shiftValue;
				if (v.nonBlendValue.HasValue)
				{
					if (v.shiftValue.Equals(double.MinValue))
					{
						value = v.nonBlendValue.Value;
					}
					else
					{
						value += v.nonBlendValue.Value;
					}
				}
				return (IWorkShiftCalculationResultHolder)new WorkShiftCalculationResult { ShiftProjection = v.shiftProjection, Value = value };
			}).Where(w => w.Value > double.MinValue).ToList();

			return allValues;
		}
	}
}