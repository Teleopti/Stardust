using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IMatrixDataWithToFewDaysOff
	{
		IList<IMatrixData> FindMatrixesWithToFewDaysOff(IList<IMatrixData> matrixDataList);
	}

	public class MatrixDataWithToFewDaysOff : IMatrixDataWithToFewDaysOff
	{
		private readonly Func<ISchedulingResultStateHolder> _resultStateHolder;
		private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;

		public MatrixDataWithToFewDaysOff(Func<ISchedulingResultStateHolder> resultStateHolder, IDayOffsInPeriodCalculator dayOffsInPeriodCalculator)
		{
			_resultStateHolder = resultStateHolder;
			_dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
		}

		public IList<IMatrixData> FindMatrixesWithToFewDaysOff(IList<IMatrixData> matrixDataList)
		{
			IList<IMatrixData> result = new List<IMatrixData>();
			foreach (var matrixData in matrixDataList)
			{
				int targetDaysOff;
				
				IList<IScheduleDay> dayOffDays;
				if (!_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_resultStateHolder().Schedules, matrixData.Matrix.SchedulePeriod, out targetDaysOff, out dayOffDays) && dayOffDays.Count < targetDaysOff)
				{
					result.Add(matrixData);
				}
				else
				{
					var contract = matrixData.Matrix.SchedulePeriod.Contract;
					var employmentType = contract.EmploymentType;

					if(dayOffDays.Count < targetDaysOff && employmentType != EmploymentType.FixedStaffDayWorkTime)
						result.Add(matrixData);
				}
			}

			return result;
		}
	}
}