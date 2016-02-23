using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public class IntradayDecisionMaker : IIntradayDecisionMaker
    {
	    private readonly IScheduleMatrixLockableBitArrayConverterEx _matrixConverter;

	    public IntradayDecisionMaker(IScheduleMatrixLockableBitArrayConverterEx matrixConverter)
	    {
		    _matrixConverter = matrixConverter;
	    }

	    /// <summary>
        /// Gets and returns the working day with the highest standard deviation.
        /// </summary>
        /// <returns></returns>
        public DateOnly? Execute(IScheduleMatrixPro matrix, IScheduleResultDataExtractor dataExtractor, IEnumerable<DateOnly> skipDates)
        {
            ILockableBitArray bitArray = _matrixConverter.Convert(matrix, false, false, skipDates);

			IList<double?> values = dataExtractor.Values();

			int? indexOfHighestStandardDeviation = getIndexOfWorkdayWithHighestValue(bitArray, values);
			if (!indexOfHighestStandardDeviation.HasValue)
				return null;
			return matrix.FullWeeksPeriodDays[indexOfHighestStandardDeviation.Value].Day;
        }

        private static int? getIndexOfWorkdayWithHighestValue(ILockableBitArray lockableBitArray, IList<double?> values)
        {
            int? currentIndex = null;
            double currentHighestValue = 0d;

            foreach (int index in lockableBitArray.UnlockedIndexes)
            {
                if (!lockableBitArray.DaysOffBitArray[index])
                {
                    if (!currentIndex.HasValue)
                    {
                        currentIndex = index;
                        continue;
                    }

                    double? currentValue = values[index - lockableBitArray.PeriodArea.Minimum];
                    if (currentValue.HasValue)
                    {
                        if (currentValue.Value > currentHighestValue)
                        {
                            currentIndex = index;
                            currentHighestValue = currentValue.Value;
                        }
                    }
                }
            }
            return currentIndex;
        }
    }
}