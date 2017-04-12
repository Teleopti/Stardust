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
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class TeamBlockDayOffOptimizerService
	{
		private readonly ILockableBitArrayFactory _lockableBitArrayFactory;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly ITeamDayOffModifier _teamDayOffModifier;
		private readonly ITeamBlockSteadyStateValidator _teamTeamBlockSteadyStateValidator;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockOptimizationLimits _teamBlockOptimizationLimits;
		private readonly ITeamBlockMaxSeatChecker _teamBlockMaxSeatChecker;
		private readonly IAllTeamMembersInSelectionSpecification _allTeamMembersInSelectionSpecification;
		private readonly ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		private readonly ITeamBlockDayOffsInPeriodValidator _teamBlockDayOffsInPeriodValidator;
		private readonly TeamBlockDaysOffSameDaysOffLockSyncronizer _teamBlockDaysOffSameDaysOffLockSyncronizer;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IOptimizerHelperHelper _optimizerHelper;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;
		private readonly DayOffOptimizerPreMoveResultPredictor _dayOffOptimizerPreMoveResultPredictor;
		private readonly ITeamBlockDaysOffMoveFinder _teamBlockDaysOffMoveFinder;
		private readonly AffectedDayOffs _affectedDayOffs;
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;

		public TeamBlockDayOffOptimizerService(
			ILockableBitArrayFactory lockableBitArrayFactory,
			ITeamBlockScheduler teamBlockScheduler,
			ITeamBlockInfoFactory teamBlockInfoFactory,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			ITeamDayOffModifier teamDayOffModifier,
			ITeamBlockSteadyStateValidator teamBlockSteadyStateValidator,
			ITeamBlockClearer teamBlockClearer,
			ITeamBlockOptimizationLimits teamBlockOptimizationLimits,
			ITeamBlockMaxSeatChecker teamBlockMaxSeatChecker,
			IAllTeamMembersInSelectionSpecification allTeamMembersInSelectionSpecification,
			ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator,
			ITeamBlockDayOffsInPeriodValidator teamBlockDayOffsInPeriodValidator,
			TeamBlockDaysOffSameDaysOffLockSyncronizer teamBlockDaysOffSameDaysOffLockSyncronizer,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IOptimizerHelperHelper optimizerHelper,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			IWorkShiftSelector workShiftSelector,
			IGroupPersonSkillAggregator groupPersonSkillAggregator,
			DayOffOptimizerPreMoveResultPredictor dayOffOptimizerPreMoveResultPredictor,
			ITeamBlockDaysOffMoveFinder teamBlockDaysOffMoveFinder,
			AffectedDayOffs affectedDayOffs,
			IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider)
		{
			_lockableBitArrayFactory = lockableBitArrayFactory;
			_teamBlockScheduler = teamBlockScheduler;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_teamDayOffModifier = teamDayOffModifier;
			_teamTeamBlockSteadyStateValidator = teamBlockSteadyStateValidator;
			_teamBlockClearer = teamBlockClearer;
			_teamBlockOptimizationLimits = teamBlockOptimizationLimits;
			_teamBlockMaxSeatChecker = teamBlockMaxSeatChecker;
			_allTeamMembersInSelectionSpecification = allTeamMembersInSelectionSpecification;
			_teamBlockShiftCategoryLimitationValidator = teamBlockShiftCategoryLimitationValidator;
			_teamBlockDayOffsInPeriodValidator = teamBlockDayOffsInPeriodValidator;
			_teamBlockDaysOffSameDaysOffLockSyncronizer = teamBlockDaysOffSameDaysOffLockSyncronizer;
			_schedulerStateHolder = schedulerStateHolder;
			_optimizerHelper = optimizerHelper;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_workShiftSelector = workShiftSelector;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
			_dayOffOptimizerPreMoveResultPredictor = dayOffOptimizerPreMoveResultPredictor;
			_teamBlockDaysOffMoveFinder = teamBlockDaysOffMoveFinder;
			_affectedDayOffs = affectedDayOffs;
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
		}

		public void OptimizeDaysOff(
			IList<IScheduleMatrixPro> allPersonMatrixList,
			DateOnlyPeriod selectedPeriod,
			IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			ISchedulingOptions schedulingOptions,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			ITeamInfoFactory teamInfoFactory,
			ISchedulingProgress schedulingProgress
			)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var tagSetter = new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling);
			var rollbackService = new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState,
				_scheduleDayChangeCallback,
				tagSetter);
			schedulingOptions.DayOffTemplate = schedulerStateHolder.CommonStateHolder.DefaultDayOffTemplate;

			
			var skillsDataExtractor = _optimizerHelper.CreateTeamBlockAllSkillsDataExtractor(optimizationPreferences.Advanced, selectedPeriod, schedulerStateHolder.SchedulingResultState, allPersonMatrixList);
			var periodValueCalculatorForAllSkills = _optimizerHelper.CreatePeriodValueCalculator(optimizationPreferences.Advanced, skillsDataExtractor);

			// create a list of all teamInfos
			var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			foreach (var selectedPerson in selectedPersons)
			{
				var teamInfo = teamInfoFactory.CreateTeamInfo(schedulerStateHolder.SchedulingResultState.PersonsInOrganization, selectedPerson, selectedPeriod, allPersonMatrixList);
				if(optimizationPreferences.Extra.UseTeamBlockOption && optimizationPreferences.Extra.UseTeamSameDaysOff  )
				{
					if (!_allTeamMembersInSelectionSpecification.IsSatifyBy(teamInfo, selectedPersons))
						continue;
				}
				if (teamInfo != null )
					allTeamInfoListOnStartDate.Add(teamInfo);
			}

			foreach (var teamInfo in allTeamInfoListOnStartDate)
			{
				foreach (var groupMember in teamInfo.GroupMembers)
				{
					if(!selectedPersons.Contains(groupMember))
						teamInfo.LockMember(selectedPeriod, groupMember);
				}
			}

			_teamBlockDaysOffSameDaysOffLockSyncronizer.SyncLocks(selectedPeriod, optimizationPreferences, allTeamInfoListOnStartDate);

			var remainingInfoList = new List<ITeamInfo>(allTeamInfoListOnStartDate.Where(x => x.GroupMembers.Any()));

			var cancelMe = false;
			while (remainingInfoList.Count > 0)
			{
				IEnumerable<ITeamInfo> teamInfosToRemove;
				if(optimizationPreferences.Extra.UseTeams && optimizationPreferences.Extra.UseTeamSameDaysOff)
				{
					teamInfosToRemove = runOneOptimizationRound(periodValueCalculatorForAllSkills, optimizationPreferences, rollbackService,
					                                            remainingInfoList, schedulingOptions,
					                                            resourceCalculateDelayer, schedulerStateHolder.SchedulingResultState, ()=>
					                                            {
						                                            cancelMe = true;
					                                            },
																dayOffOptimizationPreferenceProvider, schedulingProgress);
				}
				else
				{
					if (optimizationPreferences.Extra.IsClassic())
					{
						periodValueCalculatorForAllSkills = new noExpansivePeriodValueCalculation();
					}
					teamInfosToRemove = runOneOptimizationRoundWithFreeDaysOff(periodValueCalculatorForAllSkills, optimizationPreferences, rollbackService,
					                                                           remainingInfoList, schedulingOptions,
					                                                           selectedPersons,
																			   resourceCalculateDelayer, schedulerStateHolder.SchedulingResultState, () =>
																			   {
																				   cancelMe = true;
																			   },
																			   dayOffOptimizationPreferenceProvider, schedulingProgress);
				}

				if (cancelMe)
					break;

				foreach (var teamInfo in teamInfosToRemove)
				{
					remainingInfoList.Remove(teamInfo);
				}
			}
		}

		[RemoveMeWithToggle(Toggles.ResourcePlanner_TeamBlockDayOffForIndividuals_37998)]
		private class noExpansivePeriodValueCalculation : IPeriodValueCalculator
		{
			public double PeriodValue(IterationOperationOption iterationOperationOption)
			{
				return 0;
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

		private IEnumerable<ITeamInfo> runOneOptimizationRoundWithFreeDaysOff(IPeriodValueCalculator periodValueCalculatorForAllSkills, IOptimizationPreferences optimizationPreferences, ISchedulePartModifyAndRollbackService rollbackService, 
																			List<ITeamInfo> remainingInfoList, ISchedulingOptions schedulingOptions, IList<IPerson> selectedPersons, 
																			IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder, 
																			Action cancelAction,
																			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, ISchedulingProgress schedulingProgress)
		{
			var previousPeriodValue = periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
			var currentMatrixCounter = 0;
			var allFailed = new Dictionary<ITeamInfo, bool>();
			var matrixes = new List<Tuple<IScheduleMatrixPro, ITeamInfo>>();
			foreach (var teamInfo in remainingInfoList.Randomize()) //this randomize could be removed if we could randomize no matter IsClassic below...
			{
				allFailed[teamInfo] = true;
				matrixes.AddRange(teamInfo.MatrixesForGroup().Select(scheduleMatrixPro => new Tuple<IScheduleMatrixPro, ITeamInfo>(scheduleMatrixPro, teamInfo)));
			}

			foreach (var matrix in optimizationPreferences.Extra.IsClassic() ? matrixes.Randomize() : matrixes)
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
				var resultingArray = _teamBlockDaysOffMoveFinder.TryFindMoves(matrix.Item1, originalArray, optimizationPreferences, dayOffOptimizationPreference, _schedulerStateHolder().SchedulingResultState);

				var movedDaysOff = _affectedDayOffs.Execute(matrix.Item1, dayOffOptimizationPreference, originalArray, resultingArray);
				if (movedDaysOff != null)
				{
					if (!optimizationPreferences.Advanced.UseTweakedValues &&
							optimizationPreferences.Extra.IsClassic() &&
							!_dayOffOptimizerPreMoveResultPredictor.IsPredictedBetterThanCurrent(matrix.Item1, resultingArray, originalArray, dayOffOptimizationPreference).IsBetter)
					{
						allFailed[matrix.Item2] = false;
						lockDaysInMatrixes(movedDaysOff.AddedDaysOff, matrix.Item2);
						lockDaysInMatrixes(movedDaysOff.RemovedDaysOff, matrix.Item2);
					}
					else
					{
						var resCalcState = new UndoRedoContainer();
						var currentPeriodValue = new Lazy<double>(() => periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization));
						if (optimizationPreferences.Extra.IsClassic())
						{
							//TODO: hack -> always do this
							resCalcState.FillWith(_schedulerStateHolder().SchedulingResultState.SkillDaysOnDateOnly(movedDaysOff.ModifiedDays()));

							var personalSkillsDataExtractor = _scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(matrix.Item1, optimizationPreferences.Advanced, schedulingResultStateHolder);
							var localPeriodValueCalculator = _optimizerHelper.CreatePeriodValueCalculator(optimizationPreferences.Advanced, personalSkillsDataExtractor);
							currentPeriodValue = new Lazy<double>(() => localPeriodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization));
							previousPeriodValue = localPeriodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization);
						}
						var success = runOneMatrixOnly(optimizationPreferences, rollbackService, matrix.Item1, schedulingOptions, matrix.Item2,
							resourceCalculateDelayer,
							schedulingResultStateHolder,
							dayOffOptimizationPreferenceProvider,
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
							if (optimizationPreferences.Extra.IsClassic())
							{
								//TODO: hack -> always do this
								resCalcState.UndoAll();
								rollbackService.RollbackMinimumChecks();
							}
							else
							{
								_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
							}

							if (!optimizationPreferences.Advanced.UseTweakedValues)
							{
								allFailed[matrix.Item2] = false;
								lockDaysInMatrixes(movedDaysOff.AddedDaysOff, matrix.Item2);

								//TEST
								if (!optimizationPreferences.Extra.IsClassic())
								{
									lockDaysInMatrixes(movedDaysOff.RemovedDaysOff, matrix.Item2);
								}
							}
						}

						if (onReportProgress(schedulingProgress, matrixes.Count, currentMatrixCounter, matrix.Item2, previousPeriodValue, optimizationPreferences.Advanced.RefreshScreenInterval))
						{
							cancelAction();
							return null;
						}
					}
				}
			}

			return from allFailedKeyValue in allFailed
				where allFailedKeyValue.Value
				select allFailedKeyValue.Key;
		}
			
		private IEnumerable<ITeamInfo> runOneOptimizationRound(IPeriodValueCalculator periodValueCalculatorForAllSkills, IOptimizationPreferences optimizationPreferences, ISchedulePartModifyAndRollbackService rollbackService, 
																List<ITeamInfo> remainingInfoList, ISchedulingOptions schedulingOptions, IResourceCalculateDelayer resourceCalculateDelayer, 
																ISchedulingResultStateHolder schedulingResultStateHolder, Action cancelAction, 
																IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, ISchedulingProgress schedulingProgress)
		{
			var teamInfosToRemove = new HashSet<ITeamInfo>();
			double previousPeriodValue =
					periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
			var totalLiveTeamInfos = remainingInfoList.Count;
			var currentTeamInfoCounter = 0;
			var cancelMe = false;
			foreach (ITeamInfo teamInfo in remainingInfoList.GetRandom(remainingInfoList.Count, true))
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
												dayOffOptimizationPreferenceProvider,
												out checkPeriodValue);
					var currentPeriodValue = new Lazy<double>(()=> periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization));
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

		private bool handleResult(ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions,
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

		[RemoveMeWithToggle("maxseat check can be removed with toggle", Toggles.ResourcePlanner_MaxSeatsNew_40939)]
		private bool runOneMatrixOnly(IOptimizationPreferences optimizationPreferences,
										ISchedulePartModifyAndRollbackService rollbackService, IScheduleMatrixPro matrix,
										ISchedulingOptions schedulingOptions, ITeamInfo teamInfo, 
										IResourceCalculateDelayer resourceCalculateDelayer,
										ISchedulingResultStateHolder schedulingResultStateHolder,
										IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
										Lazy<double> currentPeriodValue, double previousPeriodValue,
										MovedDaysOff movedDaysOff)
		{
			removeAllDecidedDaysOffForMember(rollbackService, movedDaysOff.RemovedDaysOff, matrix.Person);
			addAllDecidedDaysOffForMember(rollbackService, schedulingOptions, movedDaysOff.AddedDaysOff, matrix.Person);
			
			if (!reScheduleAllMovedDaysOff(schedulingOptions, teamInfo, movedDaysOff.RemovedDaysOff, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder))
			{
				return false;
			}

			if (!optimizationPreferences.General.OptimizationStepDaysOff && optimizationPreferences.General.OptimizationStepDaysOffForFlexibleWorkTime)
			{
				var flexibleDayOffvalidator = _teamBlockDayOffsInPeriodValidator;
				if (!flexibleDayOffvalidator.Validate(teamInfo, schedulingResultStateHolder))
				{
					_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
					lockDaysInMatrixes(movedDaysOff.AddedDaysOff, teamInfo);
					lockDaysInMatrixes(movedDaysOff.RemovedDaysOff, teamInfo);
					return false;
				}
			}

			var isMaxSeatRuleViolated =
				movedDaysOff.AddedDaysOff.Any(x => !_teamBlockMaxSeatChecker.CheckMaxSeat(x, schedulingOptions, teamInfo, schedulingResultStateHolder.SkillDays)) ||
				movedDaysOff.RemovedDaysOff.Any(x => !_teamBlockMaxSeatChecker.CheckMaxSeat(x, schedulingOptions, teamInfo, schedulingResultStateHolder.SkillDays));
			
			if (isMaxSeatRuleViolated || !_teamBlockOptimizationLimits.Validate(teamInfo, optimizationPreferences, dayOffOptimizationPreferenceProvider))
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				lockDaysInMatrixes(movedDaysOff.AddedDaysOff, teamInfo);
				lockDaysInMatrixes(movedDaysOff.RemovedDaysOff, teamInfo);

				return checkPeriodValue(currentPeriodValue, previousPeriodValue);
			}

			if (!_teamBlockOptimizationLimits.ValidateMinWorkTimePerWeek(teamInfo))
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				lockDaysInMatrixes(movedDaysOff.AddedDaysOff, teamInfo);
				return true;
			}

			foreach (var dateOnly in movedDaysOff.RemovedDaysOff)
			{
				ITeamBlockInfo teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly, schedulingOptions.BlockFinder());

				if (!_teamBlockShiftCategoryLimitationValidator.Validate(teamBlockInfo, null, optimizationPreferences))
				{
					_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
					lockDaysInMatrixes(movedDaysOff.AddedDaysOff, teamInfo);
					lockDaysInMatrixes(movedDaysOff.RemovedDaysOff, teamInfo);
				}
			}

			if (optimizationPreferences.Extra.IsClassic())
			{
				//TODO - investigate...
				// * run also when normal team/block?
				// * lock also added DO days?
				lockDaysInMatrixes(movedDaysOff.RemovedDaysOff, teamInfo);
			}

			return checkPeriodValue(currentPeriodValue, previousPeriodValue);
		}

		private static bool checkPeriodValue(Lazy<double> currentPeriodValue, double previousPeriodValue)
		{
			return currentPeriodValue.Value < previousPeriodValue;
		}

		private void addAllDecidedDaysOffForMember(ISchedulePartModifyAndRollbackService rollbackService,
		                                           ISchedulingOptions schedulingOptions, IEnumerable<DateOnly> addedDaysOff, IPerson person)
		{
			foreach (DateOnly dateOnly in addedDaysOff)
			{
				_teamDayOffModifier.AddDayOffForMember(rollbackService, person, dateOnly, schedulingOptions.DayOffTemplate, true);
			}
		}

		private void removeAllDecidedDaysOffForMember(ISchedulePartModifyAndRollbackService rollbackService,
		                                              IEnumerable<DateOnly> removedDaysOff, IPerson person)
		{
			foreach (DateOnly dateOnly in removedDaysOff)
			{
				_teamDayOffModifier.RemoveDayOffForMember(rollbackService, person, dateOnly);
			}
		}

		[RemoveMeWithToggle("maxseat check can be removed with toggle", Toggles.ResourcePlanner_MaxSeatsNew_40939)]
		private bool runOneTeam(IOptimizationPreferences optimizationPreferences,
		                        ISchedulePartModifyAndRollbackService rollbackService,
		                        ISchedulingOptions schedulingOptions, IScheduleMatrixPro matrix,
		                        ITeamInfo teamInfo,
								IResourceCalculateDelayer resourceCalculateDelayer,
								ISchedulingResultStateHolder schedulingResultStateHolder,
								IDaysOffPreferences daysOffPreferences,
								IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
								out bool checkPeriodValue)
		{
			var originalArray = _lockableBitArrayFactory.ConvertFromMatrix(daysOffPreferences.ConsiderWeekBefore, daysOffPreferences.ConsiderWeekAfter, matrix);
			var resultingArray = _teamBlockDaysOffMoveFinder.TryFindMoves(matrix, originalArray, optimizationPreferences, daysOffPreferences, _schedulerStateHolder().SchedulingResultState);

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
					lockDaysInMatrixes(movedDaysOff.AddedDaysOff, teamInfo);
					lockDaysInMatrixes(movedDaysOff.RemovedDaysOff, teamInfo);
					checkPeriodValue = true;
					return false;
				}
			}

			var isMaxSeatRuleViolated = movedDaysOff.AddedDaysOff.Any(x => !_teamBlockMaxSeatChecker.CheckMaxSeat(x, schedulingOptions, teamInfo, schedulingResultStateHolder.SkillDays)) ||
										movedDaysOff.AddedDaysOff.Any(x => !_teamBlockMaxSeatChecker.CheckMaxSeat(x.AddDays(1), schedulingOptions, teamInfo, schedulingResultStateHolder.SkillDays));
			
			if (isMaxSeatRuleViolated || !_teamBlockOptimizationLimits.Validate(teamInfo, optimizationPreferences, dayOffOptimizationPreferenceProvider))
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				lockDaysInMatrixes(movedDaysOff.AddedDaysOff, teamInfo);
				lockDaysInMatrixes(movedDaysOff.RemovedDaysOff, teamInfo);
				checkPeriodValue = true;
				return true;
			}

			if (!_teamBlockOptimizationLimits.ValidateMinWorkTimePerWeek(teamInfo))
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				lockDaysInMatrixes(movedDaysOff.AddedDaysOff, teamInfo);
				checkPeriodValue = false;
				return true;
			}

			foreach (var dateOnly in movedDaysOff.RemovedDaysOff)
			{
				var teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly, schedulingOptions.BlockFinder());

				if (!_teamBlockShiftCategoryLimitationValidator.Validate(teamBlockInfo, null, optimizationPreferences))
				{
					_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
					lockDaysInMatrixes(movedDaysOff.AddedDaysOff, teamInfo);
					lockDaysInMatrixes(movedDaysOff.RemovedDaysOff, teamInfo);
				}
			}

			checkPeriodValue = true;
			return true;
		}


		private static void lockDaysInMatrixes(IEnumerable<DateOnly> datesToLock , ITeamInfo teamInfo)
		{
			foreach (var dateOnly in datesToLock)
			{
				foreach (var scheduleMatrixPro in teamInfo.MatrixesForGroupAndDate(dateOnly))
				{
					scheduleMatrixPro.LockDay(dateOnly);
				}
			}
		}

		private bool reScheduleAllMovedDaysOff(ISchedulingOptions schedulingOptions, ITeamInfo teamInfo,
		                                       IEnumerable<DateOnly> removedDaysOff,
		                                       ISchedulePartModifyAndRollbackService rollbackService,
		                                       IResourceCalculateDelayer resourceCalculateDelayer,
												ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			foreach (DateOnly dateOnly in removedDaysOff)
			{
				ITeamBlockInfo teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly,
																																								 schedulingOptions.BlockFinder());
				if (teamBlockInfo == null)
					continue;
				if (!_teamTeamBlockSteadyStateValidator.IsTeamBlockInSteadyState(teamBlockInfo, schedulingOptions))
					_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);

				if (!_teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelector, teamBlockInfo, dateOnly, schedulingOptions, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder.AllSkillDays(), schedulingResultStateHolder.Schedules, new ShiftNudgeDirective(), NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder), _groupPersonSkillAggregator))
					return false;
			}

			return true;
		}

		private void addAllDecidedDaysOffForTeam(ISchedulePartModifyAndRollbackService rollbackService,
		                                 ISchedulingOptions schedulingOptions, ITeamInfo teamInfo,
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
	}
}