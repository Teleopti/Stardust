using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization
{
	public interface ITeamBlockMoveTimeDescisionMaker
	{
		IList<DateOnly> Execute(IScheduleMatrixPro scheduleMatrix,IOptimizationPreferences optimizationPreferences, ISchedulingResultStateHolder schedulingResultStateHolder);
	}

	public class TeamBlockMoveTimeDescisionMaker : ITeamBlockMoveTimeDescisionMaker
	{
		private readonly DayValueUnlockedIndexSorter _dayValueUnlockedIndexSorter;
		private readonly ILockableBitArrayFactory lockableBitArrayFactory;
		private readonly IScheduleResultDataExtractorProvider  _scheduleResultDataExtractorProvider;
		private readonly IValidateFoundMovedDaysSpecification _validateFoundMovedDaysSpecification;

		public TeamBlockMoveTimeDescisionMaker(DayValueUnlockedIndexSorter dayValueUnlockedIndexSorter, ILockableBitArrayFactory lockableBitArrayFactory, IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider, IValidateFoundMovedDaysSpecification validateFoundMovedDaysSpecification)
		{
			_dayValueUnlockedIndexSorter = dayValueUnlockedIndexSorter;
			this.lockableBitArrayFactory = lockableBitArrayFactory;
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
			_validateFoundMovedDaysSpecification = validateFoundMovedDaysSpecification;

		}

		public IList<DateOnly> Execute(IScheduleMatrixPro scheduleMatrix, IOptimizationPreferences optimizationPreferences, ISchedulingResultStateHolder schedulingResultStateHolder)
		{

			return makeDecision(lockableBitArrayFactory.ConvertFromMatrix(false, false, scheduleMatrix), scheduleMatrix, optimizationPreferences, schedulingResultStateHolder);
		}

		private IList<DateOnly> makeDecision(ILockableBitArray lockableBitArray, IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			IScheduleResultDataExtractor scheduleResultDataExtractor =
				_scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(matrix, optimizationPreferences.Advanced, schedulingResultStateHolder);

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
