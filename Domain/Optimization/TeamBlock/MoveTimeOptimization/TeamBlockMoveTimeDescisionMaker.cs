using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization
{
	public interface ITeamBlockMoveTimeDescisionMaker
	{
		IList<DateOnly> Execute(IScheduleMatrixPro scheduleMatrix,IOptimizationPreferences optimizationPreferences);
	}

	public class TeamBlockMoveTimeDescisionMaker : ITeamBlockMoveTimeDescisionMaker
	{
		private readonly IDayValueUnlockedIndexSorter _dayValueUnlockedIndexSorter;
		private readonly ILockableBitArrayFactory lockableBitArrayFactory;
		private readonly IScheduleResultDataExtractorProvider  _scheduleResultDataExtractorProvider;

		public TeamBlockMoveTimeDescisionMaker(IDayValueUnlockedIndexSorter dayValueUnlockedIndexSorter, ILockableBitArrayFactory lockableBitArrayFactory, IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider)
		{
			_dayValueUnlockedIndexSorter = dayValueUnlockedIndexSorter;
			this.lockableBitArrayFactory = lockableBitArrayFactory;
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
		}

		public IList<DateOnly> Execute(IScheduleMatrixPro scheduleMatrix, IOptimizationPreferences optimizationPreferences)
		{
			
			return makeDecision(lockableBitArrayFactory.ConvertFromMatrix(false,false,scheduleMatrix ),scheduleMatrix  ,optimizationPreferences );
		}

		private IList<DateOnly> makeDecision(ILockableBitArray lockableBitArray, IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences)
		{
			IScheduleResultDataExtractor scheduleResultDataExtractor =
				_scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(matrix, optimizationPreferences.Advanced);

			IList<DateOnly> result = new List<DateOnly>(2);
			var values = scheduleResultDataExtractor.Values();

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
