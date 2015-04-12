using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Collection;
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
			
			var tasks = new List<Task<IList<IWorkShiftCalculationResultHolder>>>();
			foreach (var shiftProjectionCachesBatch in shiftProjectionCaches.Batch(1000))
			{
				IEnumerable<IShiftProjectionCache> batch = shiftProjectionCachesBatch;
				tasks.Add(Task<IList<IWorkShiftCalculationResultHolder>>.Factory.StartNew(() => 
					calculateBatch(batch, person, dataHolders, nonBlendSkillPeriods, schedulingOptions)
					));
			}
// ReSharper disable CoVariantArrayConversion
			Task.WaitAll(tasks.ToArray());
// ReSharper restore CoVariantArrayConversion

			var allValues = new List<IWorkShiftCalculationResultHolder>();
			foreach (var task in tasks)
			{
				allValues.AddRange(task.Result);
			}
			return allValues.ToList();
		}

		private IList<IWorkShiftCalculationResultHolder> calculateBatch(IEnumerable<IShiftProjectionCache> shiftProjectionCachesBatch, IPerson person, IWorkShiftCalculatorSkillStaffPeriodData dataHolders,
			IDictionary<ISkill, ISkillStaffPeriodDictionary> nonBlendSkillPeriods, ISchedulingOptions schedulingOptions)
		{
			var batchValues = new List<IWorkShiftCalculationResultHolder>();
			foreach (IShiftProjectionCache shiftProjection in shiftProjectionCachesBatch)
			{
				calculate(person, dataHolders, nonBlendSkillPeriods, schedulingOptions, shiftProjection, batchValues);
			}

			return batchValues;
		}

		private void calculate(IPerson person, IWorkShiftCalculatorSkillStaffPeriodData dataHolders,
			IDictionary<ISkill, ISkillStaffPeriodDictionary> nonBlendSkillPeriods, ISchedulingOptions schedulingOptions, IShiftProjectionCache shiftProjection,
			IList<IWorkShiftCalculationResultHolder> allValues)
		{
			double? nonBlendValue = null;
			double thisValue = _workShiftCalculator.CalculateShiftValue(shiftProjection.WorkShiftCalculatableLayers,
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
				var workShiftFinderResultHolder = new WorkShiftCalculationResult
				{
					ShiftProjection = shiftProjection,
					Value = thisValue
				};
				allValues.Add(workShiftFinderResultHolder);
			}
		}
	}
}