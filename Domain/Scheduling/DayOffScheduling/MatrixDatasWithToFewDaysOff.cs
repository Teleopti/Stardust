using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IMatrixDatasWithToFewDaysOff
	{
		IList<IMatrixData> FindMatrixesWithToFewDaysOff(IList<IMatrixData> matrixDataList);
	}

	public class MatrixDatasWithToFewDaysOff : IMatrixDatasWithToFewDaysOff
	{
		private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;

		public MatrixDatasWithToFewDaysOff(IDayOffsInPeriodCalculator dayOffsInPeriodCalculator)
		{
			_dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
		}

		public IList<IMatrixData> FindMatrixesWithToFewDaysOff(IList<IMatrixData> matrixDataList)
		{
			IList<IMatrixData> result = new List<IMatrixData>();
			foreach (var matrixData in matrixDataList)
			{
				int targetDaysOff;
				int daysOffNow;
				if (!_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(matrixData.Matrix.SchedulePeriod, out targetDaysOff, out daysOffNow))
					result.Add(matrixData);
			}

			return result;
		}
	}
}