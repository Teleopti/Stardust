using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
	public interface IWorkShiftSelector
	{
		IShiftProjectionCache Select(IList<IShiftProjectionCache> shiftList,
		                             IDictionary<ISkill, IDictionary<TimeSpan, ISkillIntervalData>> skillIntervalDatas,
		                             WorkShiftLengthHintOption lengthFactor, bool useMinimumPersons, bool useMaximumPersons);
	}

	public class WorkShiftSelector : IWorkShiftSelector
	{
		private readonly IWorkShiftValueCalculator _workShiftValueCalculator;
		private readonly IEqualWorkShiftValueDecider _equalWorkShiftValueDecider;

		public WorkShiftSelector(IWorkShiftValueCalculator workShiftValueCalculator, IEqualWorkShiftValueDecider equalWorkShiftValueDecider)
		{
			_workShiftValueCalculator = workShiftValueCalculator;
			_equalWorkShiftValueDecider = equalWorkShiftValueDecider;
		}

		public IShiftProjectionCache Select(IList<IShiftProjectionCache> shiftList, IDictionary<ISkill, IDictionary<TimeSpan, ISkillIntervalData>> skillIntervalDatas, WorkShiftLengthHintOption lengthFactor, bool useMinimumPersons, bool useMaximumPersons)
		{
			double? bestShiftValue = null;
			IShiftProjectionCache bestShift = null;
			foreach (var shiftProjectionCache in shiftList)
			{
				double? valueForShift = this.valueForShift(skillIntervalDatas, shiftProjectionCache, lengthFactor, useMinimumPersons,
				                                           useMaximumPersons);
				if (valueForShift.HasValue)
				{
					if (!bestShiftValue.HasValue)
					{
						bestShiftValue = valueForShift.Value;
						bestShift = shiftProjectionCache;
					}
					else
					{
						if (valueForShift.Value == bestShiftValue)
						{
							bestShiftValue = valueForShift.Value;
							bestShift = _equalWorkShiftValueDecider.Decide(bestShift, shiftProjectionCache);
						}

						if(valueForShift.Value > bestShiftValue)
						{
							bestShiftValue = valueForShift.Value;
							bestShift = shiftProjectionCache;
						}
							
					}

				}
			}

			return bestShift;
		}

		private double? valueForSkill(KeyValuePair<ISkill, IDictionary<TimeSpan, ISkillIntervalData>> keyValuePair, IShiftProjectionCache shiftProjectionCache, WorkShiftLengthHintOption lengthFactor, bool useMinimumPersons, bool useMaximumPersons)
		{
			ISkill skill = keyValuePair.Key;
			IActivity skillActivity = skill.Activity;
			var priority = skill.PriorityValue;
			var overstaffingFactor = skill.OverstaffingFactor;
			double? value = _workShiftValueCalculator.CalculateShiftValue(shiftProjectionCache.MainShiftProjection,
																		  skillActivity, keyValuePair.Value, lengthFactor,
																		  useMinimumPersons, useMaximumPersons,
																		  overstaffingFactor.Value, priority);
			

			return value;
		}

		private double? valueForShift(IDictionary<ISkill, IDictionary<TimeSpan, ISkillIntervalData>> skillIntervalDatas, IShiftProjectionCache shiftProjectionCache, WorkShiftLengthHintOption lengthFactor, bool useMinimumPersons, bool useMaximumPersons)
		{
			double? totalForAllSkillValue = null;
			foreach (var keyValuePair in skillIntervalDatas)
			{
				double? skillValue = valueForSkill(keyValuePair, shiftProjectionCache, lengthFactor, useMinimumPersons,
												   useMaximumPersons);
				if (skillValue.HasValue)
				{
					if (totalForAllSkillValue.HasValue)
					{
						totalForAllSkillValue = totalForAllSkillValue + skillValue.Value;
					}
					else
					{
						totalForAllSkillValue = skillValue.Value;
					}

				}
			}

			return totalForAllSkillValue;
		}
	}
}