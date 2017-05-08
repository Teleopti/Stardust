using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class DayOffOptimizerStandard : IDayOffOptimizerUseTeamSameDaysOff
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
		private readonly DayOffOptimizerPreMoveResultPredictor _dayOffOptimizerPreMoveResultPredictor;
		private readonly ITeamBlockDaysOffMoveFinder _teamBlockDaysOffMoveFinder;
		private readonly AffectedDayOffs _affectedDayOffs;
		private readonly IShiftCategoryLimitationChecker _shiftCategoryLimitationChecker;
		private readonly INightRestWhiteSpotSolverServiceFactory _nightRestWhiteSpotSolverServiceFactory;

		public DayOffOptimizerStandard(
			ILockableBitArrayFactory lockableBitArrayFactory,
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
			IGroupPersonSkillAggregator groupPersonSkillAggregator,
			DayOffOptimizerPreMoveResultPredictor dayOffOptimizerPreMoveResultPredictor,
			ITeamBlockDaysOffMoveFinder teamBlockDaysOffMoveFinder,
			AffectedDayOffs affectedDayOffs,
			IShiftCategoryLimitationChecker shiftCategoryLimitationChecker,
			INightRestWhiteSpotSolverServiceFactory nightRestWhiteSpotSolverServiceFactory)
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
		}

		[RemoveMeWithToggle("Maybe (?) remove IPeriodValueCalculator param", Toggles.ResourcePlanner_TeamBlockDayOffForIndividuals_37998)]
		public IEnumerable<ITeamInfo> Execute(IPeriodValueCalculator periodValueCalculatorForAllSkills, IOptimizationPreferences optimizationPreferences, ISchedulePartModifyAndRollbackService rollbackService,
			IEnumerable<ITeamInfo> remainingInfoList, SchedulingOptions schedulingOptions, IEnumerable<IPerson> selectedPersons,
			IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder,
			Action cancelAction,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, ISchedulingProgress schedulingProgress)
		{
			if (optimizationPreferences.Extra.IsClassic())
			{
				periodValueCalculatorForAllSkills = new noExpansivePeriodValueCalculation();
			}
			var previousPeriodValue = periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
			var currentMatrixCounter = 0;
			var allFailed = new Dictionary<ITeamInfo, bool>();
			var matrixes = new List<Tuple<IScheduleMatrixPro, ITeamInfo>>();
			foreach (var teamInfo in remainingInfoList)
			{
				allFailed[teamInfo] = true;
				matrixes.AddRange(teamInfo.MatrixesForGroup().Select(scheduleMatrixPro => new Tuple<IScheduleMatrixPro, ITeamInfo>(scheduleMatrixPro, teamInfo)));
			}

			foreach (var matrix in matrixes.Randomize())
			{
				currentMatrixCounter++;

				if (!(optimizationPreferences.Extra.UseTeamBlockOption && optimizationPreferences.Extra.UseTeamSameDaysOff))
				{
					if (!selectedPersons.Contains(matrix.Item1.Person))
						continue;
				}
				rollbackService.ClearModificationCollection();
				var dayOffOptimizationPreference = dayOffOptimizationPreferenceProvider.ForAgent(matrix.Item1.Person, matrix.Item1.EffectivePeriodDays.First().Day);

				var originalArray = _lockableBitArrayFactory.ConvertFromMatrix(dayOffOptimizationPreference.ConsiderWeekBefore, dayOffOptimizationPreference.ConsiderWeekAfter, matrix.Item1);
				var resultingArray = _teamBlockDaysOffMoveFinder.TryFindMoves(matrix.Item1, originalArray, optimizationPreferences, dayOffOptimizationPreference, schedulingResultStateHolder);

				var movedDaysOff = _affectedDayOffs.Execute(matrix.Item1, dayOffOptimizationPreference, originalArray, resultingArray);
				if (movedDaysOff != null)
				{
					if (!optimizationPreferences.Advanced.UseTweakedValues && optimizationPreferences.Extra.IsClassic())
					{
						var predictorResult = _dayOffOptimizerPreMoveResultPredictor.IsPredictedBetterThanCurrent(matrix.Item1, resultingArray, originalArray, dayOffOptimizationPreference);
						previousPeriodValue = predictorResult.CurrentValue;
						if (!predictorResult.IsBetter)
						{
							allFailed[matrix.Item2] = false;
							matrix.Item2.LockDays(movedDaysOff.AddedDaysOff);
							matrix.Item2.LockDays(movedDaysOff.RemovedDaysOff);
							continue;
						}
					}

					var currentPeriodValue = optimizationPreferences.Extra.IsClassic() ? 
						new Lazy<double>(() => _dayOffOptimizerPreMoveResultPredictor.CurrentValue(matrix.Item1)) : 
						new Lazy<double>(() => periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization));
					var resCalcState = new UndoRedoContainer();
					resCalcState.FillWith(schedulingResultStateHolder.SkillDaysOnDateOnly(movedDaysOff.ModifiedDays()));
					var success = runOneMatrixOnly(optimizationPreferences, rollbackService, matrix.Item1, schedulingOptions, matrix.Item2,
						resourceCalculateDelayer,
						schedulingResultStateHolder,
						currentPeriodValue,
						previousPeriodValue,
						movedDaysOff);

					if (success)
					{
						previousPeriodValue = currentPeriodValue.Value;
						allFailed[matrix.Item2] = false;
					}
					else
					{
						resCalcState.UndoAll();
						rollbackService.RollbackMinimumChecks();

						if (!optimizationPreferences.Advanced.UseTweakedValues)
						{
							if (!optimizationPreferences.Extra.IsClassic()) //removing this if makes bookingdb lot slower...
							{
								allFailed[matrix.Item2] = false;
							}
							matrix.Item2.LockDays(movedDaysOff.AddedDaysOff);
							matrix.Item2.LockDays(movedDaysOff.RemovedDaysOff);
						}
					}

					if (onReportProgress(schedulingProgress, matrixes.Count, currentMatrixCounter, matrix.Item2, previousPeriodValue, optimizationPreferences.Advanced.RefreshScreenInterval))
					{
						cancelAction();
						return null;
					}
				}
			}

			return from allFailedKeyValue in allFailed
				where allFailedKeyValue.Value
				select allFailedKeyValue.Key;
		}

		private bool runOneMatrixOnly(IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService rollbackService, IScheduleMatrixPro matrix,
			SchedulingOptions schedulingOptions, ITeamInfo teamInfo,
			IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			Lazy<double> currentPeriodValue, double previousPeriodValue,
			MovedDaysOff movedDaysOff)
		{
			removeAllDecidedDaysOffForMember(rollbackService, movedDaysOff.RemovedDaysOff, matrix.Person);
			addAllDecidedDaysOffForMember(rollbackService, schedulingOptions, movedDaysOff.AddedDaysOff, matrix.Person);

			var personToSetShiftCategoryLimitationFor = optimizationPreferences.Extra.IsClassic() ? matrix.Person : null;
			if (!reScheduleAllMovedDaysOff(schedulingOptions, teamInfo, movedDaysOff.RemovedDaysOff, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, personToSetShiftCategoryLimitationFor, matrix, optimizationPreferences))
			{
				return false;
			}

			if (!optimizationPreferences.General.OptimizationStepDaysOff && optimizationPreferences.General.OptimizationStepDaysOffForFlexibleWorkTime)
			{
				var flexibleDayOffvalidator = _teamBlockDayOffsInPeriodValidator;
				if (!flexibleDayOffvalidator.Validate(teamInfo, schedulingResultStateHolder))
				{
					teamInfo.LockDays(movedDaysOff.AddedDaysOff);
					teamInfo.LockDays(movedDaysOff.RemovedDaysOff);
					return false;
				}
			}

			if (!_teamBlockOptimizationLimits.ValidateMinWorkTimePerWeek(teamInfo))
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				teamInfo.LockDays(movedDaysOff.AddedDaysOff);
				return true;
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

			return currentPeriodValue.Value < previousPeriodValue;
		}

		private void addAllDecidedDaysOffForMember(ISchedulePartModifyAndRollbackService rollbackService, SchedulingOptions schedulingOptions, IEnumerable<DateOnly> addedDaysOff, IPerson person)
		{
			foreach (var dateOnly in addedDaysOff)
			{
				_teamDayOffModifier.AddDayOffForMember(rollbackService, person, dateOnly, schedulingOptions.DayOffTemplate, true);
			}
		}

		private void removeAllDecidedDaysOffForMember(ISchedulePartModifyAndRollbackService rollbackService, IEnumerable<DateOnly> removedDaysOff, IPerson person)
		{
			foreach (var dateOnly in removedDaysOff)
			{
				_teamDayOffModifier.RemoveDayOffForMember(rollbackService, person, dateOnly);
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

				if (!_teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelector, teamBlockInfo, dateOnly, schedulingOptions,
					rollbackService, resourceCalculateDelayer, schedulingResultStateHolder.AllSkillDays(),
					schedulingResultStateHolder.Schedules, new ShiftNudgeDirective(),
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


		[RemoveMeWithToggle(Toggles.ResourcePlanner_TeamBlockDayOffForIndividuals_37998)]
		private class noExpansivePeriodValueCalculation : IPeriodValueCalculator
		{
			public double PeriodValue(IterationOperationOption iterationOperationOption)
			{
				return 0;
			}
		}
	}
}