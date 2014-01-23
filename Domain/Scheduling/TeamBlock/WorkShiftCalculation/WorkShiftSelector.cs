using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IWorkShiftSelector
	{
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IShiftProjectionCache SelectShiftProjectionCache(IList<IShiftProjectionCache> shiftList,
		                             IDictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> skillIntervalDataDictionary,
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

        public IShiftProjectionCache SelectShiftProjectionCache(IList<IShiftProjectionCache> shiftList, IDictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> skillIntervalDataDictionary, WorkShiftLengthHintOption lengthFactor, bool useMinimumPersons, bool useMaximumPersons)
		{
			double? bestShiftValue = null;
			IShiftProjectionCache bestShift = null;
            if (shiftList != null)
                foreach (var shiftProjectionCache in shiftList)
                {
                    double? valueForShift = this.valueForShift(skillIntervalDataDictionary, shiftProjectionCache, lengthFactor, useMinimumPersons,
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

		private double? valueForActivity(KeyValuePair<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> keyValuePair, IShiftProjectionCache shiftProjectionCache, WorkShiftLengthHintOption lengthFactor, bool useMinimumPersons, bool useMaximumPersons)
		{
			IActivity skillActivity = keyValuePair.Key;
			double? value = _workShiftValueCalculator.CalculateShiftValue(shiftProjectionCache.MainShiftProjection,
																		  skillActivity, keyValuePair.Value, lengthFactor,
																		  useMinimumPersons, useMaximumPersons);
			

			return value;
		}

		private double? valueForShift(IEnumerable<KeyValuePair<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>> skillIntervalDatas, IShiftProjectionCache shiftProjectionCache, WorkShiftLengthHintOption lengthFactor, bool useMinimumPersons, bool useMaximumPersons)
		{
			double? totalForAllActivitesValue = null;
			foreach (var keyValuePair in skillIntervalDatas)
			{
				double? skillValue = valueForActivity(keyValuePair, shiftProjectionCache, lengthFactor, useMinimumPersons, useMaximumPersons);
				if (!skillValue.HasValue) return null;
				
				if (totalForAllActivitesValue.HasValue)
				{
					totalForAllActivitesValue = totalForAllActivitesValue + skillValue.Value;
				}
				else
				{
					totalForAllActivitesValue = skillValue.Value;
				}

			}

			return totalForAllActivitesValue;
		}
	}
}