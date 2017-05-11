using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class InnerOptimizerHelperHelper : IOptimizerHelperHelper
	{
		private readonly IRestrictionExtractor _restrictionExtractor;
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
		private readonly IPeriodValueCalculatorProvider _periodValueCalculatorProvider;

		public InnerOptimizerHelperHelper(IRestrictionExtractor restrictionExtractor, 
						IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider,
						IPeriodValueCalculatorProvider periodValueCalculatorProvider)
		{
			_restrictionExtractor = restrictionExtractor;
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
			_periodValueCalculatorProvider = periodValueCalculatorProvider;
		}

		public void LockDaysForDayOffOptimization(IEnumerable<IScheduleMatrixPro> matrixList, IOptimizationPreferences optimizationPreferences, DateOnlyPeriod selectedPeriod)
		{
			var schedulingOptionsCreator = new SchedulingOptionsCreator();
			var schedulingOptions = schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);

			//Not needed anymore i think, 
			var restrictionLocker = new MatrixRestrictionLocker(_restrictionExtractor);
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

		public void LockDaysForIntradayOptimization(IEnumerable<IScheduleMatrixPro> matrixList, DateOnlyPeriod selectedPeriod)
		{
			IMatrixOvertimeLocker matrixOvertimeLocker = new MatrixOvertimeLocker(matrixList);
			matrixOvertimeLocker.Execute();
			IMatrixNoMainShiftLocker noMainShiftLocker = new MatrixNoMainShiftLocker(matrixList);
			noMainShiftLocker.Execute();
			var matrixUnselectedDaysLocker = new MatrixUnselectedDaysLocker(matrixList, selectedPeriod);
			matrixUnselectedDaysLocker.Execute();
		}

		private static void lockRestrictionDaysInMatrix(IScheduleMatrixPro matrix, MatrixRestrictionLocker locker, SchedulingOptions schedulingOptions)
		{
			IList<DateOnly> daysToLock = locker.Execute(matrix, schedulingOptions);
			foreach (var dateOnly in daysToLock)
			{
				matrix.LockDay(dateOnly);
			}
		}

		public IPeriodValueCalculator CreatePeriodValueCalculator(IAdvancedPreferences advancedPreferences,
			IScheduleResultDataExtractor dataExtractor)
		{
			return _periodValueCalculatorProvider.CreatePeriodValueCalculator(advancedPreferences, dataExtractor);
		}

		public IScheduleResultDataExtractor CreateAllSkillsDataExtractor(
			IAdvancedPreferences advancedPreferences,
			DateOnlyPeriod selectedPeriod,
			ISchedulingResultStateHolder stateHolder)
		{
			IScheduleResultDataExtractorProvider dataExtractorProvider = _scheduleResultDataExtractorProvider;
			IScheduleResultDataExtractor allSkillsDataExtractor = dataExtractorProvider.CreateAllSkillsDataExtractor(selectedPeriod, stateHolder, advancedPreferences);
			return allSkillsDataExtractor;
		}

		public IScheduleResultDataExtractor CreateTeamBlockAllSkillsDataExtractor(
			IAdvancedPreferences advancedPreferences,
			DateOnlyPeriod selectedPeriod,
			ISchedulingResultStateHolder stateHolder,
			IEnumerable<IScheduleMatrixPro> allScheduleMatrixPros )
		{
			IScheduleResultDataExtractorProvider dataExtractorProvider = _scheduleResultDataExtractorProvider;
			IScheduleResultDataExtractor primarySkillsDataExtractor = dataExtractorProvider.CreatePrimarySkillsDataExtractor(selectedPeriod, stateHolder, advancedPreferences, allScheduleMatrixPros);
			return primarySkillsDataExtractor;
		}
	}
}