using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning
{
    public class WorkShiftLegalStateWeekDecisionMaker
    {
        private readonly IWorkShiftLegalStateDayIndexCalculator _dayIndexCalculator;

        public WorkShiftLegalStateWeekDecisionMaker(IWorkShiftLegalStateDayIndexCalculator dayIndexCalculator)
        {
            _dayIndexCalculator = dayIndexCalculator;
        }


        /// <summary>
        /// Executes the calculation on the specified lockable bit array.
        /// </summary>
        /// <param name="lockableBitArray">The lockable bit array.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public int? Execute(ILockableBitArray lockableBitArray, IList<double?> values)
        {
            ReadOnlyCollection<double?> normalizedValues = _dayIndexCalculator.CalculateIndexForReducing(values);

            int currentMaxIndex = int.MinValue;
            double currentMaxValue = double.MinValue;
            foreach (int currentDayIndex in lockableBitArray.UnlockedIndexes)
            {
                double? currentDayValue = normalizedValues[currentDayIndex];

                if(currentDayValue.HasValue 
                    && currentDayValue > currentMaxValue)
                {
                    currentMaxIndex = currentDayIndex;
                    currentMaxValue = currentDayValue.Value;
                }
            }
            return currentMaxIndex;
        }
    }
}
