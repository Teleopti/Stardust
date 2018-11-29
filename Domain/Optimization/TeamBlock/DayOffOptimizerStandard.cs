using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class DayOffOptimizerStandard
	{
		private readonly ILockableBitArrayFactory _lockableBitArrayFactory;
		private readonly TeamBlockScheduler _teamBlockScheduler;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly ITeamDayOffModifier _teamDayOffModifier;
		private readonly ITeamBlockSteadyStateValidator _teamBlockSteadyStateValidator;
		private readonly TeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockOptimizationLimits _teamBlockOptimizationLimits;
		private readonly ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		private readonly TeamBlockDayOffsInPeriodValidator _teamBlockDayOffsInPeriodValidator;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;
		private readonly DayOffOptimizerPreMoveResultPredictor _dayOffOptimizerPreMoveResultPredictor;
		private readonly ITeamBlockDaysOffMoveFinder _teamBlockDaysOffMoveFinder;
		private readonly AffectedDayOffs _affectedDayOffs;
		private readonly IShiftCategoryLimitationChecker _shiftCategoryLimitationChecker;
		private readonly INightRestWhiteSpotSolverServiceFactory _nightRestWhiteSpotSolverServiceFactory;
		private readonly BlockPreferencesMapper _blockPreferencesMapper;
		private readonly ICurrentOptimizationCallback _currentOptimizationCallback;

		public DayOffOptimizerStandard(
			ILockableBitArrayFactory lockableBitArrayFactory,
			TeamBlockScheduler teamBlockScheduler,
			ITeamBlockInfoFactory teamBlockInfoFactory,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			ITeamDayOffModifier teamDayOffModifier,
			ITeamBlockSteadyStateValidator teamBlockSteadyStateValidator,
			TeamBlockClearer teamBlockClearer,
			ITeamBlockOptimizationLimits teamBlockOptimizationLimits,
			ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator,
			TeamBlockDayOffsInPeriodValidator teamBlockDayOffsInPeriodValidator,
			IWorkShiftSelector workShiftSelector,
			IGroupPersonSkillAggregator groupPersonSkillAggregator,
			DayOffOptimizerPreMoveResultPredictor dayOffOptimizerPreMoveResultPredictor,
			ITeamBlockDaysOffMoveFinder teamBlockDaysOffMoveFinder,
			AffectedDayOffs affectedDayOffs,
			IShiftCategoryLimitationChecker shiftCategoryLimitationChecker,
			INightRestWhiteSpotSolverServiceFactory nightRestWhiteSpotSolverServiceFactory,
			BlockPreferencesMapper blockPreferencesMapper,
			ICurrentOptimizationCallback currentOptimizationCallback)
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
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
			_dayOffOptimizerPreMoveResultPredictor = dayOffOptimizerPreMoveResultPredictor;
			_teamBlockDaysOffMoveFinder = teamBlockDaysOffMoveFinder;
			_affectedDayOffs = affectedDayOffs;
			_shiftCategoryLimitationChecker = shiftCategoryLimitationChecker;
			_nightRestWhiteSpotSolverServiceFactory = nightRestWhiteSpotSolverServiceFactory;
			_blockPreferencesMapper = blockPreferencesMapper;
			_currentOptimizationCallback = currentOptimizationCallback;
		}

		public IEnumerable<ITeamInfo> Execute(IOptimizationPreferences optimizationPreferences, ISchedulePartModifyAndRollbackService rollbackService,
			IEnumerable<ITeamInfo> remainingInfoList, SchedulingOptions schedulingOptions, IEnumerable<IPerson> selectedPersons,
			IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder,
			Action cancelAction,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			IBlockPreferenceProvider blockPreferenceProvider,
			ISchedulingProgress schedulingProgress)
		{
			var currentMatrixCounter = 0;
			var stopOptimizeTeamInfo = new Dictionary<ITeamInfo, bool>();
			var matrixes = new List<Tuple<IScheduleMatrixPro, ITeamInfo>>();
			var callback = _currentOptimizationCallback.Current();
			foreach (var teamInfo in remainingInfoList)
			{
				stopOptimizeTeamInfo[teamInfo] = true;
				matrixes.AddRange(teamInfo.MatrixesForGroup().Select(scheduleMatrixPro => new Tuple<IScheduleMatrixPro, ITeamInfo>(scheduleMatrixPro, teamInfo)));
			}
			
			foreach (var matrix in matrixes.Randomize())
			{
				if (callback.IsCancelled())
					return null;
				var blockPreference = blockPreferenceProvider.ForAgent(matrix.Item1.Person, matrix.Item1.EffectivePeriodDays.First().Day);
				_blockPreferencesMapper.UpdateOptimizationPreferencesFromExtraPreferences(optimizationPreferences, blockPreference);
				_blockPreferencesMapper.UpdateSchedulingOptionsFromOptimizationPreferences(schedulingOptions, optimizationPreferences);

				currentMatrixCounter++;
				
				//This if is really strange... Remove?
				if (!(optimizationPreferences.Extra.UseTeamBlockOption && optimizationPreferences.Extra.UseTeamSameDaysOff))
				{
					if (!selectedPersons.Contains(matrix.Item1.Person))
						continue;
				}

				var numberOfDayOffsMoved = optimizationPreferences.Extra.UseTeams && optimizationPreferences.Extra.UseTeamSameDaysOff
					? matrix.Item2.GroupMembers.Count()
					: 1;

				rollbackService.ClearModificationCollection();
				var dayOffOptimizationPreference = dayOffOptimizationPreferenceProvider.ForAgent(matrix.Item1.Person, matrix.Item1.EffectivePeriodDays.First().Day);

				var originalArray = _lockableBitArrayFactory.ConvertFromMatrix(dayOffOptimizationPreference.ConsiderWeekBefore, dayOffOptimizationPreference.ConsiderWeekAfter, matrix.Item1);
				var resultingArray = _teamBlockDaysOffMoveFinder.TryFindMoves(matrix.Item1, originalArray, optimizationPreferences, dayOffOptimizationPreference, schedulingResultStateHolder);

				var movedDaysOff = _affectedDayOffs.Execute(matrix.Item1, dayOffOptimizationPreference, originalArray, resultingArray);
				if (movedDaysOff != null)
				{
					var predictorResult = _dayOffOptimizerPreMoveResultPredictor.IsPredictedBetterThanCurrent(matrix.Item1, resultingArray, originalArray, dayOffOptimizationPreference, 
						numberOfDayOffsMoved, optimizationPreferences, schedulingResultStateHolder, movedDaysOff);
					if (predictorResult.IsDefinatlyWorse)
					{
						stopOptimizeTeamInfo[matrix.Item2] = false;
						matrix.Item2.LockDays(movedDaysOff.ModifiedDays());
						callback.Optimizing(new OptimizationCallbackInfo(matrix.Item2, false, matrixes.Count));
						continue;
					}
					var resCalcState = new UndoRedoContainer();
					resCalcState.FillWith(schedulingResultStateHolder.SkillDaysOnDateOnly(movedDaysOff.ModifiedDays()));
					var result = runOneMatrixOnly(optimizationPreferences, rollbackService, matrix.Item1, schedulingOptions, matrix.Item2,
						resourceCalculateDelayer,
						schedulingResultStateHolder,
						movedDaysOff,
						dayOffOptimizationPreferenceProvider, predictorResult);
					
					if (result.Better)
					{
						stopOptimizeTeamInfo[matrix.Item2] = false;
					}
					else
					{
						resCalcState.UndoAll();
						rollbackService.RollbackMinimumChecks();

						if (result.MinimumAgentsAreCurrentlyBroken)
						{
							stopOptimizeTeamInfo[matrix.Item2] = false;
							matrix.Item2.LockDays(result.FailedWhenPlacingShift ?
								movedDaysOff.RemovedDaysOff : 
								movedDaysOff.AddedDaysOff);
						}
						else
						{
							if (!optimizationPreferences.Extra.IsClassic()) //removing this if makes bookingdb lot slower...
							{
								stopOptimizeTeamInfo[matrix.Item2] = false;
							}

							matrix.Item2.LockDays(movedDaysOff.ModifiedDays());
						}
					}

					callback.Optimizing(new OptimizationCallbackInfo(matrix.Item2, result.Better, matrixes.Count));
					
					if (onReportProgress(schedulingProgress, matrixes.Count, currentMatrixCounter, matrix.Item2, optimizationPreferences.Advanced.RefreshScreenInterval))
					{
						cancelAction();
						return null;
					}
				}
			}

			return from allFailedKeyValue in stopOptimizeTeamInfo 
				where allFailedKeyValue.Value
				select allFailedKeyValue.Key;
		}

		

		private WasReallyBetterResult runOneMatrixOnly(IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService rollbackService, IScheduleMatrixPro matrix,
			SchedulingOptions schedulingOptions, ITeamInfo teamInfo,
			IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			MovedDaysOff movedDaysOff,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			PredictorResult prevPredictorResult)
		{
			if (optimizationPreferences.Extra.UseTeams && optimizationPreferences.Extra.UseTeamSameDaysOff)
			{
				movedDaysOff.RemovedDaysOff.ForEach(date => _teamDayOffModifier.RemoveDayOffForTeam(rollbackService, teamInfo, date));
				movedDaysOff.AddedDaysOff.ForEach(date => _teamDayOffModifier.AddDayOffForTeamAndResourceCalculate(rollbackService, teamInfo, date, schedulingOptions.DayOffTemplate));
			}
			else
			{
				movedDaysOff.RemovedDaysOff.ForEach(date => _teamDayOffModifier.RemoveDayOffForMember(rollbackService, matrix.Person, date));
				movedDaysOff.AddedDaysOff.ForEach(date => _teamDayOffModifier.AddDayOffForMember(rollbackService, matrix.Person, date, schedulingOptions.DayOffTemplate, true));
			}

			var personToSetShiftCategoryLimitationFor = optimizationPreferences.Extra.IsClassic() ? matrix.Person : null;
			if (!reScheduleAllMovedDaysOff(schedulingOptions, teamInfo, movedDaysOff.RemovedDaysOff, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, personToSetShiftCategoryLimitationFor, matrix, optimizationPreferences))
			{
				//first "true" flag is actually wrong
				//for perf reason 
				// => true when minagent is broken
				// => if not, it should be false
				return WasReallyBetterResult.WasWorse(true, true);
			}

			if (!optimizationPreferences.General.OptimizationStepDaysOff && optimizationPreferences.General.OptimizationStepDaysOffForFlexibleWorkTime)
			{
				var flexibleDayOffvalidator = _teamBlockDayOffsInPeriodValidator;
				if (!flexibleDayOffvalidator.Validate(teamInfo, schedulingResultStateHolder))
				{
					return WasReallyBetterResult.WasWorse(false, false);
				}
			}

			if (!_teamBlockOptimizationLimits.ValidateMinWorkTimePerWeek(teamInfo))
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				teamInfo.LockDays(movedDaysOff.AddedDaysOff);
				return WasReallyBetterResult.WasBetter();
			}

			if (!_teamBlockOptimizationLimits.Validate(teamInfo, optimizationPreferences, dayOffOptimizationPreferenceProvider))
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				teamInfo.LockDays(movedDaysOff.AddedDaysOff);
				return WasReallyBetterResult.WasBetter();
			}

			if (!optimizationPreferences.Extra.IsClassic())
			{
				foreach (var dateOnly in movedDaysOff.RemovedDaysOff)
				{
					ITeamBlockInfo teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly, schedulingOptions.BlockFinder());

					if (!_teamBlockShiftCategoryLimitationValidator.Validate(teamBlockInfo, null, optimizationPreferences))
					{
						_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
						teamInfo.LockDays(movedDaysOff.AddedDaysOff);
						teamInfo.LockDays(movedDaysOff.RemovedDaysOff);
					}
				}
			}

			return _dayOffOptimizerPreMoveResultPredictor.WasReallyBetter(matrix, optimizationPreferences,
				schedulingResultStateHolder, movedDaysOff, prevPredictorResult);
		}

		private static bool onReportProgress(ISchedulingProgress schedulingProgress, int totalNumberOfTeamInfos, int teamInfoCounter, ITeamInfo currentTeamInfo, int screenRefreshRate)
		{
			if (schedulingProgress.CancellationPending)
			{
				return true;
			}
			var eventArgs = new ResourceOptimizerProgressEventArgs(0, 0,
				Resources.OptimizingDaysOff + Resources.Colon + "(" + totalNumberOfTeamInfos.ToString("####") + ")(" +
				teamInfoCounter.ToString("####") + ") " + currentTeamInfo.Name.DisplayString(20), screenRefreshRate);
			schedulingProgress.ReportProgress(1, eventArgs);
			return false;
		}

		private bool reScheduleAllMovedDaysOff(SchedulingOptions schedulingOptions, ITeamInfo teamInfo,
			IEnumerable<DateOnly> removedDaysOff,
			ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IPerson personToSetShiftCategoryLimitationFor,
			IScheduleMatrixPro matrix,
			IOptimizationPreferences optimizationPreferences)
		{
			var nightRestWhiteSpotSolver = _nightRestWhiteSpotSolverServiceFactory.Create(true); //keep some behavoir as in classic
			foreach (DateOnly dateOnly in removedDaysOff)
			{
				if (personToSetShiftCategoryLimitationFor != null)
				{
					_shiftCategoryLimitationChecker.SetBlockedShiftCategories(schedulingOptions, personToSetShiftCategoryLimitationFor, dateOnly);
				}
				var teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly, schedulingOptions.BlockFinder());
				if (teamBlockInfo == null)
					continue;
				if (!_teamBlockSteadyStateValidator.IsTeamBlockInSteadyState(teamBlockInfo, schedulingOptions))
					_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
				var resCalcData =new ResourceCalculationData(schedulingResultStateHolder, schedulingOptions.ConsiderShortBreaks, false);
				if (!_teamBlockScheduler.ScheduleTeamBlockDay(Enumerable.Empty<IPersonAssignment>(), new NoSchedulingCallback(), _workShiftSelector, teamBlockInfo, dateOnly, schedulingOptions,
					rollbackService, resourceCalculateDelayer, schedulingResultStateHolder.SkillDays,
					schedulingResultStateHolder.Schedules, resCalcData, new ShiftNudgeDirective(),
					NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder), _groupPersonSkillAggregator))
				{
					if (optimizationPreferences.Extra.IsClassic())
					{
						if (!nightRestWhiteSpotSolver.Resolve(matrix, schedulingOptions, rollbackService))
							return false;
					}
					else
					{
						return false;
					}
				}
			}

			return true;
		}
	}
}