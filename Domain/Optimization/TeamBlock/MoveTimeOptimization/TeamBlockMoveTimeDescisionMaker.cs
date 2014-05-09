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
					if (areFoundDaysValid(currentMoveFromIndex, currentMoveToIndex, matrix))
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


		private static bool areFoundDaysValid(int currentMoveFromIndex, int currentMoveToIndex, IScheduleMatrixPro matrix)
		{
			IScheduleDayPro currentMoveFromDay = matrix.FullWeeksPeriodDays[currentMoveFromIndex];
			IScheduleDayPro currentMoveToDay = matrix.FullWeeksPeriodDays[currentMoveToIndex];

			if (currentMoveFromDay.DaySchedulePart().SignificantPart() != SchedulePartView.MainShift
				 || currentMoveToDay.DaySchedulePart().SignificantPart() != SchedulePartView.MainShift)
				return false;


			TimeSpan? moveFromWorkShiftLength = null;
			TimeSpan? moveToWorkShiftLength = null;

			if (currentMoveFromDay.DaySchedulePart() != null
				&& currentMoveFromDay.DaySchedulePart().ProjectionService() != null)
				moveFromWorkShiftLength = currentMoveFromDay.DaySchedulePart().ProjectionService().CreateProjection().ContractTime();
			if (currentMoveToDay.DaySchedulePart() != null
				&& currentMoveToDay.DaySchedulePart().ProjectionService() != null)
				moveToWorkShiftLength = currentMoveToDay.DaySchedulePart().ProjectionService().CreateProjection().ContractTime();
			if (moveFromWorkShiftLength.HasValue
				 && moveToWorkShiftLength.HasValue
				 && moveFromWorkShiftLength.Value > moveToWorkShiftLength.Value)
				return false;

			return true;
		}

	}

	
}
