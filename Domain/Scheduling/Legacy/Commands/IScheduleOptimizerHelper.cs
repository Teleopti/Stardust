using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IScheduleOptimizerHelper
	{
		void ScheduleSelectedStudents(IList<IScheduleDay> allSelectedSchedules, IBackgroundWorkerWrapper backgroundWorker,
			ISchedulingOptions schedulingOptions);

		void ScheduleSelectedPersonDays(IList<IScheduleDay> allSelectedSchedules, IList<IScheduleMatrixPro> matrixList,
			IList<IScheduleMatrixPro> matrixListAll, bool useOccupancyAdjustment,
			IBackgroundWorkerWrapper backgroundWorker, ISchedulingOptions schedulingOptions);

		IEditableShift PrepareAndChooseBestShift(IScheduleDay schedulePart,
			ISchedulingOptions schedulingOptions,
			IWorkShiftFinderService finderService);

		IWorkShiftFinderResultHolder WorkShiftFinderResultHolder { get; set; }
		void ResetWorkShiftFinderResults();

		void GetBackToLegalState(IList<IScheduleMatrixPro> matrixList,
			ISchedulerStateHolder schedulerStateHolder,
			IBackgroundWorkerWrapper backgroundWorker,
			ISchedulingOptions schedulingOptions,
			DateOnlyPeriod selectedPeriod,
			IList<IScheduleMatrixPro> allMatrixes);

		void DaysOffBackToLegalState(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
			IBackgroundWorkerWrapper backgroundWorker,
			IDayOffTemplate dayOffTemplate,
			bool reschedule,
			ISchedulingOptions schedulingOptions,
			IDaysOffPreferences daysOffPreferences);

		void ReOptimize(IBackgroundWorkerWrapper backgroundWorker, IList<IScheduleDay> selectedDays, ISchedulingOptions schedulingOptions);

		void RemoveShiftCategoryBackToLegalState(
			IList<IScheduleMatrixPro> matrixList,
			IBackgroundWorkerWrapper backgroundWorker, IOptimizationPreferences optimizationPreferences,
			ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IScheduleMatrixPro> allMatrixes);

		IList<IScheduleMatrixOriginalStateContainer> CreateScheduleMatrixOriginalStateContainers(IList<IScheduleDay> scheduleDays, DateOnlyPeriod selectedPeriod);
	}
}