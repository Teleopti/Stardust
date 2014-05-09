using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization
{
	public interface ITeamBlockMoveTimeDescisionMaker
	{
		IList<DateOnly> Execute(IScheduleMatrixLockableBitArrayConverter matrixConverter, IScheduleResultDataExtractor dataExtractor);
	}

	public class TeamBlockMoveTimeDescisionMaker : ITeamBlockMoveTimeDescisionMaker
	{
		private readonly IDayValueUnlockedIndexSorter _dayValueUnlockedIndexSorter;
	    private readonly IValidateFoundMovedDaysSpecification _validateFoundMovedDaysSpecification;

	    public TeamBlockMoveTimeDescisionMaker(IDayValueUnlockedIndexSorter dayValueUnlockedIndexSorter, IValidateFoundMovedDaysSpecification validateFoundMovedDaysSpecification)
		{
			_dayValueUnlockedIndexSorter = dayValueUnlockedIndexSorter;
		    _validateFoundMovedDaysSpecification = validateFoundMovedDaysSpecification;
		}

		public IList<DateOnly> Execute(IScheduleMatrixLockableBitArrayConverter matrixConverter, IScheduleResultDataExtractor dataExtractor)
		{
			return makeDecision(matrixConverter.Convert(false, false), matrixConverter.SourceMatrix, dataExtractor);
		}

		private IList<DateOnly> makeDecision(ILockableBitArray lockableBitArray, IScheduleMatrixPro matrix,
													 IScheduleResultDataExtractor dataExtractor)
		{
			IList<DateOnly> result = new List<DateOnly>(2);
			var values = dataExtractor.Values();

			var indexesToMoveFrom = _dayValueUnlockedIndexSorter.SortAscending(lockableBitArray, values);
			var indexesToMoveTo = _dayValueUnlockedIndexSorter.SortDescending(lockableBitArray, values);
			foreach (int currentMoveFromIndex in indexesToMoveFrom)
			{
				foreach (int currentMoveToIndex in indexesToMoveTo)
				{
					if (currentMoveToIndex == currentMoveFromIndex) continue;
					if (values[currentMoveToIndex - lockableBitArray.PeriodArea.Minimum] <
						 values[currentMoveFromIndex - lockableBitArray.PeriodArea.Minimum])
						break;
					if (_validateFoundMovedDaysSpecification.AreFoundDaysValid(currentMoveFromIndex, currentMoveToIndex, matrix))
					{
						DateOnly mostUnderStaffedDay = matrix.FullWeeksPeriodDays[currentMoveFromIndex].Day;
						DateOnly mostOverStaffedDay = matrix.FullWeeksPeriodDays[currentMoveToIndex].Day;
						result.Add(mostUnderStaffedDay);
						result.Add(mostOverStaffedDay);
						return result;
					}
				}
			}
			return result;
		}
	}

	
}
