using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning
{
    public class WorkShiftBackToLegalStateDecisionMaker : IWorkShiftBackToLegalStateDecisionMaker
    {
		private readonly IRelativeDailyDifferencesByAllSkillsExtractor _dataExtractor;
        private readonly IWorkShiftLegalStateDayIndexCalculator _dayIndexCalculator;

        public WorkShiftBackToLegalStateDecisionMaker(
			IRelativeDailyDifferencesByAllSkillsExtractor dataExtractor,
            IWorkShiftLegalStateDayIndexCalculator dayIndexCalculator)
        {
            _dataExtractor = dataExtractor;
            _dayIndexCalculator = dayIndexCalculator;
        }

        /// <summary>
        /// Executes the calculation on the specified lockable bit array.
        /// </summary>
        /// <param name="lockableBitArray">The lockable bit array.</param>
        /// <param name="raise">if set to <c>true</c> [raise].</param>
        /// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public int? Execute(ILockableBitArray lockableBitArray,  bool raise, DateOnlyPeriod period)
        {
            IList<double?> values = _dataExtractor.Values(period);

            ReadOnlyCollection<double?> normalizedValues = 
                raise ? _dayIndexCalculator.CalculateIndexForRaising(values) 
                      : _dayIndexCalculator.CalculateIndexForReducing(values);

            int currentMaxIndex = int.MinValue;
            double currentMaxValue = double.MinValue;
            foreach (int currentDayIndex in lockableBitArray.UnlockedIndexes)
            {
                double? currentDayValue = normalizedValues[currentDayIndex];
                if (currentDayValue.HasValue && currentDayValue.Value > currentMaxValue)
                {
                    currentMaxIndex = currentDayIndex;
                    currentMaxValue = currentDayValue.Value;
                }
            }
            if (currentMaxIndex == int.MinValue)
                return null;
            return currentMaxIndex;
        }
    }
}
