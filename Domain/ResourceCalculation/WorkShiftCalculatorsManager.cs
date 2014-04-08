using System;
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
			//IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> dataHolders,
			IWorkShiftCalculatorSkillStaffPeriods dataHolders,
			IDictionary<ISkill, ISkillStaffPeriodDictionary> nonBlendSkillPeriods,
			ISchedulingOptions schedulingOptions);
	}

	public class SkillStaffPeriodDataWrapper : IWorkShiftCalculatorSkillStaffPeriods
	{
		private readonly IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> _data;

		public SkillStaffPeriodDataWrapper(IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> data)
		{
			_data = data;
		}

		public bool Empty()
		{
			return _data.Count == 0;
		}

		public IWorkShiftCalculatorStaffPeriods ForActivity(IWorkShiftCalculatableActivity activity)
		{
			var key = activity as IActivity;
			return _data.ContainsKey(key) ? new StaffPeriodDataWrapper(_data[key]) : null;
		}

		public IEnumerable<IWorkShiftCalculatorStaffPeriods> All()
		{
			return from i in _data.Values select new StaffPeriodDataWrapper(i);
		}
	}

	public class StaffPeriodDataWrapper : IWorkShiftCalculatorStaffPeriods
	{
		private readonly IDictionary<DateTime, ISkillStaffPeriodDataHolder> _data;

		public StaffPeriodDataWrapper(IDictionary<DateTime, ISkillStaffPeriodDataHolder> data)
		{
			_data = data;
		}

		public IWorkShiftCalculatableSkillStaffPeriod ForTime(DateTime dateTime)
		{
			return _data.ContainsKey(dateTime) ? _data[dateTime] : null;
		}

		public IWorkShiftCalculatableSkillStaffPeriod First()
		{
			return _data.Values.FirstOrDefault();
		}

		public IEnumerable<IWorkShiftCalculatableSkillStaffPeriod> All()
		{
			return _data.Values;
		}
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
				IWorkShiftCalculatorSkillStaffPeriods dataHolders,
				//IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> dataHolders,
				IDictionary<ISkill, ISkillStaffPeriodDictionary> nonBlendSkillPeriods,
				ISchedulingOptions schedulingOptions)
		{
			IList<IWorkShiftCalculationResultHolder> allValues =
				new List<IWorkShiftCalculationResultHolder>(shiftProjectionCaches.Count);
			foreach (IShiftProjectionCache shiftProjection in shiftProjectionCaches)
			{
				double? nonBlendValue = null;
				double thisValue = _workShiftCalculator.CalculateShiftValue(shiftProjection.MainShiftProjection,
																			dataHolders, schedulingOptions.WorkShiftLengthHintOption,
																			schedulingOptions.UseMinimumPersons, schedulingOptions.UseMaximumPersons,
																			TimeHelper.FitToDefaultResolution);

				if (nonBlendSkillPeriods.Count > 0)
					nonBlendValue = _nonBlendWorkShiftCalculator.CalculateShiftValue(person,
																					 shiftProjection.
																						 MainShiftProjection,
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