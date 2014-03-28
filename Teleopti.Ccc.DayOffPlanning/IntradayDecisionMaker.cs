using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Secret
{
    public class IntradayDecisionMaker : IIntradayDecisionMaker
    {

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public DateOnly? Execute(ILockableBitArray lockableBitArray, IScheduleResultDataExtractor dataExtractor, IScheduleMatrixPro matrix)
		{
			IList<double?> values = dataExtractor.Values();

			int? indexOfHighestStandardDeviation = getIndexOfWorkdayWithHighestValue(lockableBitArray, values);
			if (!indexOfHighestStandardDeviation.HasValue)
				return null;
			return matrix.FullWeeksPeriodDays[indexOfHighestStandardDeviation.Value].Day;
		}
        /// <summary>
        /// Gets and returns the working day with the highest standard deviation.
        /// </summary>
        /// <param name="matrixConverter">The matrix converter.</param>
        /// <param name="dataExtractor">The data extractor.</param>
        /// <returns></returns>
        public DateOnly? Execute(IScheduleMatrixLockableBitArrayConverter matrixConverter, IScheduleResultDataExtractor dataExtractor)
        {
            IScheduleMatrixPro matrix = matrixConverter.SourceMatrix;
            ILockableBitArray bitArray = matrixConverter.Convert(false, false);
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