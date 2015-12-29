using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IMatrixDataWithToFewDaysOff
	{
		IList<IMatrixData> FindMatrixesWithToFewDaysOff(IList<IMatrixData> matrixDataList);
	}

	public class MatrixDataWithToFewDaysOff : IMatrixDataWithToFewDaysOff
	{
		private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;

		public MatrixDataWithToFewDaysOff(IDayOffsInPeriodCalculator dayOffsInPeriodCalculator)
		{
			_dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IList<IMatrixData> FindMatrixesWithToFewDaysOff(IList<IMatrixData> matrixDataList)
		{
			IList<IMatrixData> result = new List<IMatrixData>();
			foreach (var matrixData in matrixDataList)
			{
				int targetDaysOff;
				
				IList<IScheduleDay> dayOffDays = new List<IScheduleDay>();
				if (!_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(matrixData.Matrix.SchedulePeriod, out targetDaysOff, out dayOffDays) && dayOffDays.Count < targetDaysOff)
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