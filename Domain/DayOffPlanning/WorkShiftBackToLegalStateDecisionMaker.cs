using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public class WorkShiftBackToLegalStateDecisionMaker : IWorkShiftBackToLegalStateDecisionMaker
    {
		private readonly IRelativeDailyDifferencesByAllSkillsExtractor _dataExtractor;
        private readonly WorkShiftLegalStateDayIndexCalculator _dayIndexCalculator;

        public WorkShiftBackToLegalStateDecisionMaker(
			IRelativeDailyDifferencesByAllSkillsExtractor dataExtractor,
						WorkShiftLegalStateDayIndexCalculator dayIndexCalculator)
        {
            _dataExtractor = dataExtractor;
            _dayIndexCalculator = dayIndexCalculator;
        }

		public int? Execute(ILockableBitArray lockableBitArray,  bool raise, DateOnlyPeriod period)
        {
            IList<double?> values = _dataExtractor.Values(period);

            var normalizedValues = 
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
