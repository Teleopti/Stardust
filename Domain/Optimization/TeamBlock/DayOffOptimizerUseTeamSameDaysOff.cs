using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_TeamSameDayOff_44265)]
	public class DayOffOptimizerUseTeamSameDaysOff : IDayOffOptimizerUseTeamSameDaysOff
	{
		private readonly ILockableBitArrayFactory _lockableBitArrayFactory;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly ITeamDayOffModifier _teamDayOffModifier;
		private readonly ITeamBlockSteadyStateValidator _teamBlockSteadyStateValidator;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockOptimizationLimits _teamBlockOptimizationLimits;
		private readonly ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		private readonly ITeamBlockDayOffsInPeriodValidator _teamBlockDayOffsInPeriodValidator;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;
		private readonly ITeamBlockDaysOffMoveFinder _teamBlockDaysOffMoveFinder;
		private readonly AffectedDayOffs _affectedDayOffs;

		public DayOffOptimizerUseTeamSameDaysOff(ILockableBitArrayFactory lockableBitArrayFactory,
			ITeamBlockScheduler teamBlockScheduler,
			ITeamBlockInfoFactory teamBlockInfoFactory,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			ITeamDayOffModifier teamDayOffModifier,
			ITeamBlockSteadyStateValidator teamBlockSteadyStateValidator,
			ITeamBlockClearer teamBlockClearer,
			ITeamBlockOptimizationLimits teamBlockOptimizationLimits,
			ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator,
			ITeamBlockDayOffsInPeriodValidator teamBlockDayOffsInPeriodValidator,
			IWorkShiftSelector workShiftSelector,
			ITeamBlockDaysOffMoveFinder teamBlockDaysOffMoveFinder,
			IGroupPersonSkillAggregator groupPersonSkillAggregator,
			AffectedDayOffs affectedDayOffs)
		{
			_lockableBitArrayFactory = lockableBitArrayFactory;
			_teamBlockScheduler = teamBlockScheduler;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_teamDayOffModifier = teamDayOffModifier;
			_teamBlockSteadyStateValidator = teamBlockSteadyStateValidator;
			_teamBlockClearer = teamBlockClearer;
			_teamBlockOptimizationLimits = teamBlockOptimizationLimits;
			_teamBlockShiftCategoryLimitationValidator = teamBlockShiftCategoryLimitationValidator;
			_teamBlockDayOffsInPeriodValidator = teamBlockDayOffsInPeriodValidator;
			_workShiftSelector = workShiftSelector;
			_teamBlockDaysOffMoveFinder = teamBlockDaysOffMoveFinder;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
			_affectedDayOffs = affectedDayOffs;
		}

		public IEnumerable<ITeamInfo> Execute(IPeriodValueCalculator periodValueCalculatorForAllSkills,
			IOptimizationPreferences optimizationPreferences, ISchedulePartModifyAndRollbackService rollbackService,
			IEnumerable<ITeamInfo> remainingInfoList, SchedulingOptions schedulingOptions, IEnumerable<IPerson> selectedPersons,
			IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulingResultStateHolder schedulingResultStateHolder, Action cancelAction,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, ISchedulingProgress schedulingProgress)
		{
			var teamInfosToRemove = new HashSet<ITeamInfo>();
			double previousPeriodValue =
				periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
			var totalLiveTeamInfos = remainingInfoList.Count();
			var currentTeamInfoCounter = 0;
			var cancelMe = false;
			foreach (ITeamInfo teamInfo in remainingInfoList.GetRandom(remainingInfoList.Count(), true))
			{
				currentTeamInfoCounter++;

				var allFailed = true;
				foreach (IScheduleMatrixPro matrix in teamInfo.MatrixesForGroupMember(0))
				{
					rollbackService.ClearModificationCollection();

					var dayOffOptimizationPreference = dayOffOptimizationPreferenceProvider.ForAgent(matrix.Person, matrix.EffectivePeriodDays.First().Day);

					bool checkPeriodValue;
					var success = runOneTeam(optimizationPreferences, rollbackService, schedulingOptions, matrix, teamInfo,
						resourceCalculateDelayer,
						schedulingResultStateHolder,
						dayOffOptimizationPreference,
						out checkPeriodValue);
					var currentPeriodValue = new Lazy<double>(() => periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization));
					success = handleResult(rollbackService, schedulingOptions, previousPeriodValue, success,
						teamInfo, totalLiveTeamInfos, currentTeamInfoCounter, currentPeriodValue, checkPeriodValue, () =>
						{
							cancelMe = true;
							cancelAction();
						}, schedulingProgress, optimizationPreferences.Advanced.RefreshScreenInterval);

					if (success)
					{
						allFailed = false;
						previousPeriodValue = currentPeriodValue.Value;
					}

					if (cancelMe)
						break;
				}

				if (allFailed)
					teamInfosToRemove.Add(teamInfo);

				if (cancelMe)
					break;
			}

			return teamInfosToRemove;
		}

		private bool runOneTeam(IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService rollbackService,
			SchedulingOptions schedulingOptions, IScheduleMatrixPro matrix,
			ITeamInfo teamInfo,
			IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IDaysOffPreferences daysOffPreferences,
			out bool checkPeriodValue)
		{
			var originalArray = _lockableBitArrayFactory.ConvertFromMatrix(daysOffPreferences.ConsiderWeekBefore, daysOffPreferences.ConsiderWeekAfter, matrix);
			var resultingArray = _teamBlockDaysOffMoveFinder.TryFindMoves(matrix, originalArray, optimizationPreferences, daysOffPreferences, schedulingResultStateHolder);

			var movedDaysOff = _affectedDayOffs.Execute(matrix, daysOffPreferences, originalArray, resultingArray);
			if (movedDaysOff == null)
			{
				checkPeriodValue = true;
				return false;
			}

			removeAllDecidedDaysOffForTeam(rollbackService, teamInfo, movedDaysOff.RemovedDaysOff);
			addAllDecidedDaysOffForTeam(rollbackService, schedulingOptions, teamInfo, movedDaysOff.AddedDaysOff);

			bool success = reScheduleAllMovedDaysOff(schedulingOptions, teamInfo,
				movedDaysOff.RemovedDaysOff,
				rollbackService, resourceCalculateDelayer,
				schedulingResultStateHolder);
			if (!success)
			{
				checkPeriodValue = true;
				return false;
			}

			if (!optimizationPreferences.General.OptimizationStepDaysOff && optimizationPreferences.General.OptimizationStepDaysOffForFlexibleWorkTime)
			{
				var flexibleDayOffvalidator = _teamBlockDayOffsInPeriodValidator;
				if (!flexibleDayOffvalidator.Validate(teamInfo, schedulingResultStateHolder))
				{
					_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
					teamInfo.LockDays(movedDaysOff.AddedDaysOff);
					teamInfo.LockDays(movedDaysOff.RemovedDaysOff);
					checkPeriodValue = true;
					return false;
				}
			}

			if (!_teamBlockOptimizationLimits.ValidateMinWorkTimePerWeek(teamInfo))
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				teamInfo.LockDays(movedDaysOff.AddedDaysOff);
				checkPeriodValue = false;
				return true;
			}

			foreach (var dateOnly in movedDaysOff.RemovedDaysOff)
			{
				var teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly, schedulingOptions.BlockFinder());

				if (!_teamBlockShiftCategoryLimitationValidator.Validate(teamBlockInfo, null, optimizationPreferences))
				{
					_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
					teamInfo.LockDays(movedDaysOff.AddedDaysOff);
					teamInfo.LockDays(movedDaysOff.RemovedDaysOff);
				}
			}

			checkPeriodValue = true;
			return true;
		}


		private bool handleResult(ISchedulePartModifyAndRollbackService rollbackService, SchedulingOptions schedulingOptions,
			double previousPeriodValue, bool success, ITeamInfo teamInfo,
			int totalLiveTeamInfos, int currentTeamInfoCounter, Lazy<double> currentPeriodCalculator,
			bool checkPeriodValue, Action cancelAction, ISchedulingProgress schedulingProgress, int screenRefreshRate)
		{
			var currentPeriodValue = previousPeriodValue;
			if (success && checkPeriodValue)
			{
				currentPeriodValue = currentPeriodCalculator.Value;
				success = currentPeriodValue < previousPeriodValue;
			}
			if (!success)
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
			}
			if (onReportProgress(schedulingProgress, totalLiveTeamInfos, currentTeamInfoCounter, teamInfo, currentPeriodValue, screenRefreshRate))
			{
				cancelAction();
			}

			return success;
		}

		private bool reScheduleAllMovedDaysOff(SchedulingOptions schedulingOptions, ITeamInfo teamInfo,
			IEnumerable<DateOnly> removedDaysOff,
			ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			foreach (DateOnly dateOnly in removedDaysOff)
			{
				var teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly, schedulingOptions.BlockFinder());
				if (teamBlockInfo == null)
					continue;
				if (!_teamBlockSteadyStateValidator.IsTeamBlockInSteadyState(teamBlockInfo, schedulingOptions))
					_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);

				if (!_teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelector, teamBlockInfo, dateOnly, schedulingOptions, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder.AllSkillDays(), schedulingResultStateHolder.Schedules, new ShiftNudgeDirective(), NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder), _groupPersonSkillAggregator))
					return false;
			}

			return true;
		}

		private void addAllDecidedDaysOffForTeam(ISchedulePartModifyAndRollbackService rollbackService,
			SchedulingOptions schedulingOptions, ITeamInfo teamInfo,
			IEnumerable<DateOnly> addedDaysOff)
		{
			foreach (DateOnly dateOnly in addedDaysOff)
			{
				_teamDayOffModifier.AddDayOffForTeamAndResourceCalculate(rollbackService, teamInfo, dateOnly,
					schedulingOptions.DayOffTemplate);
			}
		}

		private void removeAllDecidedDaysOffForTeam(ISchedulePartModifyAndRollbackService rollbackService,
			ITeamInfo teamInfo,
			IEnumerable<DateOnly> removedDaysOff)
		{
			foreach (DateOnly dateOnly in removedDaysOff)
			{
				_teamDayOffModifier.RemoveDayOffForTeam(rollbackService, teamInfo, dateOnly);
			}
		}

		private static bool onReportProgress(ISchedulingProgress schedulingProgress, int totalNumberOfTeamInfos, int teamInfoCounter, ITeamInfo currentTeamInfo, double periodValue, int screenRefreshRate)
		{
			if (schedulingProgress.CancellationPending)
			{
				return true;
			}
			var eventArgs = new ResourceOptimizerProgressEventArgs(0, 0,
				Resources.OptimizingDaysOff + Resources.Colon + "(" + totalNumberOfTeamInfos.ToString("####") + ")(" +
				teamInfoCounter.ToString("####") + ") " + currentTeamInfo.Name.DisplayString(20) + " (" + periodValue + ")", screenRefreshRate);
			schedulingProgress.ReportProgress(1, eventArgs);
			return false;
		}
	}
}