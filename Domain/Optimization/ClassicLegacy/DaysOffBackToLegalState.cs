using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ClassicLegacy
{
	public class DaysOffBackToLegalState
	{
		private readonly IScheduleMatrixLockableBitArrayConverterEx _scheduleMatrixLockableBitArrayConverterEx;
		private readonly OptimizerHelperHelper _optimizerHelper; 
		private readonly Func<ISchedulingResultStateHolder> _schedulerStateHolder;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

		public DaysOffBackToLegalState(IScheduleMatrixLockableBitArrayConverterEx scheduleMatrixLockableBitArrayConverterEx,
																	OptimizerHelperHelper optimizerHelper,
																	Func<ISchedulingResultStateHolder> schedulerStateHolder,
																	IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_scheduleMatrixLockableBitArrayConverterEx = scheduleMatrixLockableBitArrayConverterEx;
			_optimizerHelper = optimizerHelper;
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public void Execute(IEnumerable<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
			ISchedulingProgress backgroundWorker,
			IDayOffTemplate dayOffTemplate,
			SchedulingOptions schedulingOptions,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			IOptimizationPreferences optimizationPreferences,
			Func<IWorkShiftFinderResultHolder> workShiftFinderResultHolder,
			Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized)
		{
			var solverContainers =
				_optimizerHelper.CreateSmartDayOffSolverContainers(matrixOriginalStateContainers,
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
						WorkShiftFinderResult workShiftFinderResult =
							new WorkShiftFinderResult(backToLegalStateSolverContainer.MatrixOriginalStateContainer.ScheduleMatrix.Person,
								DateOnly.Today)
							{ Successful = false };
						foreach (string descriptionKey in backToLegalStateSolverContainer.FailedSolverDescriptionKeys)
						{
							string localizedText = Resources.ResourceManager.GetString(descriptionKey);
							var workShiftFilterResult = new WorkShiftFilterResult(localizedText, 0, 0);
							workShiftFinderResult.AddFilterResults(workShiftFilterResult);
						}
						workShiftFinderResultHolder().AddResults(new List<WorkShiftFinderResult> { workShiftFinderResult }, DateTime.Now);
					}
				}
			}

			using (PerformanceOutput.ForOperation("Moving days off according to solvers"))
			{
				foreach (ISmartDayOffBackToLegalStateSolverContainer backToLegalStateSolverContainer in solverContainers)
				{
					if (backToLegalStateSolverContainer.Result)
					{
						_optimizerHelper.SyncSmartDayOffContainerWithMatrix(
							backToLegalStateSolverContainer,
							dayOffTemplate,
							_scheduleDayChangeCallback,
							new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling),
							_scheduleMatrixLockableBitArrayConverterEx,
							_schedulerStateHolder(),
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
							var rollbackService = new SchedulePartModifyAndRollbackService(_schedulerStateHolder(), _scheduleDayChangeCallback,
								new ScheduleTagSetter(
									KeepOriginalScheduleTag.Instance));
							rollbackMatrixChanges(originalStateContainer, rollbackService, resourceOptimizerPersonOptimized, optimizationPreferences.Advanced.RefreshScreenInterval);
						}
					}
				}
			}
		}

		private void rollbackMatrixChanges(IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer, ISchedulePartModifyAndRollbackService rollbackService, Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized, int screenRefreshRate)
		{
			var e = new ResourceOptimizerProgressEventArgs(0, 0, Resources.RollingBackSchedulesFor + " " + matrixOriginalStateContainer.ScheduleMatrix.Person.Name, screenRefreshRate);
			resourceOptimizerPersonOptimized(this, e);
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