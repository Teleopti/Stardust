using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Optimization.ClassicLegacy
{
	public class DaysOffBackToLegalState
	{
		private readonly IScheduleMatrixLockableBitArrayConverterEx _scheduleMatrixLockableBitArrayConverterEx;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly ISmartDayOffBackToLegalStateService _smartDayOffBackToLegalStateService;

		public DaysOffBackToLegalState(IScheduleMatrixLockableBitArrayConverterEx scheduleMatrixLockableBitArrayConverterEx,
																	Func<ISchedulerStateHolder> schedulerStateHolder,
																	IScheduleDayChangeCallback scheduleDayChangeCallback,
																	ISmartDayOffBackToLegalStateService smartDayOffBackToLegalStateService)
		{
			_scheduleMatrixLockableBitArrayConverterEx = scheduleMatrixLockableBitArrayConverterEx;
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_smartDayOffBackToLegalStateService = smartDayOffBackToLegalStateService;
		}

		public void Execute(IEnumerable<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
			SchedulingOptions schedulingOptions,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			IOptimizationPreferences optimizationPreferences)
		{	
			var solverContainers =
				createSmartDayOffSolverContainers(matrixOriginalStateContainers,
					_scheduleMatrixLockableBitArrayConverterEx, dayOffOptimizationPreferenceProvider);

			

			using (PerformanceOutput.ForOperation("SmartSolver for " + solverContainers.Count + " containers"))
			{
				foreach (ISmartDayOffBackToLegalStateSolverContainer backToLegalStateSolverContainer in solverContainers)
				{
					var matrix = backToLegalStateSolverContainer.MatrixOriginalStateContainer.ScheduleMatrix;
					var dayOffOptimizePreference = dayOffOptimizationPreferenceProvider.ForAgent(matrix.Person,
						matrix.EffectivePeriodDays.First().Day);

					backToLegalStateSolverContainer.Execute(dayOffOptimizePreference);

					//create list to send to bruteforce
					if (!backToLegalStateSolverContainer.Result)
					{
						backToLegalStateSolverContainer.MatrixOriginalStateContainer.StillAlive = false;
					}
				}
			}

			using (PerformanceOutput.ForOperation("Moving days off according to solvers"))
			{
				foreach (ISmartDayOffBackToLegalStateSolverContainer backToLegalStateSolverContainer in solverContainers)
				{
					if (backToLegalStateSolverContainer.Result)
					{
						syncSmartDayOffContainerWithMatrix(
							backToLegalStateSolverContainer,
							_schedulerStateHolder().CommonStateHolder.DayOffs.NonDeleted().ToList()[0],
							_scheduleDayChangeCallback,
							new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling),
							_scheduleMatrixLockableBitArrayConverterEx,
							_schedulerStateHolder().SchedulingResultState,
							dayOffOptimizationPreferenceProvider
							);

						var restrictionChecker = new RestrictionChecker();
						var matrix = backToLegalStateSolverContainer.MatrixOriginalStateContainer.ScheduleMatrix;

						var dayOffOptimizePrefrerence = dayOffOptimizationPreferenceProvider.ForAgent(matrix.Person,
							matrix.EffectivePeriodDays.First().Day);

						var originalStateContainer = backToLegalStateSolverContainer.MatrixOriginalStateContainer;
						var optimizationOverLimitByRestrictionDecider = new OptimizationOverLimitByRestrictionDecider(
							restrictionChecker, optimizationPreferences, originalStateContainer, dayOffOptimizePrefrerence);

						var optimizationLimits = new OptimizationLimits(optimizationOverLimitByRestrictionDecider);
						var overLimitCounts = optimizationLimits.OverLimitsCounts(matrix);


						if (overLimitCounts.AvailabilitiesOverLimit > 0 || overLimitCounts.MustHavesOverLimit > 0 ||
							overLimitCounts.PreferencesOverLimit > 0 || overLimitCounts.RotationsOverLimit > 0 ||
							overLimitCounts.StudentAvailabilitiesOverLimit > 0 ||
							optimizationLimits.MoveMaxDaysOverLimit())
						{
							var rollbackService = new SchedulePartModifyAndRollbackService(_schedulerStateHolder().SchedulingResultState, _scheduleDayChangeCallback,
								new ScheduleTagSetter(
									KeepOriginalScheduleTag.Instance));
							rollbackMatrixChanges(originalStateContainer, rollbackService, optimizationPreferences.Advanced.RefreshScreenInterval);
						}
					}
				}
			}
		}

		private void syncSmartDayOffContainerWithMatrix(
			ISmartDayOffBackToLegalStateSolverContainer backToLegalStateSolverContainer,
			IDayOffTemplate dayOffTemplate,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			IScheduleTagSetter scheduleTagSetter,
			IScheduleMatrixLockableBitArrayConverterEx scheduleMatrixLockableBitArrayConverterEx,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			if (backToLegalStateSolverContainer.Result)
			{
				IScheduleMatrixPro matrix = backToLegalStateSolverContainer.MatrixOriginalStateContainer.ScheduleMatrix;

				var dayOffOptimizationPreference = dayOffOptimizationPreferenceProvider.ForAgent(matrix.Person, matrix.EffectivePeriodDays.First().Day);

				ILockableBitArray doArray = scheduleMatrixLockableBitArrayConverterEx.Convert(matrix, dayOffOptimizationPreference.ConsiderWeekBefore, dayOffOptimizationPreference.ConsiderWeekAfter);

				int bitArrayToMatrixOffset = 0;
				if (!dayOffOptimizationPreference.ConsiderWeekBefore)
					bitArrayToMatrixOffset = 7;

				for (int i = 0; i < doArray.Count; i++)
				{
					if (doArray[i] && !backToLegalStateSolverContainer.BitArray[i])
					{
						IScheduleDay part =
							matrix.OuterWeeksPeriodDays[i + bitArrayToMatrixOffset].DaySchedulePart();
						part.DeleteDayOff();
						new SchedulePartModifyAndRollbackService(schedulingResultStateHolder, scheduleDayChangeCallback, scheduleTagSetter).Modify(part);
					}
					else
					{
						if (!doArray[i] && backToLegalStateSolverContainer.BitArray[i])
						{
							IScheduleDay part =
								matrix.OuterWeeksPeriodDays[i + bitArrayToMatrixOffset].DaySchedulePart();
							part.DeleteMainShift();
							part.CreateAndAddDayOff(dayOffTemplate);
							new SchedulePartModifyAndRollbackService(schedulingResultStateHolder, scheduleDayChangeCallback, scheduleTagSetter).Modify(part);
						}
					}

				}
			}
		}

		private IList<ISmartDayOffBackToLegalStateSolverContainer> createSmartDayOffSolverContainers(
			IEnumerable<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
			IScheduleMatrixLockableBitArrayConverterEx scheduleMatrixLockableBitArrayConverterEx,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			IList<ISmartDayOffBackToLegalStateSolverContainer> solverContainers = new List<ISmartDayOffBackToLegalStateSolverContainer>();
			foreach (var matrixOriginalStateContainer in matrixOriginalStateContainers)
			{
				var matrix = matrixOriginalStateContainer.ScheduleMatrix;
				var dayOffOptimizationPreference = dayOffOptimizationPreferenceProvider.ForAgent(matrix.Person, matrix.EffectivePeriodDays.First().Day);
				ILockableBitArray bitArray = scheduleMatrixLockableBitArrayConverterEx.Convert(
					matrixOriginalStateContainer.ScheduleMatrix, dayOffOptimizationPreference.ConsiderWeekBefore,
					dayOffOptimizationPreference.ConsiderWeekAfter);
				ISmartDayOffBackToLegalStateSolverContainer solverContainer =
					new SmartDayOffBackToLegalStateSolverContainer(matrixOriginalStateContainer, bitArray, _smartDayOffBackToLegalStateService, _schedulerStateHolder().SchedulingResultState);
				solverContainers.Add(solverContainer);
			}

			return solverContainers;
		}

		private static void rollbackMatrixChanges(IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer, ISchedulePartModifyAndRollbackService rollbackService, int screenRefreshRate)
		{
			var e = new ResourceOptimizerProgressEventArgs(0, 0, Resources.RollingBackSchedulesFor + " " + matrixOriginalStateContainer.ScheduleMatrix.Person.Name, screenRefreshRate);
			if (e.Cancel) return;

			rollbackService.ClearModificationCollection();
			foreach (var scheduleDayPro in matrixOriginalStateContainer.ScheduleMatrix.EffectivePeriodDays)
			{
				var originalPart = matrixOriginalStateContainer.OldPeriodDaysState[scheduleDayPro.Day];
				rollbackService.Modify(originalPart);
			}
		}
	}
}