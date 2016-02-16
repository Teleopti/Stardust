using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class InnerOptimizerHelperHelper : IOptimizerHelperHelper
	{
		private readonly IRestrictionExtractor _restrictionExtractor;

		public InnerOptimizerHelperHelper(IRestrictionExtractor restrictionExtractor)
		{
			_restrictionExtractor = restrictionExtractor;
		}

		public void LockDaysForDayOffOptimization(IList<IScheduleMatrixPro> matrixList, IOptimizationPreferences optimizationPreferences, DateOnlyPeriod selectedPeriod)
		{
			var schedulingOptionsCreator = new SchedulingOptionsCreator();
			var schedulingOptions = schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);

			//Not needed anymore i think, 
			IMatrixRestrictionLocker restrictionLocker = new MatrixRestrictionLocker(_restrictionExtractor);
			foreach (IScheduleMatrixPro scheduleMatrixPro in matrixList)
				lockRestrictionDaysInMatrix(scheduleMatrixPro, restrictionLocker, schedulingOptions);
			IMatrixMeetingDayLocker meetingDayLocker = new MatrixMeetingDayLocker(matrixList);
			meetingDayLocker.Execute();
			IMatrixPersonalShiftLocker personalShiftLocker = new MatrixPersonalShiftLocker(matrixList);
			personalShiftLocker.Execute();
			IMatrixOvertimeLocker matrixOvertimeLocker = new MatrixOvertimeLocker(matrixList);
			matrixOvertimeLocker.Execute();
			IMatrixNoMainShiftLocker noMainShiftLocker = new MatrixNoMainShiftLocker(matrixList);
			noMainShiftLocker.Execute();
			IMatrixShiftsNotAvailibleLocker matrixShiftsNotAvailibleLocker = new MatrixShiftsNotAvailibleLocker();
			matrixShiftsNotAvailibleLocker.Execute(matrixList);
			var matrixUnselectedDaysLocker = new MatrixUnselectedDaysLocker(matrixList, selectedPeriod);
			matrixUnselectedDaysLocker.Execute();

			var matrixKeepActivityLocker = new MatrixKeepActivityLocker(matrixList, optimizationPreferences.Shifts.SelectedActivities);
			matrixKeepActivityLocker.Execute();
		}

		public void LockDaysForIntradayOptimization(IList<IScheduleMatrixPro> matrixList, DateOnlyPeriod selectedPeriod)
		{
			IMatrixOvertimeLocker matrixOvertimeLocker = new MatrixOvertimeLocker(matrixList);
			matrixOvertimeLocker.Execute();
			IMatrixNoMainShiftLocker noMainShiftLocker = new MatrixNoMainShiftLocker(matrixList);
			noMainShiftLocker.Execute();
			var matrixUnselectedDaysLocker = new MatrixUnselectedDaysLocker(matrixList, selectedPeriod);
			matrixUnselectedDaysLocker.Execute();
		}

		private static void lockRestrictionDaysInMatrix(IScheduleMatrixPro matrix, IMatrixRestrictionLocker locker, ISchedulingOptions schedulingOptions)
		{
			IList<DateOnly> daysToLock = locker.Execute(matrix, schedulingOptions);
			foreach (var dateOnly in daysToLock)
			{
				matrix.LockPeriod(new DateOnlyPeriod(dateOnly, dateOnly));
			}
		}

		public IPeriodValueCalculator CreatePeriodValueCalculator(IAdvancedPreferences advancedPreferences,
			IScheduleResultDataExtractor dataExtractor)
		{
			IPeriodValueCalculatorProvider calculatorProvider = new PeriodValueCalculatorProvider();
			return calculatorProvider.CreatePeriodValueCalculator(advancedPreferences, dataExtractor);
		}

		public IScheduleResultDataExtractor CreateAllSkillsDataExtractor(
			IAdvancedPreferences advancedPreferences,
			DateOnlyPeriod selectedPeriod,
			ISchedulingResultStateHolder stateHolder)
		{
			IScheduleResultDataExtractorProvider dataExtractorProvider = new ScheduleResultDataExtractorProvider();
			IScheduleResultDataExtractor allSkillsDataExtractor = dataExtractorProvider.CreateAllSkillsDataExtractor(selectedPeriod, stateHolder, advancedPreferences);
			return allSkillsDataExtractor;
		}
	}
}